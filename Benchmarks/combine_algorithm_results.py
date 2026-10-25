"""Regenerate the consolidated Markdown/CSV reports from retained results.

Run from any directory: python Benchmarks/combine_algorithm_results.py
Only the Python standard library is required. No benchmarks are rerun.
"""

import csv
import json
import math
import re
import xml.etree.ElementTree as ET
from collections import Counter, defaultdict
from pathlib import Path


ROOT = Path(__file__).resolve().parent / "Results"
ALGORITHMS = ["Original", "MinMaxUnified", "PositionTree", "PositionTreeWithCopying"]


def read_json(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def write_csv(path, rows):
    with path.open("w", newline="", encoding="utf-8") as output:
        writer = csv.DictWriter(output, fieldnames=list(rows[0]))
        writer.writeheader()
        writer.writerows(rows)


def main():
    current = []
    for dataset in ("AlgorithmDepths1To5", "Rc5GamePositions"):
        with (ROOT / dataset / "summary.csv").open(encoding="utf-8-sig") as source:
            for row in csv.DictReader(source):
                current.append({
                    "Dataset": dataset,
                    "Scenario": row["Scenario"],
                    "FullMoveDepth": int(row["FullMoveDepth"]),
                    "Plies": int(row["Plies"]),
                    "SourceGamePly": int(row.get("SourceGamePly", 0)),
                    "HistoryPositions": int(row.get("HistoryPositions", 0)),
                    "Algorithm": row["Algorithm"],
                    "MeanMs": float(row["MeanMs"]),
                    "StandardDeviationMs": float(row["StandardDeviationMs"]),
                    "ManagedBytesPerOperation": float(row["ManagedBytesPerOperation"]),
                    "MeanPeakCommittedMiB": float(row["MeanPeakCommittedBytes"]) / 2**20,
                    "MeanPeakResidentMiB": float(row["MeanPeakResidentBytes"]) / 2**20,
                })
    assert len(current) == 128
    groups = defaultdict(dict)
    for row in current:
        assert row["Plies"] == row["FullMoveDepth"] * 2
        assert row["Algorithm"] not in groups[row["Scenario"]]
        groups[row["Scenario"]][row["Algorithm"]] = row
    assert len(groups) == 32
    assert all(set(group) == set(ALGORITHMS) for group in groups.values())
    identities = [read_json(ROOT / name / "run-info.json") for name in ("AlgorithmDepths1To5", "Rc5GamePositions")]
    assert identities[0]["NativeDllSha256"] == identities[1]["NativeDllSha256"]

    namespace = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
    trx = ET.parse(ROOT / "Rc5GamePositions" / "comparison-tests.trx").getroot()
    mismatches = Counter()
    outcomes = {}
    for result in trx.findall(".//t:UnitTestResult", namespace):
        if not result.attrib["testName"].startswith("AllAlgorithmsReturnTheSameResult"):
            continue
        scenario = result.attrib["testName"].split("(", 1)[1].rstrip(")")
        outcomes[scenario] = result.attrib["outcome"]
        message = result.find("t:Output/t:ErrorInfo/t:Message", namespace)
        if message is not None:
            mismatches.update(set(re.findall(r"^(.+ vs .+): [\w.]+:", message.text, re.MULTILINE)))
    assert set(outcomes) == set(groups)

    raw = []
    sources = [
        ("MinMaxUnified", "measurements.json", True),
        ("MinMaxUnifiedVsPositionTree", "measurements.json", True),
        ("AlgorithmDepths1To5", "minmax-measurements.json", False),
        ("AlgorithmDepths1To5", "search-measurements.json", False),
        ("Rc5GamePositions", "minmax-measurements.json", False),
        ("Rc5GamePositions", "search-measurements.json", False),
    ]
    for dataset, filename, historical in sources:
        for benchmark in read_json(ROOT / dataset / filename)["Benchmarks"]:
            scenario = re.search(r"\(Scenario: (.*)\)$", benchmark["FullName"]).group(1)
            algorithm = {"Unified": "MinMaxUnified"}.get(benchmark["Method"], benchmark["Method"])
            depth = int(re.search(r"D(\d+)$", scenario).group(1))
            statistics = benchmark["Statistics"]
            assert statistics is not None
            raw.append({
                "Dataset": dataset,
                "TimingSuite": filename,
                "HistoricalShortRun": historical,
                "Scenario": scenario,
                "FullMoveDepth": depth / 2 if historical else depth,
                "Plies": depth if historical else depth * 2,
                "Algorithm": algorithm,
                "RetainedMeasurements": statistics["N"],
                "MeanMs": statistics["Mean"] / 1e6,
                "StandardDeviationMs": statistics["StandardDeviation"] / 1e6,
                "ManagedBytesPerOperation": benchmark["Memory"]["BytesAllocatedPerOperation"],
            })
    assert len(raw) == 210
    assert all(row["RetainedMeasurements"] == 20 for row in raw if not row["HistoricalShortRun"])
    write_csv(ROOT / "algorithm-comparison.csv", current)
    write_csv(ROOT / "all-timing-results.csv", raw)

    overview = []
    for algorithm in ALGORITHMS:
        rows = [group[algorithm] for group in groups.values()]
        relative = math.exp(sum(math.log(group[algorithm]["MeanMs"] / group["Original"]["MeanMs"]) for group in groups.values()) / len(groups))
        wins = sum(group[algorithm]["MeanMs"] == min(row["MeanMs"] for row in group.values()) for group in groups.values())
        matching = 32 if algorithm == "Original" else 32 - mismatches[f"Original vs {algorithm}"]
        committed = [row["MeanPeakCommittedMiB"] for row in rows]
        resident = [row["MeanPeakResidentMiB"] for row in rows]
        managed = [row["ManagedBytesPerOperation"] for row in rows]
        overview.append(f"| {algorithm} | {relative:.3f}× | {wins}/32 | {min(committed):.2f}–{max(committed):.2f} | {min(resident):.2f}–{max(resident):.2f} | {min(managed):.0f}–{max(managed):.0f} | {matching}/32 |")

    lines = [
        "# Consolidated algorithm comparison",
        "",
        "Prepared: 2026-10-18. This combines all retained runs without rerunning searches or averaging separate datasets together.",
        "",
        "## Current algorithm overview",
        "",
        "The current matrix contains **32 scenarios**, **four algorithms**, **160 timed benchmark cases**, and **384 isolated memory samples**. Unified was independently measured in both timing suites; the main table uses its tree-suite timing, with the extra split/unified measurements preserved below. The two current datasets share the same native DLL identity, Release mode, 20 retained measurement iterations, five warm-ups, one launch, 150 ms target iteration time and no outlier removal.",
        "",
        "Relative time is the geometric mean of each scenario's mean time divided by Original MinMax's mean time, with all 32 scenarios weighted equally. **Lower is faster**; it is not an overall mean in milliseconds, a confidence interval or a playing-strength ranking. Lowest-mean counts do not establish statistical significance. Memory ranges are the minimum/maximum **three-sample mean process peaks** across scenarios, in MiB; managed allocation is bytes per operation.",
        "",
        "| Algorithm | Relative time vs Original | Lowest mean | Peak commit range MiB | Peak resident range MiB | Managed B/op range | Exact matches to Original |",
        "| --- | ---: | ---: | ---: | ---: | ---: | ---: |",
        *overview,
        "",
        "Original and Unified match exactly in all 32 strict equality tests. All four algorithms match in only **2/32** scenarios; **30/32** tests fail and remain enabled. Differing raw evaluation fields or selected moves count as discrepancies, including alternative moves or terminal-value/distance differences. Algorithms have not been changed. A lower time does not imply equivalent search results.",
        "",
        "Process peaks include CLR/JIT, loaded code, fixtures and native heap. Rc5 memory baselines include PGN parsing and preserved game history, while synthetic cases start with empty history. Peak committed and resident memory are different measurements; neither is an isolated native allocation count or WASM requirement. See the [measurement definitions](../README.md#timing-and-memory).",
        "",
        f"Current native DLL SHA-256: `{identities[0]['NativeDllSha256']}`.",
        "",
        "## Mean search times for every current scenario",
        "",
        "All times are milliseconds per search; the lowest mean in each row is bold. Nominal depth is **full moves** and the ply column is twice that value. The Original timing is from the split/unified suite, and Unified/direct/copying timings are from the tree suite. No measurements are pooled across suites. The CSV retains standard deviations and allocation data for all 128 distinct algorithm/scenario combinations.",
        "",
        "| Scenario | Full moves | Plies | Prior positions | Original ms | Unified ms | Tree ms | Copying tree ms | Equality test |",
        "| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |",
    ]
    for scenario in sorted(groups):
        group = groups[scenario]
        source = group["Original"]
        fastest = min(row["MeanMs"] for row in group.values())
        values = [f"**{group[a]['MeanMs']:.4f}**" if group[a]["MeanMs"] == fastest else f"{group[a]['MeanMs']:.4f}" for a in ALGORITHMS]
        lines.append(f"| {scenario} | {source['FullMoveDepth']} | {source['Plies']} | {source['HistoryPositions']} | " + " | ".join(values) + f" | {outcomes[scenario]} |")

    lines.extend([
        "",
        "## Memory for every current scenario",
        "",
        "Cells show **committed / resident MiB**, each the mean of three fresh-process peak measurements. These are process totals, not allocations per search. Baseline/incremental peaks remain available in the underlying dataset CSV/JSON.",
        "",
        "| Scenario | Original MiB | Unified MiB | Tree MiB | Copying tree MiB |",
        "| --- | ---: | ---: | ---: | ---: |",
    ])
    for scenario in sorted(groups):
        values = [f"{groups[scenario][a]['MeanPeakCommittedMiB']:.2f} / {groups[scenario][a]['MeanPeakResidentMiB']:.2f}" for a in ALGORITHMS]
        lines.append(f"| {scenario} | " + " | ".join(values) + " |")

    lines.extend(["", "## Exact result comparison", "", "| Pair | Matching scenarios | Differing scenarios |", "| --- | ---: | ---: |"])
    for index, left in enumerate(ALGORITHMS):
        for right in ALGORITHMS[index + 1:]:
            differing = mismatches[f"{left} vs {right}"]
            lines.append(f"| {left} vs {right} | {32 - differing}/32 | {differing}/32 |")
    lines.extend([
        "",
        "The passing all-four cases are `MateInTwoWhiteD2` and `MateInTwoWhiteD3`. Four Rc5 fixture/history tests also pass. The earlier 66-test MinMax regression selection passed separately; it is not an assertion of four-backend equivalence. Full comparison diagnostics are retained in the [original comparison report](AlgorithmDepths1To5/COMPARISON_TESTS.md) and the [Rc5 report](Rc5GamePositions/COMPARISON.md).",
        "",
        "## Independent Unified measurements retained",
        "",
        "Unified ran in both current suites. The table below exposes both means instead of silently averaging them. Different launches/times and surrounding workloads can produce different timings even with the same binary and scenario.",
        "",
        "| Scenario | Split/unified suite ms | Tree suite ms |",
        "| --- | ---: | ---: |",
    ])
    for scenario in sorted(groups):
        measured = [row for row in raw if not row["HistoricalShortRun"] and row["Scenario"] == scenario and row["Algorithm"] == "MinMaxUnified"]
        assert len(measured) == 2
        first = next(row for row in measured if row["TimingSuite"] == "minmax-measurements.json")
        second = next(row for row in measured if row["TimingSuite"] == "search-measurements.json")
        lines.append(f"| {scenario} | {first['MeanMs']:.4f} | {second['MeanMs']:.4f} |")

    lines.extend([
        "",
        "## Earlier short runs: retained historical evidence",
        "",
        "The following 50 benchmark cases used five requested measurement iterations, two warm-ups and outlier removal (some retained only four measurements). Native DLL identities and workloads differ from the current run. They are **not pooled** into the current comparison. Historical `D` labels count plies, whereas the current `D` labels count full moves. Their source reports did not measure peak process memory; zero managed bytes must not be read as zero native usage.",
    ])
    for dataset in ("MinMaxUnified", "MinMaxUnifiedVsPositionTree"):
        selected = [row for row in raw if row["Dataset"] == dataset]
        methods = [algorithm for algorithm in ALGORITHMS if any(row["Algorithm"] == algorithm for row in selected)]
        lines.extend(["", f"### {dataset}", "", "| Historical scenario | Full moves | Plies | " + " | ".join(f"{a} mean ms" for a in methods) + " |", "| --- | ---: | ---: | " + " | ".join("---:" for _ in methods) + " |"])
        for scenario in sorted({row["Scenario"] for row in selected}):
            group = {row["Algorithm"]: row for row in selected if row["Scenario"] == scenario}
            first = next(iter(group.values()))
            lines.append(f"| {scenario} | {first['FullMoveDepth']:g} | {first['Plies']} | " + " | ".join(f"{group[a]['MeanMs']:.4f}" for a in methods) + " |")
        lines.extend(["", f"[Original report, uncertainty and limitations]({dataset}/COMPARISON.md). Peak process memory: **not measured** in this run."])

    lines.extend([
        "",
        "## Data and reproduction",
        "",
        "- [Current 128-row timing/memory CSV](algorithm-comparison.csv)",
        "- [All 210 individual timing cases across six runs](all-timing-results.csv), preserving retained sample counts, standard deviations, managed allocation and source dataset",
        "- [Original depth 1–5 dataset](AlgorithmDepths1To5/COMPARISON.md) and [Rc5 dataset](Rc5GamePositions/COMPARISON.md), including their raw measurements",
        "- [Benchmark guide](../README.md) and [consolidation script](../combine_algorithm_results.py)",
        "",
        "Regenerate without rerunning benchmarks: `python Benchmarks/combine_algorithm_results.py`. The 26-scenario and 32-scenario test reports overlap; they are not independent cohorts to add together. This report preserves their current 32-scenario result once. No runtime or algorithm changes were made to produce these tables.",
        "",
    ])
    (ROOT / "ALGORITHM_COMPARISON.md").write_text("\n".join(lines), encoding="utf-8")
    print("\n".join(overview))
    print(f"Verified {len(groups)} current scenarios, {len(current)} combined rows and {len(raw)} individual timing cases.")


if __name__ == "__main__":
    main()
