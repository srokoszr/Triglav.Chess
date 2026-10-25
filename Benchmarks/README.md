# Search comparisons

Git retains benchmark source, shared fixtures, runner/report scripts and this
guide. The entire `Benchmarks/Results` and benchmark `Output` trees are ignored.
Generated Markdown reports, timing/memory JSON, CSV, test results, charts,
checkpoints and logs therefore stay local. A fresh benchmark run generates its
own scenario catalog and run metadata from the source; previous results are not
required to rerun after performance fixes. Regenerating a historical report or
resuming a partial run does require its local raw data.

## Consolidated full benchmark run

A pre-consolidation `FixedDepth16` run covered all four algorithms over the
53-scenario matrix: **212 timed cases**, **16 retained
measurement iterations per case**, five warm-ups, and three isolated memory
samples per case. Starting positions use depths 1–4 full moves; all other roots
use depths 1–5 (at most 10 plies).

The former MinMax and MinMaxUnified implementations used the same fixed-depth entry points as correctness
comparisons, with capture/check extensions and the extra two-ply allowance
disabled. MinMaxUnified is now the sole `MinMax` implementation; the historical
four-backend measurements remain unchanged. Actual visited depth is checked. Both tree algorithms retain their
existing fixed horizons. App search defaults are unchanged. Earlier timings used
a different MinMax depth policy and remain separate.

The new consolidated matrix contains **38 scenarios** and three algorithms:
**114 timed cases** and **342 isolated memory samples**. Starting positions retain
both White and Black to move at depths 1–4. Each colour-symmetric synthetic
endgame is measured once, with its canonical White-to-move position, at depths
1–5. The three Rc5 positions retain the side to move from the actual game and are
not flipped. This avoids treating artificial colour mirrors as distinct workloads.

Build native and managed projects in Release, then run:

```powershell
./Benchmarks/run-full-depth.ps1 -Date 2026-10-24 -RunName MinMaxConsolidated
```

Use a new run name because the retained four-backend dataset has a different API,
scenario matrix, and binary identity. The runner updates the summary after each case, preserves checkpoints, and checks
binary/matrix/depth-policy/iteration identity when resuming. A process lock rejects
duplicate runners for the same dataset. It verifies all 16 retained measurements
and matching timing/memory search policies. There is no runtime timeout or sample
reduction. Do not rebuild binaries during the run. To stop between cases, create
`Results/MinMaxConsolidated/stop-after-case`. Regenerate reports and charts with
`./Benchmarks/write-full-depth-report.ps1 -RunName MinMaxConsolidated`.

## Earlier full-depth rerun (2026-10-18, stopped)

The locally generated `FullDepthRerun` report records each completed case,
including mean/SD, managed allocation, committed/resident process memory, result
agreement and high-level comparisons. Its charts show depth scaling and runtime
against root piece/legal-move counts; projections are explicitly separated from
measurements. The interrupted run remains marked stopped.

The historical run used the same **53 scenarios**: starting positions for
both colours at full-move depths **1–4**, and all other roots at **1–5**. This
includes all three Rc5 game positions with their complete history. The single
`--full-depth-comparison` suite measures four algorithms exactly once per scenario:
**212 timed cases**, **20 retained measurement iterations each**, plus **636 memory
probes** in fresh processes. There is no runtime limit or reduced-sample fallback.
Earlier reports below retain their original, smaller matrices and test outcomes.

The historical run cannot be resumed with the changed binaries and policy. The
current command starts or resumes the separate fixed-depth dataset. No ongoing
notification or hourly monitoring is configured.

The report generator is implemented entirely in PowerShell and writes portable
SVG charts without Python or plotting-package dependencies. Regenerate the
historical report with
`./Benchmarks/write-full-depth-report.ps1 -RunName FullDepthRerun`; chart data
refreshes after each newly completed scenario group and at completion. Report
text and CSV refresh after each case.

The matrix coverage test and four Rc5 fixture/history tests pass in Release.
The fixed-depth correctness assessment covers all 33 scenarios through depth 3;
depths 4–5 have not been rerun as unit tests. Strict result discrepancies remain
enabled. Generated consolidated reports contain the retained timing runs,
scenario-by-scenario mean times, process memory and equality-test results, but
those reports and their CSV inputs remain local.

The active algorithm benchmark suites use a Release-only policy and shared scenarios:

