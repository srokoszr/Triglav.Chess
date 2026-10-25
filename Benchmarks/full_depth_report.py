"""Incremental/full report and scientific plots for the uncapped depth experiment."""

import csv
import itertools
import json
import math
import os
import sys
from collections import Counter, defaultdict
from datetime import datetime, timezone
from pathlib import Path
from statistics import mean

DEFAULT_ALGORITHMS = ["MinMax", "PositionTree", "PositionTreeWithCopying"]
RESULT_FIELDS = ["Move", "Piece", "ResultCode", "InNMoves", "EvaluationValue"]
COLORS = {
    "MinMax": "#1765a3", "Original": "#1765a3", "MinMaxUnified": "#00a38c",
    "PositionTree": "#d67b12", "PositionTreeWithCopying": "#8c4bb6",
}
REPO = Path(__file__).resolve().parent.parent


def load(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def signature(result):
    return tuple(result.get(field) for field in RESULT_FIELDS)


def plots(root, rows, catalog, final, algorithms):
    dependency = REPO / "artifacts/benchmark_plot_dependencies"
    sys.path.insert(0, str(dependency))
    os.environ.setdefault("MPLCONFIGDIR", str(REPO / "artifacts/matplotlib-cache"))
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    import numpy as np

    plt.rcParams.update({"font.size": 9, "axes.spines.top": False, "axes.spines.right": False})
    destination = root / "charts"
    destination.mkdir(exist_ok=True)
    total_cases = len(catalog) * len(algorithms)
    label = "Complete run" if final else f"Provisional: {len(rows)}/{total_cases} completed algorithm/scenario cases"

    def save(figure, name):
        figure.savefig(destination / f"{name}.png", dpi=160, bbox_inches="tight")
        figure.savefig(destination / f"{name}.svg", bbox_inches="tight")
        plt.close(figure)

    root_names = sorted({(s["Kind"], s["Side"]) for s in catalog})
    figure, axes = plt.subplots(4, 3, figsize=(17, 17), squeeze=False)
    projections = []
    for axis, (kind, side) in zip(axes.flat, root_names):
        for algorithm in algorithms:
            group = sorted([r for r in rows if r["Kind"] == kind and r["Side"] == side and r["Algorithm"] == algorithm], key=lambda r: r["FullMoveDepth"])
            if not group:
                continue
            x = np.array([r["FullMoveDepth"] for r in group], float)
            y = np.array([r["MeanMs"] for r in group], float)
            sd = np.array([r["StandardDeviationMs"] for r in group], float)
            axis.plot(x, y, "o-", color=COLORS[algorithm], label=algorithm, markersize=4)
            axis.fill_between(x, np.maximum(y - sd, y / 100), y + sd, color=COLORS[algorithm], alpha=.10)
            required = 4 if kind == "Start" else 5
            if len(group) >= 3 and max(x) == required:
                coefficient = np.polyfit(x, np.log10(y), 1)
                fitted = np.polyval(coefficient, x)
                total = np.sum((np.log10(y) - np.mean(np.log10(y)))**2)
                r2 = 1 - np.sum((np.log10(y) - fitted)**2) / total if total > 1e-12 else 0
                # Never present a low-quality fit as an extrapolated measurement.
                if r2 >= .6:
                    next_depth = required + 1
                    estimated = float(10**np.clip(np.polyval(coefficient, next_depth), -6, 15))
                    axis.plot([required, next_depth], [y[-1], estimated], "--", color=COLORS[algorithm], alpha=.7)
                    axis.plot(next_depth, estimated, marker="o", markerfacecolor="none", color=COLORS[algorithm], markersize=5)
                    projections.append({"Kind": kind, "Side": side, "Algorithm": algorithm, "PredictedFullMoveDepth": next_depth, "PredictedMs": estimated, "LogLinearR2": float(r2), "Measured": False})
        axis.set_title(f"{kind} / {side}")
        if not any(r["Kind"] == kind and r["Side"] == side for r in rows):
            axis.text(.5, .5, "Pending measurements", transform=axis.transAxes, ha="center", color="#777777")
        axis.set_yscale("log")
        axis.set_xlabel("Nominal full-move depth")
        axis.set_ylabel("Mean search time (ms, log scale)")
        axis.grid(alpha=.2)
        axis.set_xticks(range(1, 7))
    for axis in list(axes.flat)[len(root_names):]:
        axis.set_visible(False)
    handles = [plt.Line2D([], [], color=COLORS[a], marker="o", label=a) for a in algorithms]
    figure.legend(handles=handles, loc="upper center", ncol=len(algorithms), bbox_to_anchor=(.5, .952))
    figure.suptitle(f"Depth scaling — {label}\nSolid: measured means; shading: ±1 SD; dashed/open: one-step log-linear extrapolation (R² ≥ 0.6)", y=.995)
    figure.tight_layout(rect=(0, 0, 1, .93))
    save(figure, "depth-scaling")
    (root / "extrapolations.json").write_text(json.dumps(projections, indent=2) + "\n", encoding="utf-8")

    for field, name, xlabel in [("PieceCount", "time-vs-pieces", "Pieces on the root board"), ("LegalMoveCount", "time-vs-legal-moves", "Legal moves at the root")]:
        figure, axes = plt.subplots(2, 3, figsize=(17, 9), squeeze=False)
        for depth, axis in enumerate(list(axes.flat)[:5], 1):
            for algorithm in algorithms:
                group = [r for r in rows if r["FullMoveDepth"] == depth and r["Algorithm"] == algorithm]
                if not group:
                    continue
                x = np.array([r[field] for r in group], float)
                y = np.array([r["MeanMs"] for r in group], float)
                axis.scatter(x, y, color=COLORS[algorithm], label=algorithm, s=28, alpha=.8)
                # Fits are descriptive and remain inside the measured x range.
                if len(group) >= 5 and len(set(x)) >= 3:
                    fit = np.polyfit(x, np.log10(y), 1)
                    grid = np.linspace(min(x), max(x), 100)
                    axis.plot(grid, 10**np.polyval(fit, grid), color=COLORS[algorithm], alpha=.4, linewidth=1)
            axis.set_title(f"Depth {depth} full moves / {depth * 2} plies")
            if not any(r["FullMoveDepth"] == depth for r in rows):
                axis.text(.5, .5, "Pending measurements", transform=axis.transAxes, ha="center", color="#777777")
            axis.set_yscale("log")
            axis.set_xlabel(xlabel)
            axis.set_ylabel("Mean search time (ms, log scale)")
            axis.grid(alpha=.2)
        axes.flat[5].set_visible(False)
        figure.legend(handles=handles, loc="upper center", ncol=len(algorithms), bbox_to_anchor=(.5, .925))
        figure.suptitle(f"{xlabel} vs measured runtime — {label}\nOne point = one root position/side at a fixed depth. Lines are within-range descriptive fits, not causal laws.", y=.995)
        figure.tight_layout(rect=(0, 0, 1, .89))
        save(figure, name)

    figure, axes = plt.subplots(1, 2, figsize=(14, 5))
    for algorithm in algorithms:
        for axis, field, title in [(axes[0], "PeakCommittedMiB", "Peak committed process memory"), (axes[1], "PeakResidentMiB", "Peak resident process memory")]:
            depths, medians, lows, highs = [], [], [], []
            for depth in range(1, 6):
                values = [r[field] for r in rows if r["Algorithm"] == algorithm and r["FullMoveDepth"] == depth]
                if values:
                    depths.append(depth)
                    medians.append(float(np.median(values)))
                    lows.append(min(values))
                    highs.append(max(values))
            if depths:
                axis.plot(depths, medians, "o-", color=COLORS[algorithm], label=algorithm)
                axis.fill_between(depths, lows, highs, color=COLORS[algorithm], alpha=.1)
            axis.set_title(title)
            axis.set_xlabel("Nominal full-move depth")
            axis.set_xticks(range(1, 6))
            axis.set_ylabel("MiB (log scale)")
            axis.set_yscale("log")
            axis.grid(alpha=.2)
    figure.legend(handles=handles, loc="upper center", ncol=len(algorithms), bbox_to_anchor=(.5, .86))
    figure.suptitle(f"Median across positions; shading is min–max of three-sample mean process peaks\n{label}; includes CLR/JIT/fixture/native heap, not isolated native allocation", y=.995)
    figure.tight_layout(rect=(0, 0, 1, .80))
    save(figure, "memory-scaling")

    # Exploratory branching model with root-group holdout, not random-row holdout.
    models = []
    figure, axis = plt.subplots(figsize=(8, 7))
    for algorithm in algorithms:
        group = [r for r in rows if r["Algorithm"] == algorithm]
        roots = {(r["Kind"], r["Side"]) for r in group}
        if len(group) < 12 or len(roots) < 5:
            continue
        x = np.array([[1, r["FullMoveDepth"], r["FullMoveDepth"] * math.log10(r["LegalMoveCount"] + 1), r["PieceCount"]] for r in group], float)
        y = np.log10([r["MeanMs"] for r in group])
        coefficients, _, rank, _ = np.linalg.lstsq(x, y, rcond=None)
        if rank < 4:
            continue
        held_out = np.full(len(group), np.nan)
        for key in roots:
            test = np.array([(r["Kind"], r["Side"]) == key for r in group])
            train = ~test
            beta, _, train_rank, _ = np.linalg.lstsq(x[train], y[train], rcond=None)
            if train_rank == 4:
                held_out[test] = x[test] @ beta
        valid = np.isfinite(held_out)
        if not valid.any():
            continue
        factor = float(10**np.sqrt(np.mean((held_out[valid] - y[valid])**2))) if valid.any() else None
        axis.scatter(10**y[valid], 10**held_out[valid], color=COLORS[algorithm], alpha=.65,
                     label=f"{algorithm}: holdout error ×{factor:.2f}")
        models.append({"Algorithm": algorithm, "Equation": "log10(ms) = b0 + b1*depth + b2*depth*log10(rootLegalMoves+1) + b3*pieces", "Coefficients": coefficients.tolist(), "DistinctRoots": len(roots), "Rows": len(group), "HeldOutRootLogRMSEFactor": factor})
    if models:
        limits = axis.get_xlim(), axis.get_ylim()
        low = max(1e-6, min(limits[0][0], limits[1][0]))
        high = max(limits[0][1], limits[1][1])
        axis.plot([low, high], [low, high], "k--", alpha=.4)
        axis.set_xscale("log")
        axis.set_yscale("log")
        axis.set_xlabel("Measured mean (ms)")
        axis.set_ylabel("Predicted mean with this root held out (ms)")
        axis.legend(fontsize=8)
        axis.grid(alpha=.2)
        axis.set_title("Exploratory depth / piece / root-branching model\nFew distinct, correlated inputs; large errors invalidate general predictions")
        figure.tight_layout()
        save(figure, "branching-model-validation")
    else:
        plt.close(figure)
    (root / "exploratory-models.json").write_text(json.dumps(models, indent=2) + "\n", encoding="utf-8")


def generate(root):
    info = load(root / "run-info.json")
    iterations = info["MeasurementIterations"]
    run_name = info.get("RunName", root.name)
    search_policy = info.get("SearchPolicy", "LegacyExtensions")
    catalog = load(root / "scenarios.json")
    algorithms = info.get("Algorithms", DEFAULT_ALGORITHMS)
    timed_cases = len(catalog) * len(algorithms)
    baseline = algorithms[0]
    cases = [load(path) for path in sorted((root / "cases").glob("*/case.json"))]
    by_key = {(case["Scenario"], case["Algorithm"]): case for case in cases}
    rows = []
    for scenario in catalog:
        for algorithm in algorithms:
            state = by_key.get((scenario["Scenario"], algorithm), {})
            row = {**scenario, "Algorithm": algorithm, "Status": state.get("Status", "Pending"),
                   "MeanMs": None, "StandardDeviationMs": None, "ManagedBytesPerOperation": None,
                   "PeakCommittedMiB": None, "PeakResidentMiB": None, "RetainedMeasurements": None,
                   "MemorySamples": len(state.get("MemorySamples", [])), "Error": state.get("Error", "")}
            if state.get("Timing"):
                row.update(state["Timing"])
            if row["MemorySamples"] == 3:
                row["PeakCommittedMiB"] = mean(s["PeakCommittedBytes"] for s in state["MemorySamples"]) / 2**20
                row["PeakResidentMiB"] = mean(s["PeakResidentBytes"] for s in state["MemorySamples"]) / 2**20
            rows.append(row)
    with (root / "summary.csv").open("w", newline="", encoding="utf-8") as output:
        writer = csv.DictWriter(output, fieldnames=list(rows[0]))
        writer.writeheader()
        writer.writerows(rows)
    completed = [r for r in rows if r["Status"] == "Completed"]
    failures = [r for r in rows if r["Status"] == "Failed"]
    final = len(completed) + len(failures) == timed_cases
    grouped = defaultdict(dict)
    for row in completed:
        grouped[row["Scenario"]][row["Algorithm"]] = row
    matched = {name: group for name, group in grouped.items() if len(group) == len(algorithms)}
    plot_state = root / "plot-state.json"
    previous_plot = load(plot_state).get("CompletedCases", 0) if plot_state.exists() else 0
    if completed and (len(completed) >= previous_plot + len(algorithms) or final):
        plots(root, completed, catalog, final, algorithms)
        plot_state.write_text(json.dumps({"CompletedCases": len(completed)}) + "\n", encoding="utf-8")
    progress = load(root / "progress.json") if (root / "progress.json").exists() else {}
    stopped = progress.get("Phase", "").startswith("Stopped")
    phase = "STOPPED — partial results" if stopped else (("FINISHED WITH FAILURES" if failures else "FINISHED") if final else "IN PROGRESS — partial results")
    lines = ["# Full-depth algorithm benchmark summary", "", f"Run date: {info['Date']}. **{phase}**.", "",
             f"**{len(completed)}/{timed_cases} cases complete; {len(failures)} failed; {len(matched)}/{len(catalog)} scenarios have all {len(algorithms)} algorithms complete.** No runtime limit and no reduction below {iterations} retained measurement iterations. Updated UTC: {datetime.now(timezone.utc).isoformat()}.", "",
             f"Search policy: **{search_policy}**. " + ("MinMax uses the same fixed-depth entry points as correctness comparisons: no capture/check extensions or extra two-ply allowance. Maximum requested horizon is 5 full moves / 10 plies. Earlier legacy-depth datasets are retained separately." if search_policy == "FixedDepthNoExtensions" else "Historical depth policy, including the original MinMax extension/extra-depth behavior."), "",
             "## High-level summary: when each algorithm is better", ""]
    if not matched:
        lines.append(f"No complete {len(algorithms)}-algorithm scenario is available yet. Comparative conclusions and fitted charts will be filled from this rerun, not copied from historical timings.")
    else:
        wins = Counter()
        for group in matched.values():
            wins[min(group, key=lambda algorithm: group[algorithm]["MeanMs"])] += 1
        lines += ["These conclusions are provisional until every requested case has finished. ‘Faster’ refers to measured runtime, not playing strength or result equivalence." if not final else "‘Faster’ refers to measured runtime, not playing strength or result equivalence.", "",
                  f"| Algorithm | Lowest mean in matched cases | Mean-time ratio vs {baseline}¹ | Peak committed process MiB range | Peak resident process MiB range |", "| --- | ---: | ---: | ---: | ---: |"]
        for algorithm in algorithms:
            group = [value[algorithm] for value in matched.values()]
            ratio = math.exp(mean(math.log(value[algorithm]["MeanMs"] / value[baseline]["MeanMs"]) for value in matched.values()))
            lines.append(f"| {algorithm} | {wins[algorithm]}/{len(matched)} | {ratio:.3f}× | {min(r['PeakCommittedMiB'] for r in group):.2f}–{max(r['PeakCommittedMiB'] for r in group):.2f} | {min(r['PeakResidentMiB'] for r in group):.2f}–{max(r['PeakResidentMiB'] for r in group):.2f} |")
        lines += ["", "¹ Equal-scenario geometric mean ratio. Lower is faster; it is workload-dependent, not a confidence interval.", ""]
        for algorithm in algorithms:
            alternatives = [candidate for candidate in algorithms if candidate != algorithm]
            best_name, best_group = max(matched.items(), key=lambda item: min(item[1][other]["MeanMs"] for other in alternatives) / item[1][algorithm]["MeanMs"])
            advantage = min(best_group[other]["MeanMs"] for other in alternatives) / best_group[algorithm]["MeanMs"]
            if advantage > 1:
                lines.append(f"- **{algorithm}** has its largest observed advantage over the next-fastest backend at `{best_name}`: {advantage:.2f}× faster by mean ({best_group[algorithm]['MeanMs']:.4f} ms), with {best_group[algorithm]['PeakCommittedMiB']:.2f} MiB committed and {best_group[algorithm]['PeakResidentMiB']:.2f} MiB resident process peaks. Small differences require considering the per-case SDs.")
            else:
                lines.append(f"- **{algorithm}** has no lowest-mean case among the {len(matched)} fully matched scenarios completed so far.")
        lines += ["",
                  "- **MinMax:** use it as the non-tree baseline and compare both runtime and process-memory behavior against retained tree structures.",
                  "- **Direct PositionTree:** useful in the measured scenarios where its mean beats plain MinMax and its process-memory footprint is acceptable. It may lose on shallow searches where allocation/cache costs dominate; the fixed-depth tables show whether that occurs here.",
                  "- **Copying PositionTree:** evaluate separately above depth 3, where iterative copying actually runs. Below that it follows the direct-build branch. A speed gain can come with overlapping tree allocations and larger committed-memory peaks.", "",
                  "### Observed fastest cases by position family and depth", "", "| Position family | Full-move depth | Completed sides | Lowest-mean algorithms |", "| --- | ---: | ---: | --- |"]
        family = defaultdict(list)
        for group in matched.values():
            row = group[baseline]
            family[row["Kind"], row["FullMoveDepth"]].append(min(group, key=lambda algorithm: group[algorithm]["MeanMs"]))
        for (kind, depth), values in sorted(family.items()):
            counts = Counter(values)
            lines.append(f"| {kind} | {depth} | {len(values)} | " + ", ".join(f"{a}: {n}" for a, n in counts.items()) + " |")
    lines += ["", "## Measurement scope and safeguards", "",
              "- Starting position: White and Black to move at depths **1, 2, 3, 4 full moves**. Each synthetic endgame is measured once, without an artificial colour/side-to-move mirror, at depths **1, 2, 3, 4, 5**. The three game positions retain their natural side to move and use depths **1–5**. There are 8 distinct root/side inputs and 38 depth/scenario combinations.",
              "- Every backend searches the same requested horizon. MinMax receives the horizon in plies (twice the full-move depth); the PositionTree APIs receive the equivalent full-move depth and expand it internally to that ply horizon.",
              f"- {len(algorithms)} backends in one suite: **{timed_cases} timed cases**, {iterations} retained measurements each, one warm-up, one launch, 150 ms target iteration duration, outliers retained. A single slow search can exceed the target. No timeout or lower-sample fallback is allowed.",
              f"- **{timed_cases * 3} memory samples** are required for a successful full run: three fresh processes per algorithm/scenario. Process committed and resident peaks include CLR/JIT, fixture initialization and native heap, not isolated native allocations or WASM requirements.",
              "- Release managed/native guards remain enabled. Setup initializes only the selected root/backend. Cleanup validates the last measured move and history prefix, without repeating every other deep search. Timed boundaries include history reset/restore, native allocation, search, move extraction and cleanup.",
              "- Rc5 cases carry 23/55/116 pre-root historical positions. Synthetic cases have empty history. Piece and legal-move counts are recorded at the actual root; legal moves are not a native total-node counter.",
              f"- Native DLL SHA-256: `{info['NativeDllSha256']}`. Individual raw BenchmarkDotNet exports and memory probes are retained per case. Native failures are recorded, never replaced by guessed means.", "",
              "## Charts and extrapolation", "",
              "Depth is separated in the piece/legal-move plots to avoid mixing exponential depth growth with root complexity. There are only 8 distinct root/side inputs, including two starting-position sides and three positions from the same game; these inputs are correlated. Repeated depths are not independent new positions. Tactics, captures, terminal shortcuts, history, ordering and transpositions confound simple piece/move-count trends.", ""]
    for name, title in [("depth-scaling", "Measured depth scaling and limited one-step projections"), ("time-vs-pieces", "Runtime as a function of root piece count, by depth"), ("time-vs-legal-moves", "Runtime as a function of root legal-move count, by depth"), ("memory-scaling", "Committed and resident process-memory scaling"), ("branching-model-validation", "Exploratory model checked by holding out whole root positions")]:
        if (root / "charts" / f"{name}.png").exists():
            lines += [f"### {title}", "", f"![{title}](charts/{name}.png)", "", f"[SVG export](charts/{name}.svg)", ""]
    lines += ["Depth projections are dashed/open points, never measured results. A log-linear fit is projected by only one extra full move and only when its within-root R² is at least 0.6. That threshold is not proof of predictive accuracy. Piece/move trend lines stay inside the observed range. The separate multivariable exploratory model includes depth, depth × log(root legal moves + 1), and pieces, with whole-root holdout errors. Do not infer a universal complexity law or use these projections as completed benchmark results.", "",
              "## Full per-case timing and memory table", "", "Mean/SD: milliseconds per search. Memory cells: committed / resident MiB, mean of three fresh-process peaks. `—` means not yet measured or failed; it never means zero.", "",
              "| Scenario | Pieces | Root legal moves | History | Algorithm | Status | Mean ms | SD ms | Managed B/op | Process peaks MiB |", "| --- | ---: | ---: | ---: | --- | --- | ---: | ---: | ---: | ---: |"]
    for row in rows:
        value = lambda key: f"{row[key]:.4f}" if row[key] is not None else "—"
        memory = f"{row['PeakCommittedMiB']:.2f} / {row['PeakResidentMiB']:.2f}" if row['PeakCommittedMiB'] is not None else "—"
        lines.append(f"| {row['Scenario']} | {row['PieceCount']} | {row['LegalMoveCount']} | {row['HistoryPositions']} | {row['Algorithm']} | {row['Status']} | {value('MeanMs')} | {value('StandardDeviationMs')} | {value('ManagedBytesPerOperation')} | {memory} |")
    lines += ["", "## Result agreement from this rerun", "", "This is exact agreement of the validated last returned result in completed timing cases, not a claim that every iteration or the entire MSTest suite was rerun. The existing strict comparison tests remain unchanged in intent and cover the shared expanded matrix. Native result codes, distance, raw value, move and piece all count; alternate moves and terminal-field differences are not silently ignored.", "", "| Pair | Comparable completed scenarios | Exact matches |", "| --- | ---: | ---: |"]
    for left, right in itertools.combinations(algorithms, 2):
        count = 0
        same = 0
        for scenario in catalog:
            a, b = by_key.get((scenario['Scenario'], left)), by_key.get((scenario['Scenario'], right))
            if a and b and a.get('Result') and b.get('Result') and a['Status'] == b['Status'] == 'Completed':
                count += 1
                same += signature(a['Result']) == signature(b['Result'])
        lines.append(f"| {left} vs {right} | {count} | {same} |")
    if failures:
        lines += ["", "## Failed measurements", "", f"A resource/native/runtime failure is not a completed {iterations}-iteration benchmark. Review each raw log; no missing mean is extrapolated into the results table.", ""]
        for row in failures:
            lines.append(f"- {row['Scenario']} / {row['Algorithm']}: {row['Error']}")
    lines += ["", "## Data and reproduction", "", "- [Scenario catalog: FENs, pieces, legal moves and history](scenarios.json)", "- [All requested case statuses and measurements](summary.csv)", "- [Run identity and uncapped policy](run-info.json), [live progress](progress.json)", f"- Per-case `cases/<scenario>--<algorithm>/case.json` retains the actual result and all three memory probes; `timing.json` retains all {iterations} samples and environment metadata.", "- [Runner](../../run_full_depth.py) and [report/plot generator](../../full_depth_report.py)", "", f"Resume without changing the binary/matrix/policy: `python Benchmarks/run_full_depth.py --date {info['Date']} --run-name {run_name}`. Successful cases are skipped, interrupted cases resume, and failed cases require `--retry-failed`. Regenerate plots/report only with `python Benchmarks/full_depth_report.py --run-name {run_name}`. Historical datasets are preserved, not silently pooled into this new experiment.", ""]
    (root / "SUMMARY.md").write_text("\n".join(lines), encoding="utf-8")


if __name__ == "__main__":
    import argparse
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--run-name", default="MinMaxConsolidated")
    args = parser.parse_args()
    if not args.run_name or Path(args.run_name).name != args.run_name or args.run_name in (".", ".."):
        parser.error("--run-name must be a directory name, not a path.")
    generate(REPO / "Benchmarks/Results" / args.run_name)
