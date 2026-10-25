# Contributing to Triglav Chess

Thank you for helping improve Triglav Chess. Bug reports, documentation fixes,
tests, performance investigations, and focused code changes are welcome.

## Before making a change

- Check the [issue tracker](https://github.com/srokoszr/Triglav.Chess/issues)
  and the project [roadmap](TODO.md) for related work.
- Open an issue before starting a large feature, architectural change, persisted
  format change, or native ABI change so the approach can be agreed first.
- Remember that Triglav Chess implements the medieval rules documented in
  [CHESS_RULES.md](CHESS_RULES.md), not modern orthodox chess.

## Development environment

Source builds currently require Windows x64, the .NET 10 SDK, and Visual Studio
or Visual Studio Build Tools with the v143 C++ toolset and a Windows SDK. Use a
Visual Studio Developer PowerShell for the commands below.

Build the native library first, followed by the managed projects:

```powershell
$solutionDir = (Resolve-Path .).Path + '\'
msbuild .\Native\Triglav.Chess.Native\Triglav.Chess.Native.vcxproj /p:Configuration=Release /p:Platform=x64 /p:SolutionDir=$solutionDir
dotnet build .\App\Triglav.Chess.App.GUI\Triglav.Chess.App.GUI.csproj -c Release
dotnet build .\App\Triglav.Chess.App.CLI\Triglav.Chess.App.CLI.csproj -c Release
dotnet build .\Tests\Triglav.Chess.Engine.Tests\Triglav.Chess.Engine.Tests.csproj -c Release
```

The complete setup, publishing, and application startup instructions are in the
[README](README.md#installation).

## Tests

Run the automated suite after a successful Release build:

```powershell
dotnet run --project .\Tests\Triglav.Chess.Engine.Tests\Triglav.Chess.Engine.Tests.csproj -c Release --no-build -- --no-progress
```

The fixed-horizon algorithm comparison can take substantially longer than the
ordinary tests. If you limit it with `TRIGLAV_COMPARISON_MAX_FULL_MOVES`, state
the selected value in the pull request.

## Change guidelines

- Keep each change focused and avoid unrelated cleanup.
- Follow the repository `.editorconfig` and the existing style in the files you
  modify.
- Add or update tests for changed behavior and bug fixes.
- Update the relevant user, format, rule, benchmark, or ABI documentation.
- Change native exports and managed interop declarations together. Follow the
  [native ABI contract](README.md#native-abi-contract).
- Do not commit build outputs, IDE settings, logs, local game output, generated
  benchmark working data, or credentials. Retain benchmark summaries and their
  supporting artifacts only when they are intentionally part of the published
  results.
- Preserve third-party copyright and license notices. Update
  [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) when adding or changing a
  dependency or asset.

## Pull requests

A pull request should explain the problem and solution, link related issues,
and list the validation performed. Include screenshots for visible GUI or
localization changes. For benchmark changes, record the build configuration,
hardware-relevant details, workload, depth policy, and comparison method needed
to interpret or reproduce the result.

## Licensing

By submitting a contribution for inclusion in Triglav Chess, you agree that it
is licensed under the [Apache License 2.0](LICENSE.md), without additional terms
or conditions unless explicitly agreed in writing.
