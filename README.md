# Triglav Chess

Modern chess engine for medieval chess rules ;)

## Name

**Triglav Chess** is the product and solution name. Source projects use the
`Triglav.Chess` namespace prefix, and the native engine is distributed as
`Triglav.Chess.Native.dll`.

## Description

Triglav Chess is a Windows x64 chess application and engine for the medieval
ruleset used by a tournament played at Grunwald. It includes a Windows Forms GUI
for playing against the engine or another local player, position editing,
saved-game and PGN import, replay and analysis views, plus a command-line game
interface. The repository also contains the managed model and engine, a native
C search and move-generation library, automated tests, and benchmark results.

This is not an implementation of modern orthodox chess. The variant's initial
position, king and queen first moves, promotion rules, and game-ending conditions
are documented in [Medieval chess rules](CHESS_RULES.md).

## Medieval chess rules

See [Medieval chess rules](CHESS_RULES.md) for board setup, piece movement, and
game endings. The rules are used at the medieval chess tournament held annually
during the Battle of Grunwald reconstruction. Source documents are available as
the [English rules](https://drive.google.com/file/d/19PWjMzfwhXLvauD8rX6Iwe6u5n9HI2oU/view)
and the
[official Polish version](https://grunwald1410.pl/wp-content/uploads/2026/07/Zasady-Grunwaldzkiego-Turnieju-Szachow-Sredniowiecznych-v1.pdf).
The Polish version contains a known translation mistake; consult the English
source if the wording differs.


## Saving positions and games

See [Position and game files](docs/PERSISTENCE.md) for save/load actions, importing native `.TgGame` or `.PGN` games, compatibility, and medieval FEN queen-state fields. See [PGN format and import](docs/PGN_FORMAT.md) for the supported medieval PGN profile and legacy import options.

## Tests

Algorithm correctness is assessed by automated comparison tests using the app's
copying-tree algorithm as the reference for returned moves and evaluations.

Correctness comparisons use a fixed horizon with capture/check extensions and the
extra MinMax depth allowance disabled. They reject depths above 5 full moves / 10
plies. Generated comparison reports and historical result sets remain local and
are not included in the repository.

Set `TRIGLAV_COMPARISON_MAX_FULL_MOVES` to a value from 1 to 5 to select a shallower comparison run; the default includes the full matrix up to depth 5.

## Benchmarks

Benchmark source, shared fixtures, runners and report generators are included,
while generated results remain local and are ignored by Git. See
[Search comparison benchmarks](Benchmarks/README.md) for MinMax versus
PositionTree, matching nominal depths, reproduction commands and the documented
historical run configurations.

## Native ABI contract

`Triglav.Chess.Native.dll` exposes a C ABI to the managed projects. The supported
interop target is Windows x64. The native project is compiled as C17, and managed
imports use `CallingConvention.Cdecl`/`CallConvCdecl`. The DLL and managed
assemblies are released as a matched set; there is currently no ABI-version
negotiation or compatibility shim.

### Symbol naming and visibility

- Every function exported from the DLL with `__declspec(dllexport)` must start
  with `TriglavChess_External_`. The complete suffix is the exact unmanaged entry
  point used by `LibraryImport` or `DllImport`; changing it is an ABI break.
- A non-static function declared in a native header for use by other native source
  files must start with `TriglavChess_`, but not `TriglavChess_External_`, unless
  it is also a DLL export.
- Helpers used by only one source file should be `static` and do not need a public
  prefix. Do not export a helper merely to share it inside the native project.
- Export declarations live in the relevant public native headers, including
  [`InteropInterface.h`](Native/Triglav.Chess.Native/InteropInterface.h),
  [`MinMax.h`](Native/Triglav.Chess.Native/MinMax.h),
  [`PositionStack.h`](Native/Triglav.Chess.Native/PositionStack.h), and
  [`PositionTree.h`](Native/Triglav.Chess.Native/PositionTree.h). C++ consumers
  must see exported declarations through `extern "C"` so names are not mangled.

### Types and calling convention

- ABI signatures use fixed-width C integer types from `<stdint.h>`. The local
  `ulong` alias is exactly `uint64_t`; it must not be replaced with the
  platform-dependent C/C++ `unsigned long` type.
- Interop structs are passed by value or pointer exactly as declared in
  [`InteropTypes.h`](Native/Triglav.Chess.Native/InteropTypes.h). Their field
  order, size, alignment, signedness, and padding must match the explicit managed
  layouts under
  [`Core/Triglav.Chess.Model/Structs/Interoperable`](Core/Triglav.Chess.Model/Structs/Interoperable).
- Native enums crossing the boundary currently correspond to 32-bit managed
  enums. Changing an enum's underlying representation or numeric values is an ABI
  and persisted-data change.
- Do not expose C++ classes, references, templates, standard-library containers,
  exceptions, compiler-specific `bool`, or ownership-managing objects in an ABI
  signature. Use plain C structs, fixed-width scalars, and opaque pointers.
- Every managed declaration must specify the exact exported `EntryPoint` and
  `CallConvCdecl`. Parameter direction (`in`, `out`, pointer), return type, and
  struct layout are part of the contract and must be updated on both sides in the
  same change.

### Memory ownership and compatibility

- Memory allocated by the native DLL must be released by its matching native
  function. For example, `TriglavChess_External_PositionStack_new` pairs with
  `TriglavChess_External_PositionStack_free`, and returned position trees are
  released through `TriglavChess_External_PositionTree_free`.
- Caller-provided output buffers remain caller-owned. Borrowed pointers, such as
  the Zobrist-table pointer, must not be freed or retained beyond the documented
  lifetime. Never free native memory with the managed allocator or managed memory
  with the native allocator.
- Renaming or removing an export, changing a signature or calling convention,
  reordering/resizing a shared struct, changing enum values, or changing ownership
  rules is an ABI-breaking change. Update all managed interop declarations and
  tests together; add an intentional compatibility export if older consumers must
  continue working.

After an ABI change, build native Release x64 and the managed interop/tests, inspect
the DLL export table, and verify that every managed entry point resolves. The
canonical managed declarations are in
[`Core/Triglav.Chess.Interop`](Core/Triglav.Chess.Interop).

## Getting started

### Publish the GUI as one executable

On Windows, install the .NET 10 SDK and Visual Studio/Build Tools with the v143
C++ toolset and Windows SDK. From the repository root, run:

```powershell
.\publish-gui.ps1
```

The script discovers Visual Studio MSBuild, rebuilds the native engine in Release
x64 with a static C runtime, and publishes a self-contained `win-x64` application.
The output is `artifacts/publish/gui-win-x64/Triglav.Chess.App.GUI.exe`; the target
computer does not need a separate .NET installation. Both English and Polish
resources, the native engine and the sample analysis game are included.

Use `-OutputDirectory C:\Releases\Triglav` to choose a destination, or
`-MSBuildPath 'C:\path\to\MSBuild.exe'` to select a specific Visual Studio build.
The destination must be empty or contain only an earlier GUI executable; the
script never deletes unrelated files. The first publish may download runtime
packages from NuGet.

The [single-file package](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/)
extracts native libraries to the user's temporary directory when launched. The
embedded example game is copied to `%LOCALAPPDATA%\Triglav Chess\Examples` when
loaded. Preferences and saved games keep their existing locations, and the PGN
recovery copy stays in the GUI executable's directory.

If Windows blocks scripts, enable execution just for this invocation:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\publish-gui.ps1
```

Open **Analysis** from the main window for **Position analysis** and **Game analysis**. The supplied example game is included; load another native `.TgGame` or supported `.PGN` using **Choose game file**. Step with **First / Previous / Next / Last**, the Left/Right/Home/End keys, or select a move in the list. The selected position and last move appear on the board. **Open in position analysis** transfers a snapshot; the editor's **Analyse this position** button also opens the position tab. Engine scores, best moves and variations are placeholders for a later implementation; no engine search runs in this interface.

The main window has three buttons: **New Game**, **Analysis** and **Editor**. New Game offers the engine or a friend on the same computer. For engine games, set strength and human mode, then click the White, Random or Black picture button to start. **File → Import Game** opens saved-game import. Editor actions are available in its **File** and **Position** menus.

The game's right panel displays live PGN, and the Position Editor displays live FEN. Both fields are read-only and support text selection, Ctrl+C, and the **Copy / Select all** context menu.

In the desktop app, open **File → Preferences** from the main window to choose where new games are saved. New games receive unique `.TgGame` filenames automatically; the folder defaults to `Documents/Triglav Chess/Games`. Preferences persist in `%LOCALAPPDATA%/Triglav Chess/settings.json` under `GameSaveDirectory`. Existing games keep their own save paths, and **Save game as…** remains available. The PGN crash-recovery copy continues to be saved in the GUI directory.



## Installation

There is not yet a released installer. To produce a self-contained GUI from a
source checkout, follow [Publish the GUI as one executable](#publish-the-gui-as-one-executable).

### Requirements for a source build

- Windows x64.
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
- Visual Studio or Visual Studio Build Tools with MSBuild, the v143 C++ toolset,
  and a Windows SDK. The **Desktop development with C++** workload supplies the
  usual components.

Run the following commands from a Visual Studio Developer PowerShell in the
repository root. The native DLL must be built first because the managed interop
project copies it from `Core/Triglav.Chess.Interop` into application outputs.

```powershell
$solutionDir = (Resolve-Path .).Path + '\'
msbuild .\Native\Triglav.Chess.Native\Triglav.Chess.Native.vcxproj /p:Configuration=Release /p:Platform=x64 /p:SolutionDir=$solutionDir
dotnet build .\App\Triglav.Chess.App.GUI\Triglav.Chess.App.GUI.csproj -c Release
dotnet build .\App\Triglav.Chess.App.CLI\Triglav.Chess.App.CLI.csproj -c Release
dotnet build .\Tests\Triglav.Chess.Engine.Tests\Triglav.Chess.Engine.Tests.csproj -c Release
```

The `dotnet build` commands restore their NuGet dependencies automatically.

## Usage

Start the already-built desktop application with:

```powershell
dotnet run --project .\App\Triglav.Chess.App.GUI\Triglav.Chess.App.GUI.csproj -c Release --no-build
```

Use **New Game** to play against the engine or another person on the same
computer, **Editor** to create or load a position, and **Analysis** to replay an
imported game or inspect a position. File locations, supported formats, and
compatibility behavior are described in [Position and game files](docs/PERSISTENCE.md)
and [PGN format and import](docs/PGN_FORMAT.md).

Start a command-line game as White or Black with:

```powershell
dotnet run --project .\App\Triglav.Chess.App.CLI\Triglav.Chess.App.CLI.csproj -c Release --no-build -- play
dotnet run --project .\App\Triglav.Chess.App.CLI\Triglav.Chess.App.CLI.csproj -c Release --no-build -- play --as-black
```

Enter moves in coordinate form such as `e2-e4`. The CLI prints the board and
evaluation and writes a detailed `.out` log under
`App/Triglav.Chess.App.CLI/Output`.

## Support

Report reproducible problems in the
[current repository issue tracker](https://github.com/srokoszr/Triglav.Chess/issues).
Include the Windows version, build configuration, steps to reproduce, and any
relevant FEN, PGN, `.TgGame`, or log file. Repository policy and release
automation are tracked in the roadmap.

## Roadmap

The detailed, checkbox-based roadmap is maintained in [TODO.md](TODO.md). Major
remaining areas include repository and pull-request policy, continuous
integration and releases, strong-name and Authenticode signing, completing the
analysis engine workflow, search lifecycle safety, and interactive GUI
verification.
Blazor WASM GUI option is planned at some point.

## Contributing

Contributions are welcome. By contributing, you agree that your changes are
licensed under the Apache License 2.0; see [CONTRIBUTING.md](CONTRIBUTING.md).
Keep changes focused, add or update tests for changed behavior, and update the
relevant documentation. Native interop changes must follow the
[Native ABI contract](#native-abi-contract) and update both sides of the boundary
in the same change.

After completing the native-first source build above, run the full automated
suite with:

```powershell
dotnet run --project .\Tests\Triglav.Chess.Engine.Tests\Triglav.Chess.Engine.Tests.csproj -c Release --no-build -- --no-progress
```

The deeper fixed-horizon comparison matrix can take substantially longer. Use
`TRIGLAV_COMPARISON_MAX_FULL_MOVES` as described in [Tests](#tests) when a
shorter local run is appropriate, and disclose the chosen limit with results.

## Authors and acknowledgment

Triglav Chess was created and is maintained by Robert Srokosz. Thanks to all
contributors and to the authors of the libraries and visual assets used by the
project; attribution and license details are recorded in
[THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).


## License

Project: Triglav Chess

Copyright (c) 2026 Robert Srokosz

Licensed under Apache 2.0.
The software is provided "AS IS", without warranties or guarantees of any kind.
Use at your own risk.

Repository:
https://github.com/srokoszr/Triglav.Chess

## Project status

The project is under active development and has not reached its first signed,
versioned release. The GUI, CLI, persistence, automated tests, and search
benchmarks are usable, while engine-backed analysis, release automation, signing,
and several lifecycle and validation tasks remain in progress. See
[TODO.md](TODO.md) for the current state.
