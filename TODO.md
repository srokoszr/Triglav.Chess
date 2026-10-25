# Triglav Chess TODO

Only independently verifiable deliverables have checkboxes. Section headings group
related work without implying that every subtask is complete.

## Project

- [x] Create and maintain a TODO list
- [x] Rename the project to "Triglav Chess"
- [x] Add the project license

### README

- [x] Document the medieval rules, persistence formats, analysis UI, benchmarks, and single-file GUI publishing
- [x] Remove the remaining GitLab starter template
- [x] Document native and managed source-build order, test commands, and GUI/CLI startup
- [ ] Verify the documented setup from a fresh checkout

### Third-party licensing

- [x] Add the third-party notices file and the Cburnett BSD 3-Clause notice
- [x] Inventory the current runtime, test, and benchmark direct/transitive dependencies with resolved versions and license evidence
- [x] Verify that the current single-file GUI publish does not include separate notice or license files
- [x] Remove the locally bundled rules PDF and link the externally hosted English and official Polish versions
- [ ] Add NuGet lock files or an automated dependency/license inventory check so later transitive-version drift is detected
- [ ] Package `LICENSE.md`, `THIRD_PARTY_NOTICES.md`, the MS-PL text, and matching .NET runtime notices with every binary release

### GitHub repository and pull-request policy

- [ ] Create or select the canonical GitHub repository and add it as a Git remote
- [ ] Check the complete intended source, documentation, and retained benchmark summaries into GitHub
- [ ] Define the protected default-branch and pull-request policy, including required reviews and status checks
- [ ] Enable the agreed GitHub branch-protection or ruleset settings and verify that direct noncompliant changes are rejected

### GitHub Actions and releases

- [ ] Add a GitHub Actions workflow that builds the native and managed projects on Windows x64
- [ ] Run the appropriate automated test suites and publish useful failure diagnostics in pull requests
- [ ] Add a versioned release workflow that produces the self-contained GUI artifact
- [ ] Publish release notes, checksums, notices, licenses, and signed artifacts through GitHub Releases
- [ ] Exercise the release workflow from a test tag before publishing the first production release

### Managed strong-name signing

- [ ] Establish ownership, secure storage, backup, and rotation policy for the strong-name private key
- [ ] Configure all distributable managed assemblies to use the Strong Name Key signature
- [ ] Verify strong-name identity and references in CI and in the published package

### Authenticode signing

- [ ] Obtain an appropriate Authenticode code-signing certificate and protect its private key
- [ ] Configure the release workflow to sign distributable executables and libraries after publishing
- [ ] Timestamp signatures and verify the complete signed release before upload to GitHub Releases

## GUI

### Evaluation and board

- [x] Display the current evaluation
- [x] Display the MinMax evaluation
- [x] Display the evaluation bar
- [ ] Display material advantage separately from positional evaluation

#### Iterative evaluation and search feedback

- [x] Use iterative/deepening behavior in the existing copying-tree search where configured
- [ ] Define a search request/result contract with depth, limits, best move, score, principal variation, and completion state
- [ ] Add cooperative cancellation and retain the last completed iteration
- [ ] Report bounded progress to the GUI and reject stale results from an earlier position

#### Board labels and localization

- [x] Flip board coordinates with board orientation
- [x] Resize coordinate labels with the board
- [ ] Define the intended label alignment at representative board sizes and display scales
- [ ] Visually verify board labels and longer English/Polish text at those sizes and scales
- [x] Translate application-owned labels, menus, filters, statuses, dialogs, and results

### Game interface and persistence

- [x] Move games to a separate form
- [x] Save the current position from the editor and active game
- [x] Save games safely to a selected `.TgGame` file and create a recovery PGN
- [x] Cover position and game persistence with automated round-trip and malformed-input tests
- [ ] Smoke-test save/load dialogs, overwrite behavior, resignation saves, and close behavior interactively
- [x] Implement the position editor
- [x] Support the FEN position format

### PGN and game import