- `--search-comparison`: MinMax versus direct PositionTree and PositionTreeWithCopying, the entry point used by `BreadthFirstTreeSearch`. Setup checks returned move legality and history cleanup while retaining result differences.
- `--full-depth-comparison`: the same three backends as independently selectable cases for the resumable runner.

## Defaults and Release enforcement

Each algorithm/scenario now uses **16 measurement iterations**, **1 warm-up iteration**,
**one launch** and a **150 ms target iteration time**. Outliers are retained, so
all 16 measurements contribute to the arithmetic mean and standard deviation.
BenchmarkDotNet chooses how many search calls fit into an iteration; a slow search
can exceed 150 ms. These defaults are configured on both suites, not just supplied
by a one-off run command.

The launcher and each benchmark's setup refuse a Debug managed build. The native
DLL must also identify itself as a Release build (`NDEBUG`, without `_DEBUG`). An
older DLL without the build-identity export is rejected until rebuilt. Generated
BenchmarkDotNet projects explicitly use Release. Do not use CLI overrides to
weaken these settings when comparing with the recorded runs.

## Positions and depth units

**Nominal depth means full moves**. All three algorithms receive the same search
horizon: MinMax receives **2, 4, 6, 8 and 10 plies**, while the PositionTree APIs
receive the equivalent **1, 2, 3, 4 and 5 full moves** and expand those values to
the same ply horizon internally. The copying branch is exercised above tree
argument 3.

`MateInTwo`, `PawnEnding`, and `RookEnding` use only their canonical
White-to-move form at depths **1–5**. The starting position is the sole synthetic
exception and retains both sides to move at depths **1–4**. Rc5 positions preserve
their actual side to move from the source game at depths **1–5**.

The mate fixture comes from `Shared/MinMaxComparisonCases.cs`. The simple pawn
ending is `7k/6pp/8/8/8/8/PP6/K7`, with the selected side to move and used king
first-move rights. These are nonterminal, low-material inputs. A found mating
continuation can make a deeper search finish early.

The earlier recorded runs kept starting positions at depths 1 and 2, and the rook/pawn ending at depth 2,
for both colours. That matrix had **32 scenarios**, hence **64 benchmark cases** in the
split/unified suite and **96 cases** in the tree suite. The earlier depth-scaling
report retains its original 26-scenario dataset.

Matching fixed depth does not imply identical explored trees or playing
strength: move ordering, transposition and termination policies still differ.
`--search-comparison-check` writes the selected moves and raw evaluations as JSON.

## Timing and memory

Synthetic positions start with empty history. Rc5 game positions start with the
complete pre-move game history; it is restored before every timed invocation. History reset, native allocation, search,
best-move extraction and native cleanup are timed. Position construction,
modifier creation and history allocation/disposal are outside timing. Every tree
is freed in a `finally` block; no cache survives between calls.

BenchmarkDotNet's managed `Allocated` column is **bytes per operation**, not peak
memory and not native allocations. A separate `--algorithm-memory` command runs
**three fresh child processes per algorithm/scenario** (342 samples for the complete current matrix). Each child
initializes only its selected scenario, collects managed garbage, records process
baseline peaks, performs one search, frees the result, then reads Windows process
high-water marks. It deliberately avoids benchmark setup, which would run other
algorithms and contaminate the peaks.

Reports give mean timing, standard deviation, managed bytes/operation, and the
mean of the three **peak committed** and **peak resident** process-memory samples
at every tested nominal depth. Committed memory is Windows peak page-file usage
(`Process.PeakPagedMemorySize64`); resident memory is peak working set. These
process totals include the CLR, loaded code, JIT and native heap. Baseline peaks
and incremental peaks remain in the raw JSON. A zero incremental peak only means
the process did not exceed its earlier high-water mark; it does not mean zero
native allocation. This measures observed Windows process usage, not isolated
native allocation counts, a hard upper bound or WASM requirements.

## Reproduce

Build the native project with Visual Studio MSBuild, `Configuration=Release`,
`Platform=x64`, and `SolutionDir` set to the repository root with a trailing slash.
Build the managed benchmark project in Release, then run from the repository root:

```powershell
dotnet build Benchmarks/Triglav.Chess.Engine.Benchmarks/Triglav.Chess.Engine.Benchmarks.csproj -c Release --no-restore
./Benchmarks/run-full-depth.ps1 -Date 2026-10-24 -RunName MinMaxConsolidated
```

