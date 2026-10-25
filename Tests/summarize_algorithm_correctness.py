"""Summarize real MSTest TRX results against the app's copying-tree reference."""

import argparse
from collections import Counter, defaultdict
from datetime import datetime
import hashlib
import json
from pathlib import Path
import xml.etree.ElementTree as ET
from zoneinfo import ZoneInfo

NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
REFERENCE = "PositionTreeWithCopying"
ALGORITHMS = ["Original", "MinMaxUnified", "PositionTree", REFERENCE]
FIELDS = ["Move", "Piece", "ResultCode", "InNMoves", "EvaluationValue"]
REPO = Path(__file__).resolve().parent.parent


def read_results(path):
    document = ET.parse(path)
    tests = document.findall(".//t:UnitTestResult", NS)
    definitions = {}
    for test in document.findall(".//t:UnitTest", NS):
        method = test.find("t:TestMethod", NS)
        definitions[test.attrib["id"]] = method.attrib["className"].split(",")[0] if method is not None else "Unknown"
    cases = {}
    # MSTest can include accumulated class stdout in several test results.
    # Deduplicate by scenario, and reject conflicting snapshots, never count them twice.
    for output in document.findall(".//t:StdOut", NS):
        for line in (output.text or "").splitlines():
            if line.strip().startswith("ALGORITHM_RESULT_JSON "):
                case = json.loads(line.strip().split(" ", 1)[1])
                name = case["Scenario"]
                if name in cases and cases[name] != case:
                    raise ValueError(f"Conflicting snapshots for {name}")
                cases[name] = case
    return tests, definitions, cases


def differs(actual, expected):
    return [field for field in FIELDS if actual[field] != expected[field]]


