"""Compare a completed consolidated benchmark with the retained FullDepthRerun."""

import argparse
import csv
import json
import math
from collections import Counter
from pathlib import Path
from statistics import mean, median


ALGORITHM_MAP = {
    "MinMax": "MinMaxUnified",
    "PositionTree": "PositionTree",
    "PositionTreeWithCopying": "PositionTreeWithCopying",
}
RESULT_FIELDS = ("Move", "Piece", "ResultCode", "InNMoves", "EvaluationValue")


def load(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def load_cases(root):
    return [load(path) for path in sorted((root / "cases").glob("*/case.json"))]


def geometric_mean(values):
    return math.exp(mean(math.log(value) for value in values))


def memory_mean(case, field):
    samples = case.get("MemorySamples", [])
    return mean(sample[field] for sample in samples) if len(samples) == 3 else None


def signature(case):
    result = case.get("Result") or {}
    return tuple(result.get(field) for field in RESULT_FIELDS)


def format_ratio(value):
    return f"{value:.3f}×" if value is not None else "—"


def compare(old_root, new_root, output):
    old_info = load(old_root / "run-info.json")
    new_info = load(new_root / "run-info.json")
    old_catalog = load(old_root / "scenarios.json")
    new_catalog = load(new_root / "scenarios.json")
    old_cases = load_cases(old_root)
    new_cases = load_cases(new_root)

    expected_new_cases = new_info["TimedCases"]
    complete_new = [case for case in new_cases if case.get("Status") == "Completed"]
    if len(new_cases) != expected_new_cases or len(complete_new) != expected_new_cases:
        raise RuntimeError(
            f"New run is incomplete: {len(complete_new)}/{expected_new_cases} completed cases."
        )
    if new_info.get("MeasurementIterations") != 16 or new_info.get("WarmupIterations") != 1:
        raise RuntimeError("New run does not use the requested 16-measurement/one-warm-up policy.")
    if any(len(case.get("MemorySamples", [])) != 3 for case in complete_new):
        raise RuntimeError("At least one new case does not have three memory samples.")

    old_by_key = {
        (case["Scenario"], case["Algorithm"]): case
        for case in old_cases
        if case.get("Status") == "Completed"
    }
    new_by_key = {(case["Scenario"], case["Algorithm"]): case for case in complete_new}
    rows = []
    for (scenario, new_algorithm), new_case in sorted(new_by_key.items()):
        old_algorithm = ALGORITHM_MAP[new_algorithm]
        old_case = old_by_key.get((scenario, old_algorithm))
        if old_case is None:
            continue
        old_mean = old_case["Timing"]["MeanMs"]
        new_mean = new_case["Timing"]["MeanMs"]
        old_committed = memory_mean(old_case, "PeakCommittedBytes")
        new_committed = memory_mean(new_case, "PeakCommittedBytes")
        old_resident = memory_mean(old_case, "PeakResidentBytes")
        new_resident = memory_mean(new_case, "PeakResidentBytes")
        rows.append({
            "Scenario": scenario,
            "NewAlgorithm": new_algorithm,
            "OldAlgorithm": old_algorithm,
            "OldMeanMs": old_mean,
            "NewMeanMs": new_mean,
            "SpeedupOldOverNew": old_mean / new_mean,
            "OldPeakCommittedMiB": old_committed / 2**20 if old_committed is not None else None,
            "NewPeakCommittedMiB": new_committed / 2**20 if new_committed is not None else None,
            "CommittedRatioNewOverOld": new_committed / old_committed if old_committed and new_committed else None,
            "OldPeakResidentMiB": old_resident / 2**20 if old_resident is not None else None,
            "NewPeakResidentMiB": new_resident / 2**20 if new_resident is not None else None,
            "ResidentRatioNewOverOld": new_resident / old_resident if old_resident and new_resident else None,
            "ExactResultMatch": signature(old_case) == signature(new_case),
        })

    output.parent.mkdir(parents=True, exist_ok=True)
    csv_path = output.with_suffix(".csv")
    with csv_path.open("w", newline="", encoding="utf-8") as stream:
        writer = csv.DictWriter(stream, fieldnames=list(rows[0]))
        writer.writeheader()
        writer.writerows(rows)

    old_names = {scenario["Scenario"] for scenario in old_catalog}
    new_names = {scenario["Scenario"] for scenario in new_catalog}
    removed = sorted(old_names - new_names)
    added = sorted(new_names - old_names)
    old_statuses = Counter(case.get("Status", "Missing") for case in old_cases)
    new_statuses = Counter(case.get("Status", "Missing") for case in new_cases)

    lines = [
        "# MinMaxConsolidated compared with FullDepthRerun",
        "",
        "This report compares only the same scenario and corresponding backend where both runs have a completed timing and all three memory probes. It does not treat the older stopped run as complete.",
        "",
        "## What changed between runs",
        "",
        "| Property | FullDepthRerun | MinMaxConsolidated |",
        "| --- | ---: | ---: |",
        f"| Run date | {old_info['Date']} | {new_info['Date']} |",
        f"| Scenario/depth combinations | {old_info['Scenarios']} | {new_info['Scenarios']} |",
        f"| Algorithms | {len(old_info['Algorithms'])} | {len(new_info['Algorithms'])} |",
        f"| Requested timing cases | {old_info['TimedCases']} | {new_info['TimedCases']} |",
        f"| Completed timing/memory cases | {old_statuses['Completed']} | {new_statuses['Completed']} |",
        f"| Measurement iterations | {old_info['MeasurementIterations']} | {new_info['MeasurementIterations']} |",
        f"| Warm-up iterations | {old_info['WarmupIterations']} | {new_info['WarmupIterations']} |",
        f"| Search policy metadata | {old_info.get('SearchPolicy', 'not recorded')} | {new_info.get('SearchPolicy', 'not recorded')} |",
        f"| Native DLL SHA-256 | `{old_info['NativeDllSha256']}` | `{new_info['NativeDllSha256']}` |",
        "",
        "The new `MinMax` is the implementation previously named `MinMaxUnified`, so it is compared with that old row rather than with the removed old `Original` implementation. The two PositionTree backends are paired by name.",
        "",
        f"The new matrix removes **{len(removed)}** old scenarios and adds **{len(added)}**. The removed scenarios are the artificial Black mirrors of `MateInTwo`, `PawnEnding`, and `RookEnding`; both starting-position sides and the natural Rc5 sides remain."
        if not added else f"The new matrix removes **{len(removed)}** scenarios and adds **{len(added)}**.",
        "",
        "## Matched performance",
        "",
        "Speedup is `old mean / new mean`; values above 1 mean the new run was faster. Memory ratios are `new / old`; values below 1 mean a lower observed process peak.",
        "",
        "| New backend (old counterpart) | Matched cases | Geometric-mean speedup | Median speedup | New faster | Exact result matches | Committed-memory ratio | Resident-memory ratio |",
        "| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |",
    ]
    summaries = {}
    for new_algorithm, old_algorithm in ALGORITHM_MAP.items():
        group = [row for row in rows if row["NewAlgorithm"] == new_algorithm]
        speeds = [row["SpeedupOldOverNew"] for row in group]
        committed = [row["CommittedRatioNewOverOld"] for row in group if row["CommittedRatioNewOverOld"] is not None]
        resident = [row["ResidentRatioNewOverOld"] for row in group if row["ResidentRatioNewOverOld"] is not None]
        summary = {
            "MatchedCases": len(group),
            "GeometricMeanSpeedup": geometric_mean(speeds),
            "MedianSpeedup": median(speeds),
            "NewFasterCases": sum(value > 1 for value in speeds),
            "ExactResultMatches": sum(row["ExactResultMatch"] for row in group),
            "CommittedMemoryRatio": geometric_mean(committed) if committed else None,
            "ResidentMemoryRatio": geometric_mean(resident) if resident else None,
        }
        summaries[new_algorithm] = summary
        lines.append(
            f"| {new_algorithm} ({old_algorithm}) | {summary['MatchedCases']} | "
            f"{format_ratio(summary['GeometricMeanSpeedup'])} | {format_ratio(summary['MedianSpeedup'])} | "
            f"{summary['NewFasterCases']}/{summary['MatchedCases']} | "
            f"{summary['ExactResultMatches']}/{summary['MatchedCases']} | "
            f"{format_ratio(summary['CommittedMemoryRatio'])} | {format_ratio(summary['ResidentMemoryRatio'])} |"
        )

    lines += [
        "",
        "## Largest matched timing changes",
        "",
        "These are descriptive single-case mean ratios, not confidence intervals.",
        "",
        "| Direction | Scenario | Backend | Old ms | New ms | Old/new speedup |",
        "| --- | --- | --- | ---: | ---: | ---: |",
    ]
    fastest = sorted(rows, key=lambda row: row["SpeedupOldOverNew"], reverse=True)[:5]
    slowest = sorted(rows, key=lambda row: row["SpeedupOldOverNew"])[:5]
    for direction, selected in (("Largest speedup", fastest), ("Largest slowdown", slowest)):
        for row in selected:
            lines.append(
                f"| {direction} | {row['Scenario']} | {row['NewAlgorithm']} | "
                f"{row['OldMeanMs']:.4f} | {row['NewMeanMs']:.4f} | {row['SpeedupOldOverNew']:.3f}× |"
            )

    lines += [
        "",
        "## How to interpret the differences",
        "",
        "- The comparison is not a controlled before/after measurement of one code edit. Native and managed binary hashes changed, and the old run did not record the new fixed-depth policy identifier.",
        "- The new run uses 16 measurements and one warm-up instead of 20 measurements and five warm-ups. BenchmarkDotNet still chooses invocation counts per iteration, but reduced warm-up can affect JIT/cache stabilization, especially for short cases.",
        "- FullDepthRerun was stopped, so matched-case counts differ by backend and exclude every unfinished old case. No missing old result is extrapolated.",
        "- Exact-result counts identify whether the selected move and raw evaluation fields stayed identical. Timing improvements must not be read as playing-strength improvements when results differ.",
        "- Process-memory values are whole-process peaks. Small differences can reflect CLR/JIT and process startup; large PositionTree differences are more informative than sub-megabyte MinMax fluctuations.",
        "",
        f"Machine-readable matched rows: [{csv_path.name}]({csv_path.name}).",
        "",
    ]
    output.write_text("\n".join(lines), encoding="utf-8")
    json_path = output.with_suffix(".json")
    json_path.write_text(json.dumps({
        "OldRun": old_info,
        "NewRun": new_info,
        "OldStatuses": old_statuses,
        "NewStatuses": new_statuses,
        "RemovedScenarios": removed,
        "AddedScenarios": added,
        "AlgorithmSummaries": summaries,
        "MatchedRows": rows,
    }, indent=2) + "\n", encoding="utf-8")
    print(f"Wrote {output}, {csv_path}, and {json_path}; {len(rows)} matched cases.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--old", type=Path, required=True)
    parser.add_argument("--new", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    compare(args.old.resolve(), args.new.resolve(), args.output.resolve())
