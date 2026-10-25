"""Resume the uncapped, Release-only full-depth experiment, one durable case at a time.

Build the benchmark project in Release first. Run from any directory:
  python Benchmarks/run_full_depth.py --date 2026-10-24
No timeouts or reduced-iteration fallback are applied. Old datasets are untouched.
"""

import argparse
import hashlib
import importlib
import json
import msvcrt
import os
import re
import shutil
import subprocess
import sys
from datetime import datetime, timezone
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
EXE = REPO / "Benchmarks/Triglav.Chess.Engine.Benchmarks/bin/x64/Release/net10.0/Triglav.Chess.Engine.Benchmarks.exe"
OUT = REPO / "Benchmarks/Results/MinMaxConsolidated"
ARTIFACTS = REPO / "Benchmarks/Triglav.Chess.Engine.Benchmarks/Output/MinMaxConsolidated"
ALGORITHMS = ["MinMax", "PositionTree", "PositionTreeWithCopying"]
MEASUREMENT_ITERATIONS = 16
SEARCH_POLICY = "FixedDepthNoExtensions"


def now():
    return datetime.now(timezone.utc).isoformat()


def write_json(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    temporary = path.with_suffix(path.suffix + ".tmp")
    temporary.write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")
    os.replace(temporary, path)


def run(command, log):
    log.parent.mkdir(parents=True, exist_ok=True)
    with log.open("w", encoding="utf-8") as output:
        # Native crash dialogs are disabled by the launcher; there is no time limit.
        return subprocess.run(command, cwd=REPO, stdout=output, stderr=subprocess.STDOUT,
                              creationflags=subprocess.CREATE_NO_WINDOW).returncode


def refresh():
    try:
        import full_depth_report
        importlib.reload(full_depth_report).generate(OUT)
        (OUT / "report-error.json").unlink(missing_ok=True)
    except Exception as error:
        # Plotting/report issues must not discard completed long searches or stop
        # subsequent measurements. Raw case checkpoints remain authoritative.
        write_json(OUT / "report-error.json", {"Error": repr(error), "UpdatedUtc": now()})
        print(f"Report refresh failed; measurements continue: {error}", flush=True)


def main():
    global OUT, ARTIFACTS
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--date", required=True)
    parser.add_argument("--run-name", default="MinMaxConsolidated")
    parser.add_argument("--retry-failed", action="store_true")
    args = parser.parse_args()
    if not re.fullmatch(r"[A-Za-z0-9_-]+", args.run_name):
        parser.error("--run-name must contain only letters, digits, underscores or hyphens.")
    OUT = REPO / "Benchmarks/Results" / args.run_name
    ARTIFACTS = REPO / "Benchmarks/Triglav.Chess.Engine.Benchmarks/Output" / args.run_name
    OUT.mkdir(parents=True, exist_ok=True)
    # The OS releases the byte lock on exit, including interrupted runners.
    # Keep the file on disk so two processes cannot lock different file objects.
    with (OUT / "runner.lock").open("a+b") as lock:
        if lock.tell() == 0:
            lock.write(b"0")
            lock.flush()
        lock.seek(0)
        try:
            msvcrt.locking(lock.fileno(), msvcrt.LK_NBLCK, 1)
        except OSError as error:
            raise RuntimeError("A runner is already active for this dataset.") from error
        try:
            run_experiment(args)
        finally:
            lock.seek(0)
            msvcrt.locking(lock.fileno(), msvcrt.LK_UNLCK, 1)


def run_experiment(args):
    write_json(OUT / "runner-process.json", {"Pid": os.getpid(), "StartedUtc": now()})
    catalog = subprocess.run([str(EXE), "--algorithm-scenarios"], cwd=REPO,
                             capture_output=True, check=True).stdout.decode("utf-8-sig")
    scenarios = json.loads(catalog)
    assert len(scenarios) == 38
    for scenario in scenarios:
        assert 1 <= scenario["FullMoveDepth"] <= (4 if scenario["Kind"] == "Start" else 5)
        assert scenario["Plies"] == scenario["FullMoveDepth"] * 2
    signature = hashlib.sha256(json.dumps(scenarios, sort_keys=True).encode()).hexdigest()
    dll_hash = hashlib.sha256((REPO / "Core/Triglav.Chess.Interop/Triglav.Chess.Native.dll").read_bytes()).hexdigest()
    loaded_dll_hash = hashlib.sha256((EXE.parent / "Triglav.Chess.Native.dll").read_bytes()).hexdigest()
    if loaded_dll_hash != dll_hash:
        raise RuntimeError("Benchmark output contains a different native DLL; rebuild before running.")
    managed = {path.name: hashlib.sha256(path.read_bytes()).hexdigest()
               for path in EXE.parent.glob("Triglav.Chess.*.dll") if path.name != "Triglav.Chess.Native.dll"}
    info_path = OUT / "run-info.json"
    if info_path.exists():
        previous = json.loads(info_path.read_text(encoding="utf-8"))
        if (previous["CatalogSha256"] != signature or previous["NativeDllSha256"] != dll_hash
                or previous["ManagedAssemblies"] != managed
                or previous.get("Algorithms") != ALGORITHMS
                or previous.get("MeasurementIterations") != MEASUREMENT_ITERATIONS
                or previous.get("SearchPolicy") != SEARCH_POLICY):
            raise RuntimeError("Resume refused: binaries, scenario matrix, depth policy or iteration count differ from this run.")
    else:
        write_json(info_path, {
            "Date": args.date, "StartedUtc": now(), "CatalogSha256": signature,
            "NativeDllSha256": dll_hash, "Configuration": "Release x64",
            "ManagedAssemblies": managed,
            "Scenarios": len(scenarios), "Algorithms": ALGORITHMS,
            "TimedCases": len(scenarios) * len(ALGORITHMS),
            "MeasurementIterations": MEASUREMENT_ITERATIONS, "WarmupIterations": 1, "Launches": 1,
            "SearchPolicy": SEARCH_POLICY, "RunName": args.run_name,
            "TargetIterationMilliseconds": 150, "Outliers": "Retained",
            "MemorySamplesPerCase": 3, "TimeoutSeconds": None,
            "IterationReductionAllowed": False,
            "TimingBoundary": "reset/restore history, native search allocations, result extraction and cleanup",
            "Setup": "initialize selected scenario only; validate last measured result during cleanup",
        })
    write_json(OUT / "scenarios.json", scenarios)
    refresh()
    ordered = sorted(scenarios, key=lambda s: (s["FullMoveDepth"], s["PieceCount"], s["LegalMoveCount"], s["Scenario"]))
    for scenario in ordered:
        for algorithm in ALGORITHMS:
            if (OUT / "stop-after-case").exists():
                write_json(OUT / "progress.json", {"ActiveCase": None, "Phase": "Stopped between cases", "UpdatedUtc": now()})
                refresh()
                print("Stopped between cases; remove stop-after-case and rerun to resume.", flush=True)
                return
            name = f"{scenario['Scenario']}--{algorithm}"
            directory = OUT / "cases" / name
            state_file = directory / "case.json"
            state = json.loads(state_file.read_text(encoding="utf-8")) if state_file.exists() else {
                **scenario, "Algorithm": algorithm, "Status": "Pending", "MemorySamples": [],
            }
            if state["Status"] == "Completed" or (state["Status"] == "Failed" and not args.retry_failed):
                continue
            print(f"{now()} START {name}", flush=True)
            state.update(Status="Running", StartedUtc=state.get("StartedUtc", now()), UpdatedUtc=now())
            write_json(state_file, state)
            write_json(OUT / "progress.json", {"ActiveCase": name, "Phase": "Timing", "UpdatedUtc": now()})
            refresh()
            try:
                if not state.get("Timing"):
                    logs = ARTIFACTS / name
                    pattern = f"*FullDepthComparisonBenchmark.{algorithm}(Scenario: {scenario['Scenario']})"
                    code = run([str(EXE), "--full-depth-comparison", "--filter", pattern,
                                "--exporters", "json", "markdown", "--artifacts", str(logs)], logs / "runner.log")
                    reports = list((logs / "results").glob("*-report-full-compressed.json"))
                    if code != 0 or len(reports) != 1:
                        raise RuntimeError(f"Benchmark process failed (exit {code}); inspect {logs / 'runner.log'}")
                    document = json.loads(reports[0].read_text(encoding="utf-8-sig"))
                    benchmarks = document["Benchmarks"]
                    if len(benchmarks) != 1:
                        raise RuntimeError("The per-case filter did not select exactly one benchmark.")
                    result = benchmarks[0]
                    expected = f"(Scenario: {scenario['Scenario']})"
                    if result["Method"] != algorithm or not result["FullName"].endswith(expected):
                        raise RuntimeError("Benchmark identity differs from the requested algorithm/scenario.")
                    if result["Statistics"] is None or result["Statistics"]["N"] != MEASUREMENT_ITERATIONS:
                        raise RuntimeError(f"Benchmark did not produce all {MEASUREMENT_ITERATIONS} retained measurements.")
                    text = (logs / "runner.log").read_text(encoding="utf-8-sig", errors="replace")
                    snapshots = re.findall(r"^SEARCH_RESULT_JSON (.+)$", text, re.MULTILINE)
                    if not snapshots:
                        raise RuntimeError("No validated last-result snapshot was emitted during cleanup.")
                    last = json.loads(snapshots[-1])
                    if last.get("SearchPolicy") != SEARCH_POLICY:
                        raise RuntimeError("Benchmark search policy differs from the fixed-depth run.")
                    state["Timing"] = {
                        "MeanMs": result["Statistics"]["Mean"] / 1e6,
                        "StandardDeviationMs": result["Statistics"]["StandardDeviation"] / 1e6,
                        "RetainedMeasurements": result["Statistics"]["N"],
                        "ManagedBytesPerOperation": result["Memory"]["BytesAllocatedPerOperation"],
                    }
                    state["Result"] = last
                    directory.mkdir(parents=True, exist_ok=True)
                    shutil.copyfile(reports[0], directory / "timing.json")
                    for report in (logs / "results").glob("*-report-github.md"):
                        shutil.copyfile(report, directory / "benchmarkdotnet-report.md")
                    state.update(Status="Profiling", UpdatedUtc=now())
                    write_json(state_file, state)
                    refresh()
                for sample in range(len(state["MemorySamples"]), 3):
                    write_json(OUT / "progress.json", {"ActiveCase": name, "Phase": f"Memory {sample + 1}/3", "UpdatedUtc": now()})
                    log = ARTIFACTS / name / f"memory-{sample + 1}.json"
                    code = run([str(EXE), "--algorithm-memory-case", str(scenario["Index"]), algorithm], log)
                    if code != 0:
                        raise RuntimeError(f"Memory worker failed (exit {code}); inspect {log}")
                    measured = json.loads(log.read_text(encoding="utf-8-sig"))
                    if measured["Scenario"] != scenario["Scenario"] or measured["Algorithm"] != algorithm:
                        raise RuntimeError("Memory worker returned a different scenario/backend.")
                    if measured.get("SearchPolicy") != SEARCH_POLICY:
                        raise RuntimeError("Memory worker search policy differs from the timing run.")
                    state["MemorySamples"].append(measured)
                    state.update(UpdatedUtc=now())
                    write_json(state_file, state)
                state.update(Status="Completed", CompletedUtc=now(), UpdatedUtc=now())
                state.pop("Error", None)
            except Exception as error:
                state.update(Status="Failed", Error=str(error), UpdatedUtc=now())
                print(f"{now()} FAILED {name}: {error}", flush=True)
            write_json(state_file, state)
            refresh()
            print(f"{now()} {state['Status'].upper()} {name}", flush=True)
    write_json(OUT / "progress.json", {"ActiveCase": None, "Phase": "Finished", "UpdatedUtc": now()})
    refresh()
    print("All requested cases have been attempted. SUMMARY.md records completed and failed cases.", flush=True)


if __name__ == "__main__":
    main()
