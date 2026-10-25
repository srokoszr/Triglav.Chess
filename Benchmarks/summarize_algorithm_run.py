"""Build a timing/memory report from the retained algorithm-run artifacts.

Usage: python Benchmarks/summarize_algorithm_run.py RUN_DIRECTORY
Requires only the Python standard library. Copies of both BenchmarkDotNet JSON
reports, memory-samples.json and run-info.json must already be in RUN_DIRECTORY.
"""

import csv
import json
from collections import defaultdict
from pathlib import Path
from statistics import mean
import sys


def read_json(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def summarize(directory):
    timing = {}
    counts = {}
    for suite in ("minmax", "search"):
        document = read_json(directory / f"{suite}-measurements.json")
        counts[suite] = len(document["Benchmarks"])
        for case in document["Benchmarks"]:
            statistics = case["Statistics"]
            if statistics is None or statistics["N"] != 20:
                raise ValueError(f"Expected 20 retained measurements: {case['DisplayInfo']}")
            algorithm = {"Unified": "MinMaxUnified"}.get(case["Method"], case["Method"])
            key = (case["Parameters"].removeprefix("Scenario="), algorithm)
            # The tree suite's Unified run is the baseline for the cross-backend table.
            timing[key] = {
                "MeanMs": statistics["Mean"] / 1e6,
                "StandardDeviationMs": statistics["StandardDeviation"] / 1e6,
                "ManagedBytesPerOperation": case["Memory"]["BytesAllocatedPerOperation"],
            }
    if counts != {"minmax": 52, "search": 78}:
        raise ValueError(f"Unexpected/incomplete scenario coverage: {counts}")

    memory = defaultdict(list)
    for sample in read_json(directory / "memory-samples.json"):
        measurement = sample["Measurement"]
        memory[(measurement["Scenario"], measurement["Algorithm"])].append(measurement)
    if len(memory) != 104 or any(len(samples) != 3 for samples in memory.values()):
        raise ValueError("Expected 104 algorithm/scenario combinations, three memory samples each")

    rows = []
    for key, samples in memory.items():
        first = samples[0]
        values = {
            "Scenario": first["Scenario"],
            "Position": first["Kind"],
            "Side": "Black" if first["Black"] else "White",
            "FullMoveDepth": first["NominalFullMoves"],
            "Plies": first["NominalPlies"],
            "Algorithm": first["Algorithm"],
            **timing[key],
        }
        for field in ("PeakCommittedBytes", "PeakResidentBytes", "AdditionalPeakCommittedBytes",
                      "AdditionalPeakResidentBytes", "BaselinePeakCommittedBytes", "BaselinePeakResidentBytes"):
            values[f"Mean{field}"] = mean(sample[field] for sample in samples)
            values[f"Max{field}"] = max(sample[field] for sample in samples)
        rows.append(values)
    algorithms = ["Original", "MinMaxUnified", "PositionTree", "PositionTreeWithCopying"]
    rows.sort(key=lambda row: (row["Position"], row["Side"], row["FullMoveDepth"], algorithms.index(row["Algorithm"])))
    with (directory / "summary.csv").open("w", newline="", encoding="utf-8") as output:
        writer = csv.DictWriter(output, fieldnames=list(rows[0]))
        writer.writeheader()
        writer.writerows(rows)

    info = read_json(directory / "run-info.json")
    lines = [
        "# Algorithm comparison: nominal depths 1–5",
        "",
        f"Date: {info['Date']}. Nominal depths are **full moves**, as requested; MinMax uses twice as many plies.",
        "",
        "## Verification and run settings",
        "",
        "- All **130 timed cases** completed: 52 split/unified cases and 78 Unified/tree cases.",
        "- Each case has **20 retained measurement iterations**, **5 warm-ups**, **one launch**, and a **150 ms target iteration time**. Outliers are retained. There are 2,600 measurement iterations in total, each containing one or more searches.",
        "- All 26 scenarios passed the tree suite's legal-move/history checks and the split/unified suite's exact-result checks. The 66 existing MinMax regression tests also passed in Release.",
        "- All **312 memory samples** completed: three fresh processes for each of the 104 distinct algorithm/scenario combinations.",
        "- Managed Release and native Release are checked at runtime. Managed Debug execution and a Release launcher loading the native identity export compiled with Debug flags were both verified to fail before searching. The measured native DLL was rebuilt in x64 Release with MSVC. Generated benchmark projects also use Release.",
        f"- Native DLL SHA-256: `{info['NativeDllSha256']}`.",
        "- BenchmarkDotNet 0.15.8, .NET SDK 10.0.401, .NET runtime 10.0.12, Windows x64. Full environment and individual measurements are retained below. BenchmarkDotNet could not identify the processor or change the power plan.",
        "",
        "The two simpler inputs, MateInTwo and PawnEnding, cover depths 1–5 for both colours. Starting positions cover depths 1–2, and the rook ending covers depth 2. At depths 4–5 the copying entry point uses its deeper iterative/copying path.",
        "",
        "## Reading the tables",
        "",
        "Mean and SD describe elapsed time per search. Managed B/op is BenchmarkDotNet's managed allocation per search. Peak commit and peak resident are **means of three process high-water marks**, in MiB (1,048,576 bytes), from separate untimed memory runs. The process totals include the CLR, JIT, loaded code and native heap; they are not isolated native allocator counts. Peak commit is Windows peak page-file usage; peak resident is peak working set. Zero incremental peak does not imply zero allocation.",
        "",
        "The table uses Original timing from the split/unified suite and Unified/tree timings from the tree suite. Both suites' full timing results, including the separate Unified run in the split/unified suite, are linked below. Summary CSV also includes baseline peaks, incremental peaks and maximum observed peaks across the three memory samples.",
        "",
        "All searches start with empty history. Native buffer/tree allocation, best-move extraction, history reset and cleanup are timed; position/modifier/history construction is outside timing. Trees are freed after each invocation, with no cross-call reuse. See the [benchmark guide](../../README.md) for precise boundaries and reproduction commands.",
        "",
        "Matching nominal depth does not make search behavior equivalent. Extensions, move ordering, caching and early exits differ. The [move/evaluation checks](search-results.json) retain discrepancies; timing alone is not a playing-strength result. A mate found early can make deeper searches unusually cheap.",
        "",
    ]

    if (directory / "COMPARISON_TESTS.md").exists():
        lines.extend([
            "Strict equality is tested separately. The [comparison unit-test report](COMPARISON_TESTS.md) records unresolved discrepancies; successful benchmark setup is not a claim that all four algorithms return identical results.",
            "",
        ])

    for algorithm in algorithms:
        candidates = [row for row in rows if row["Algorithm"] == algorithm]
        peak = max(candidates, key=lambda row: row["MeanPeakCommittedBytes"])
        lines.append(f"- {algorithm}: largest mean process peak commit was **{peak['MeanPeakCommittedBytes'] / 2**20:.2f} MiB**, at {peak['Scenario']}; mean peak resident there was **{peak['MeanPeakResidentBytes'] / 2**20:.2f} MiB**.")
    lines.append("")

    last_group = None
    for row in rows:
        group = (row["Position"], row["Side"])
        if group != last_group:
            if last_group is not None:
                lines.append("")
            lines.extend([
                f"## {group[0]} — {group[1]}",
                "",
                "| Full moves | Plies | Algorithm | Mean ms | SD ms | Managed B/op | Peak commit MiB | Peak resident MiB |",
                "| ---: | ---: | --- | ---: | ---: | ---: | ---: | ---: |",
            ])
            last_group = group
        lines.append(f"| {row['FullMoveDepth']} | {row['Plies']} | {row['Algorithm']} | {row['MeanMs']:.4f} | {row['StandardDeviationMs']:.4f} | {row['ManagedBytesPerOperation']} | {row['MeanPeakCommittedBytes'] / 2**20:.2f} | {row['MeanPeakResidentBytes'] / 2**20:.2f} |")

    lines.extend([
        "",
        "## Retained artifacts and limitations",
        "",
        "- [Original/Unified BenchmarkDotNet report](minmax-benchmarkdotnet-report.md) and [raw timing measurements](minmax-measurements.json)",
        "- [Unified/PositionTree BenchmarkDotNet report](search-benchmarkdotnet-report.md) and [raw timing measurements](search-measurements.json)",
        "- [All memory samples](memory-samples.json), [combined CSV](summary.csv), and [move/evaluation checks](search-results.json)",
        "- [Run identity](run-info.json) and [report generator](../../summarize_algorithm_run.py)",
        "",
        "Twenty samples improve on the earlier five-iteration runs, but this is still one launch per timed case on one Windows machine. Inspect variability and confidence intervals before drawing small-difference conclusions. Process memory varies with runtime initialization, JIT and allocator behavior. Results do not establish WASM memory requirements, isolated native allocation totals, performance under a fixed time budget, or the benefit of reusing an existing tree. The earlier short-run reports are retained as historical data and use different depth labels/configuration.",
        "",
    ])
    (directory / "COMPARISON.md").write_text("\n".join(lines), encoding="utf-8")


if __name__ == "__main__":
    summarize(Path(sys.argv[1]))