def generate(trx, destination, partial, regression_trx=None, stopped=False):
    tests, definitions, cases = read_results(trx)
    if regression_trx is not None:
        regression_tests, regression_definitions, regression_cases = read_results(regression_trx)
        if regression_cases:
            raise ValueError("Regression input also contains comparison snapshots; keep the runs separate to avoid double counting.")
        tests.extend(regression_tests)
        definitions.update(regression_definitions)
    counts = Counter(test.attrib["outcome"] for test in tests)
    comparison_tests = [test for test in tests if "BenchmarkScenarioComparisonTests" in definitions.get(test.attrib["testId"], "")]
    now = datetime.now(ZoneInfo("Europe/Warsaw")).isoformat(timespec="seconds")
    status = "COMPLETED focused comparison — partial depth coverage" if partial else "COMPLETED correctness run"
    if stopped:
        status = "STOPPED at the user's request — partial comparison results"
    policies = {case.get("SearchPolicy", "LegacyExtensions") for case in cases.values()}
    if len(policies) > 1:
        raise ValueError("Do not combine legacy and fixed-depth comparisons in one assessment.")
    policy_note = (
        "**Historical results:** these snapshots used the original MinMax depth policy. Current correctness comparisons disable capture/check extensions and the extra two-ply allowance, and reject depths above 5 full moves / 10 plies. These counts have not been rerun under that policy."
        if "LegacyExtensions" in policies else
        "**Fixed-depth policy:** capture/check extensions and the extra MinMax depth allowance are disabled. Comparison searches are limited to 5 full moves / 10 plies."
    )
    lines = ["# Algorithm correctness assessment", "", f"Updated: {now}. **{status}**.", "",
             policy_note, "",
             "This assesses returned results, not speed. Per the user's instruction, the application algorithm is the reference: **PositionTreeWithCopying**, called by `BreadthFirstTreeSearch`. Direct PositionTree is also compared. Production search policies are unchanged; comparison entry points use the policy stated above. Equality assertions have not been relaxed.", "",
             "## Test scope and outcomes", "",
             f"TRX: `{trx.relative_to(REPO).as_posix()}`. Recorded results: **{len(tests)}**, **{counts['Passed']} passed**, **{counts['Failed']} failed**, **{counts['NotExecuted']} skipped**. Other outcomes: {dict(counts - Counter({'Passed': counts['Passed'], 'Failed': counts['Failed'], 'NotExecuted': counts['NotExecuted']})) or 'none'}.", "",
             f"**{len(comparison_tests)} comparison tests executed; {len(cases)}/53 comparison scenarios** have complete four-backend snapshots. All selected comparisons retain strict move/piece/result-code/distance/raw-score checks, legal-move assertions, input preservation and prior-history preservation.", ""]
    if partial:
        if stopped:
            lines += ["These are the completed depth-1–3 comparison results. The unfinished full comparison was stopped at the user's request. Its runner had not flushed a final TRX, so no additional cases from that interrupted run are counted. Deeper-case results remain unknown; no tests are still running.", ""]
        else:
            lines += ["This focused run covers depths 1–3 full moves only. Depths 4–5 were not run, and their results are not implied by this subset. No deeper comparison is running.", ""]
    if regression_trx is not None:
        lines += [f"Regression TRX: `{regression_trx.relative_to(REPO).as_posix()}`. Its results are combined with the isolated comparison run, without rerunning or double-counting the comparison class.", ""]
    lines += ["The initial unrestricted whole-project attempt was stopped during an unrelated unpruned tree-construction stress run, before a final TRX was produced. A second broad attempt was stopped during long legacy smoke execution. The final regression selection excludes three unpruned construction tests: `PositionTreeTests.SimpleTests.StartingPosition_MaxDepth5`, `StartingPosition_MaxDepth6`, and `StartingPosition_MaxDepth8_NoLoop`; it also excludes `MinMaxTests.EndGameTests.FromLysia` and `MinMaxUnifiedTests.EndGameTests.FromLysia`, whose result assertions are entirely commented out. These five exclusions do not remove any of the 53 algorithm result comparisons. Existing `[Ignore]` tests remain ignored. No one should describe this as every test in the repository passing or completing.", "",
              "## Agreement with the app algorithm", "",
              "| Algorithm | Exact matches | Same move and piece | Same result code | Same terminal marker | Same distance | Same raw score |",
              "| --- | ---: | ---: | ---: | ---: | ---: | ---: |"]
    for algorithm in ALGORITHMS[:-1]:
        score = Counter()
        for case in cases.values():
            by_algorithm = {result["Algorithm"]: result for result in case["Results"]}
            actual, expected = by_algorithm[algorithm], by_algorithm[REFERENCE]
            score["Exact"] += not differs(actual, expected)
            score["Move"] += all(actual[field] == expected[field] for field in ("Move", "Piece"))
            score["Result"] += actual["ResultCode"] == expected["ResultCode"]
            score["Terminal"] += (actual["EvaluationValue"] == -32768) == (expected["EvaluationValue"] == -32768)
            score["Distance"] += actual["InNMoves"] == expected["InNMoves"]
            score["Score"] += actual["EvaluationValue"] == expected["EvaluationValue"]
        lines.append(f"| {algorithm} | " + " | ".join(f"{score[key]}/{len(cases)}" for key in ("Exact", "Move", "Result", "Terminal", "Distance", "Score")) + " |")
    parity = sum(not differs(case["Results"][0], case["Results"][1]) for case in cases.values())
    distance_only = sum(
        differs(next(result for result in case["Results"] if result["Algorithm"] == "Original"),
                next(result for result in case["Results"] if result["Algorithm"] == REFERENCE)) == ["InNMoves"]
        for case in cases.values()
    )
    lines += ["", f"Original and Unified MinMax agree exactly in **{parity}/{len(cases)}** scenarios. Agreement between those two implementations alone does not establish agreement with the application.", "",
              f"Original MinMax differs from the application tree **only in Distance in {distance_only}/{len(cases)} scenarios**. These remain strict equality failures, but should be distinguished from differences in move, score, terminal marker or result code.", "",
              "The terminal marker is the native `EvaluationNaN` raw value, **-32768**. `DrawOrUknown` is also the code for an ordinary nonterminal heuristic evaluation: equal result codes alone do not prove equal outcomes. Distance and raw terminal-marker mismatches remain failures; they are not normalized away. A different legal move has not been proven equally good merely because its result code matches.", "",
              "## Source observations to investigate next", "",
              "These are read-only findings, not fixes or proofs that one line explains every mismatch:", "",
              "1. **Legacy depth policy differs from fixed-depth comparisons.** The original production recursion does not decrement `depthToGo` and can terminate at the separate `StartingDepth + 2` limit. Fixed-depth comparison entry points instead stop at the actual requested ply count, disable extensions, and assert the deepest visited ply. Legacy depth handling is not an explanation for discrepancies in a fixed-depth snapshot.",
              "2. **Returned distance uses different conventions.** The nonterminal native evaluator returns `InNMoves = 0`; the tree rewrites propagated nonterminal distances and the application exposes the best child's evaluation. Preserve terminal mate distance and terminal marker while separating this convention from genuine move/score differences.",
              "3. **Terminal handling and initial best values differ.** Plain MinMax returns its heuristic leaf evaluation before checking terminal moves there. Its initial best sentinels use real winner codes with distance zero and raw score zero, and the function returns the accumulated best value. The tree checks terminal conditions before returning a leaf and ends an expanded node with its sorted best child's actual evaluation. Investigate a sentinel surviving the MinMax loop where a winner code arrives with distance zero and a nonterminal raw score; this is a source-level explanation to verify, not a fix already applied.",
              "4. **Repetition policies differ.** Plain MinMax contains a short fixed-offset stack-pattern check, while the tree checks repeated positions near the root. Rc5 comparisons carry the full prior game history; do not replace it with an empty stack to make tests agree.",
              "5. **Copying tree above depth 3 is a separate issue.** Its iterative copying/pruning branch differs from direct build. Use copying tree as the requested reference when a direct-tree result diverges; shallow agreement does not establish deeper agreement.", "",
              "## Per-scenario comparison", "",
              "Each detailed snapshot contains the actual result returned by that run. Full-move depth maps to twice as many input plies for MinMax. Passing legality/history checks do not make a differing score or move correct.", ""]
    for name, case in sorted(cases.items(), key=lambda item: (item[1]["FullMoveDepth"], item[0])):
        expected = next(result for result in case["Results"] if result["Algorithm"] == REFERENCE)
        lines += [f"### {name}", "", f"Depth: {case['FullMoveDepth']} full moves / {case['Plies']} input plies; prior positions: {case['HistoryPositions']}.", "",
                  f"FEN: `{case['Fen']}`", "", "| Algorithm | Move | Piece | Result | Terminal | Distance | Raw score | Differences from app tree |",
                  "| --- | --- | --- | --- | --- | ---: | ---: | --- |"]
        for result in case["Results"]:
            fields = differs(result, expected)
            lines.append(f"| {result['Algorithm']} | {result['Move']} | {result['Piece']} | {result['Result']} | {'yes' if result['EvaluationValue'] == -32768 else 'no'} | {result['InNMoves']} | {result['EvaluationValue']} | {', '.join(fields) or 'none'} |")
        lines.append("")
    lines += ["## Results by test class", "", "| Class | Passed | Failed | Skipped |", "| --- | ---: | ---: | ---: |"]
    classes = defaultdict(Counter)
    for test in tests:
        classes[definitions.get(test.attrib["testId"], "Unknown")][test.attrib["outcome"]] += 1
    for name, outcomes in sorted(classes.items()):
        lines.append(f"| {name.removeprefix('Triglav.Chess.Engine.Tests.')} | {outcomes['Passed']} | {outcomes['Failed']} | {outcomes['NotExecuted']} |")
    noncomparison = [test for test in tests if test.attrib["outcome"] == "Failed" and "BenchmarkScenarioComparisonTests" not in definitions.get(test.attrib["testId"], "")]
    if noncomparison:
        lines += ["", "## Other failing tests", ""]
        for test in noncomparison:
            message = test.findtext("t:Output/t:ErrorInfo/t:Message", default="No message", namespaces=NS).splitlines()[0]
            lines.append(f"- `{test.attrib['testName']}`: {message[:500]}")
    incomplete = [test for test in tests if test.attrib["outcome"] == "Failed"
                  and "BenchmarkScenarioComparisonTests" in definitions.get(test.attrib["testId"], "")
                  and not any(f"({name})" in test.attrib["testName"] for name in cases)]
    if incomplete:
        lines += ["", "## Comparison cases without complete four-backend snapshots", "",
                  "These tests failed before all four result/legality/history checks completed. They are not counted as matches or silently omitted from the failure totals.", ""]
        for test in incomplete:
            message = test.findtext("t:Output/t:ErrorInfo/t:Message", default="No message", namespaces=NS).splitlines()[0]
            lines.append(f"- `{test.attrib['testName']}`: {message[:500]}")
    dll = REPO / "Core/Triglav.Chess.Interop/Triglav.Chess.Native.dll"
    lines += ["", "## Reproduction", "", "Build and run the test project in **Release x64**. No benchmark runner is used. Current local native DLL SHA-256 (not proof of the DLL used by an older TRX): `" + hashlib.sha256(dll.read_bytes()).hexdigest() + "`.", "",
              "The comparison test emits `ALGORITHM_RESULT_JSON` snapshots even on success; the report generator deduplicates repeated class stdout in TRX. All 53 cases remain enabled by default and are ordered by depth to expose early discrepancies first.", "",
              "```powershell", "dotnet build Tests/Triglav.Chess.Engine.Tests/Triglav.Chess.Engine.Tests.csproj -c Release --no-restore",
              "dotnet run --project Tests/Triglav.Chess.Engine.Tests/Triglav.Chess.Engine.Tests.csproj -c Release --no-build -- --filter 'FullyQualifiedName~BenchmarkScenarioComparisonTests' --report-trx --report-trx-filename comparisons.trx --results-directory artifacts/algorithm-consistency --no-progress",
              "python Tests/summarize_algorithm_correctness.py --trx artifacts/algorithm-consistency/comparisons.trx --output docs/ALGORITHM_CORRECTNESS_RESULTS.md", "```", ""]
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_text("\n".join(lines), encoding="utf-8")
    print(f"{len(tests)} test results; {len(cases)}/53 comparison snapshots; report: {destination}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--trx", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--partial", action="store_true")
    parser.add_argument("--stopped", action="store_true")
    parser.add_argument("--regression-trx", type=Path)
    args = parser.parse_args()
    generate(args.trx.resolve(), args.output.resolve(), args.partial or args.stopped,
             args.regression_trx.resolve() if args.regression_trx else None, args.stopped)