Successful checkpoints are skipped when that command is rerun. Add
`-RetryFailed` to retry failed cases. Reports can be regenerated without running
benchmarks:

```powershell
./Benchmarks/write-full-depth-report.ps1 -RunName MinMaxConsolidated
```

The optional completion watcher can validate a run without changing power state:

```powershell
./Benchmarks/watch-benchmark-completion.ps1 -RunDirectory Benchmarks/Results/MinMaxConsolidated -CheckOnly
```

Omit `-CheckOnly` only when Windows should sleep after every timing export,
memory sample, summary, SVG chart, and runner-lock check succeeds. The memory
measurements are Windows-only and run sequentially. Do not run suites
concurrently: competing workloads distort timing. `--no-restore` assumes
dependencies have already been restored. Historical reports remain separate so
new results do not silently replace data from older settings.

## Recorded run profiles

- Current depth 1–5 timing and memory run: `AlgorithmDepths1To5`.
- Earlier short MinMaxUnified/PositionTree run: `MinMaxUnifiedVsPositionTree`.
- Earlier short split/unified MinMax run: `MinMaxUnified`.

Their generated reports remain in the ignored local `Benchmarks/Results` tree.


## Algorithm equality tests

[BenchmarkScenarioComparisonTests](../Tests/Triglav.Chess.Engine.Tests/AlgorithmComparisonTests/BenchmarkScenarioComparisonTests.cs)
uses the exact same shared scenario matrix as both suites. All three pairs of the
three algorithms are compared on raw evaluation fields and the selected move/piece,
with move legality, input preservation and history cleanup assertions. The class
runs sequentially and always frees native resources. The original 26-scenario Release run had
**2 passing and 24 failing scenarios**. With the Rc5 positions added, the current
32-scenario comparison run had **2 passing and 30 failing scenarios**, plus
four passing Rc5 fixture/history checks. Failures remain enabled for later
review, with no algorithm changes. The locally generated comparison-test report
contains per-scenario and per-pair details; use the commands above to reproduce
it.


## Rc5 game positions

The earlier Rc5 run included three actual positions from
`BlackRc5Threefold.PGN`, at nominal full-move depths 1 and 2 (now expanded to 1–5):

| Position | Source ply | Last move | Side to move | Prior positions |
| --- | ---: | --- | --- | ---: |
| Rc5RandomPly023 | 23 | 12. b4 | Black | 23 |
| Rc5RandomPly055 | 55 | 28. g4 | Black | 55 |
| Rc5RepetitionStart | 116 | 58... Rc5 | White | 116 |

The two random samples are frozen from Python `Random(20261017).sample(range(20, 101), 2)`
(sorted for display), so all backends and future runs use the same inputs. The
repetition-start position is the first occurrence of the board subsequently
repeated after 60...Rc5 and 62...Rc5; it is not the terminal third occurrence.
Both the position and its prior history are reconstructed through the PGN reader,
retaining counters and first-move rights. Cached fixture parsing and history
snapshot creation happen outside timing. Timed calls clear and restore the same
prefix before searching. Unit tests also verify that no supplied history entry
is changed. Memory baselines include fixture parsing and history creation.

[Fixture/history tests](../Tests/Triglav.Chess.Engine.Tests/AlgorithmComparisonTests/Rc5GameScenarioTests.cs)
check the recorded source moves, prefix ordering and the first occurrence against
both later occurrences. The generated Rc5 timing, memory and comparison-test
report remains local under `Benchmarks/Results/Rc5GamePositions`. The original
depth-scaling report is unchanged.

To run only these cases:

```powershell
& 'Benchmarks/Triglav.Chess.Engine.Benchmarks/bin/x64/Release/net10.0/Triglav.Chess.Engine.Benchmarks.exe' --algorithm-memory Benchmarks/Results/Rc5GamePositions/memory-samples.json Rc5
& 'Benchmarks/Triglav.Chess.Engine.Benchmarks/bin/x64/Release/net10.0/Triglav.Chess.Engine.Benchmarks.exe' --search-comparison --filter '*Rc5*' --exporters json markdown --artifacts Benchmarks/Triglav.Chess.Engine.Benchmarks/Output/Rc5GameComparisons/Search
```

The optional final argument to `--algorithm-memory` selects a scenario-name prefix;
indices still identify the full shared matrix. This avoids remeasuring unrelated
scenarios when adding a workload.