- [x] Document the supported Triglav medieval PGN subset
- [x] Validate PGN export and recovery saves
- [x] Support PGN legacy compatibility through the shared game import screen
- [x] Import native `.TgGame` and `.PGN` games with validated replay, history, and final-position preview
- [ ] Smoke-test the complete import workflow interactively in both supported UI languages

### Analysis

#### Position analysis

- [x] Open an isolated position snapshot in the analysis UI
- [x] Display the board, side to move, and FEN
- [ ] Run an engine search with start/stop/restart controls
- [ ] Display depth, score, elapsed time, best move, and a legal principal variation

#### Game analysis

- [x] Load native and PGN games into a replay timeline
- [x] Navigate to the first, previous, next, and last position
- [x] Transfer a replay position to position analysis
- [ ] Analyze every position under a common depth or time budget with progress and cancellation
- [ ] Compare played moves with engine alternatives and display an evaluation timeline
- [ ] Export an analysis report and invalidate cached results when engine settings change

### Logging

- [x] Write CLI game/search output to a file
- [ ] Add a durable GUI engine diagnostic log, including failures and search metadata
- [ ] Add a distinct GUI game log rather than treating the binary save file as a log
- [ ] Flush logs correctly on completion, failure, cancellation, and close

## Core

### Game lifecycle and cleanup

- [x] Surface engine failures in the GUI instead of swallowing them
- [x] Save the correct resignation winner and reject later move commits
- [ ] Implement a supported draw-offer workflow or disable the action
- [ ] Own, cancel, and await the active search worker before disposing game resources
- [ ] Complete managed/native stack and tree ownership and disposal
- [ ] Remove obsolete APIs and leftover scaffolding after the safety work is covered by tests

### Search correctness and iterative sessions

- [x] Add fixed-depth comparison entry points that disable capture/check extensions
- [x] Verify that fixed-depth policy does not depend on the optional depth-output pointer
- [ ] Resolve or explicitly normalize strict result differences between plain MinMax and the application tree search
- [ ] Add terminal-aware leaf handling, reliable legal fallback moves, and balanced history restoration
- [ ] Make production iterative search cancellation-safe and preserve completed results

### Game-tree reuse

- [x] Provide generic `PositionTree_Extend` support
- [ ] Implement the two MinMax tree-extension functions that currently return the input tree unchanged
- [ ] Define cache/tree validity across moves, settings changes, loaded positions, and history-dependent rules
- [ ] Add a bounded persistent search session with invalidation and reclamation tests

### Performance and memory

- [x] Adopt the shared-colour search as the canonical `MinMax` implementation and remove the split implementation
- [x] Complete the 212-case fixed-depth benchmark with timing and process-memory measurements
- [x] Prepare the consolidated three-algorithm benchmark with matched depth horizons and no redundant synthetic colour mirrors
- [ ] Run the consolidated 114 timing cases and 342 isolated memory probes, then retain the generated summary
- [ ] Commit the retained fixed-depth summary and synchronize README/assessment status with the completed run
- [ ] Define measurable optimization targets and require result-quality checks alongside timing
- [ ] Add explicit native memory budgets and allocation-failure handling
- [ ] Replace child/hash-chain pointers with offsets into their owning bucket or node pools where measurement justifies it

### Time control

- [ ] Define supported clock modes and timeout adjudication
- [ ] Implement testable player clocks and exact start/switch/stop behavior
- [ ] Allocate engine search time from the remaining clock and retain a legal fallback move
- [ ] Add clock configuration, display, persistence, and race-condition tests

## Known issues and remaining validation

### Board orientation

- [x] Make orientation depend on the player/side instead of always flipping
- [ ] Visually verify both colours, local two-player mode, analysis, and editor orientation

### Search stability

- [x] Address the originally reported stack-overflow crash path
- [ ] Add deep-search and repeated-call stability coverage for every supported backend

### Evaluation confidence

- [x] Keep the existing evaluation regression tests passing
- [ ] Define expected calibration/playing-strength criteria before declaring low evaluations fully validated

### Closed regressions

- [x] Fix pawn promotion and retain regression coverage
- [x] Show a message when the game ends

## Web Based GUI

- [ ] Prepare web based version of Triglav.
