# Third-Party Notices — Triglav Chess

- Project: Triglav Chess
- Copyright: Copyright (c) 2026 Robert Srokosz
- Project license: [Apache License 2.0](LICENSE.md)
- Repository: <https://github.com/srokoszr/Triglav.Chess>

Triglav Chess contains third-party software and artwork. Those components remain
under their respective licenses; the project's Apache License 2.0 does not replace
or supersede them.

## Audit scope and method

This inventory was reviewed on 2026-10-24 against all tracked project files and
the restored NuGet graphs in each `obj/project.assets.json`, using .NET SDK
10.0.401. Package links below identify the exact package version and provide the
package publisher's license and source metadata.

To review the graph after changing or restoring packages, run the following for
the GUI, CLI, tests, and benchmarks, then compare the results with this file:

```powershell
dotnet restore .\App\Triglav.Chess.App.GUI\Triglav.Chess.App.GUI.csproj
dotnet restore .\App\Triglav.Chess.App.CLI\Triglav.Chess.App.CLI.csproj
dotnet restore .\Tests\Triglav.Chess.Engine.Tests\Triglav.Chess.Engine.Tests.csproj
dotnet restore .\Benchmarks\Triglav.Chess.Engine.Benchmarks\Triglav.Chess.Engine.Benchmarks.csproj

dotnet list .\App\Triglav.Chess.App.GUI\Triglav.Chess.App.GUI.csproj package --include-transitive
dotnet list .\App\Triglav.Chess.App.CLI\Triglav.Chess.App.CLI.csproj package --include-transitive
dotnet list .\Tests\Triglav.Chess.Engine.Tests\Triglav.Chess.Engine.Tests.csproj package --include-transitive
dotnet list .\Benchmarks\Triglav.Chess.Engine.Benchmarks\Triglav.Chess.Engine.Benchmarks.csproj package --include-transitive
```

The repository does not currently commit NuGet lock files, so later restores can
select newer transitive versions allowed by the direct packages. Re-run this audit
whenever package or SDK versions change.

## Distributed artwork

### Cburnett chess pieces

The twelve SVG files under
`App/Triglav.Chess.App.GUI/Assets/CBurnett/Chess_*t45.svg` are from the Cburnett
chess-piece set, originally created by Colin M. L. Burnett. Individual Wikimedia
files have also received later contributor edits.

- Original collection and license statement:
  <https://en.wikipedia.org/wiki/User:Cburnett/GFDL_images/Chess>
- File history and license metadata are available from each corresponding file
  page on Wikimedia Commons.
- The author offers the set under multiple licenses. Triglav Chess selects the
  **BSD 3-Clause** option.

The notice for that option is:

```text
Copyright (c) 2006, Colin M.L. Burnett
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice,
   this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.
3. Neither the name of the copyright holder nor the names of its contributors
   may be used to endorse or promote products derived from this software without
   specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```

No other third-party image, font, audio, or data asset was found in the tracked
application projects during this audit.

## Externally hosted rules documents

The source rules are not stored in this repository or included in application
output. [`CHESS_RULES.md`](CHESS_RULES.md) links to the
[English source](https://drive.google.com/file/d/19PWjMzfwhXLvauD8rX6Iwe6u5n9HI2oU/view)
and the
[official Polish version](https://grunwald1410.pl/wp-content/uploads/2026/07/Zasady-Grunwaldzkiego-Turnieju-Szachow-Sredniowiecznych-v1.pdf).
Those externally hosted documents remain under their owners' terms. The Polish
version contains a known translation mistake; consult the English source if the
wording differs.

## Runtime software

These components are present in application output or are embedded by the
self-contained GUI publish.

| Component | Version | Relationship | License | Copyright/author metadata |
|---|---:|---|---|---|
| [CliFx](https://www.nuget.org/packages/CliFx/2.3.6) | 2.3.6 | Direct CLI dependency | [MIT](https://licenses.nuget.org/MIT) | Copyright (C) Oleksii Holub |
| [ExCSS](https://www.nuget.org/packages/ExCSS/4.2.3) | 4.2.3 | GUI dependency through Svg | [MIT](https://licenses.nuget.org/MIT) | Tyler Brinks |
| [protobuf-net](https://www.nuget.org/packages/protobuf-net/3.2.56) | 3.2.56 | Direct engine dependency | [Apache-2.0](https://licenses.nuget.org/Apache-2.0) | Copyright 2008 onwards Marc Gravell |
| [protobuf-net.Core](https://www.nuget.org/packages/protobuf-net.Core/3.2.56) | 3.2.56 | Direct engine dependency | [Apache-2.0](https://licenses.nuget.org/Apache-2.0) | Copyright 2008 onwards Marc Gravell |
| [Svg](https://www.nuget.org/packages/Svg/3.4.7) | 3.4.7 | Direct GUI dependency | [MS-PL](https://licenses.nuget.org/MS-PL) | Copyright © vvvv.org and contributors listed by the package |
| [.NET Runtime](https://github.com/dotnet/runtime/tree/v10.0.12) and Windows Desktop shared framework | 10.0.12 in the inspected self-contained publish | Embedded by `publish-gui.ps1` | MIT, with additional third-party notices | Microsoft and .NET contributors |

The .NET runtime carries its own
[third-party notices](https://github.com/dotnet/runtime/blob/v10.0.12/THIRD-PARTY-NOTICES.TXT).
The runtime patch included in future self-contained publishes follows the SDK and
runtime packs used for that release and must be re-audited when it changes.

## Test-only software

The following packages are resolved by the MSTest project. They are used to build
or run tests and are not part of the current GUI or CLI application output. All
are licensed under the [MIT License](https://licenses.nuget.org/MIT).

| Package | Resolved version |
|---|---:|
| [Microsoft.ApplicationInsights](https://www.nuget.org/packages/Microsoft.ApplicationInsights/2.23.0) | 2.23.0 |
| [Microsoft.DiaSymReader](https://www.nuget.org/packages/Microsoft.DiaSymReader/2.0.0) | 2.0.0 |
| [Microsoft.Extensions.DependencyModel](https://www.nuget.org/packages/Microsoft.Extensions.DependencyModel/8.0.2) | 8.0.2 |
| [Microsoft.Testing.Extensions.CodeCoverage](https://www.nuget.org/packages/Microsoft.Testing.Extensions.CodeCoverage/18.4.1) | 18.4.1 |
| [Microsoft.Testing.Extensions.Telemetry](https://www.nuget.org/packages/Microsoft.Testing.Extensions.Telemetry/2.1.0) | 2.1.0 |
| [Microsoft.Testing.Extensions.TrxReport](https://www.nuget.org/packages/Microsoft.Testing.Extensions.TrxReport/2.1.0) | 2.1.0 |
| [Microsoft.Testing.Extensions.TrxReport.Abstractions](https://www.nuget.org/packages/Microsoft.Testing.Extensions.TrxReport.Abstractions/2.1.0) | 2.1.0 |
| [Microsoft.Testing.Extensions.VSTestBridge](https://www.nuget.org/packages/Microsoft.Testing.Extensions.VSTestBridge/2.1.0) | 2.1.0 |
| [Microsoft.Testing.Platform](https://www.nuget.org/packages/Microsoft.Testing.Platform/2.1.0) | 2.1.0 |
| [Microsoft.Testing.Platform.MSBuild](https://www.nuget.org/packages/Microsoft.Testing.Platform.MSBuild/2.1.0) | 2.1.0 |
| [Microsoft.TestPlatform.ObjectModel](https://www.nuget.org/packages/Microsoft.TestPlatform.ObjectModel/18.0.1) | 18.0.1 |
| [MSTest.Analyzers](https://www.nuget.org/packages/MSTest.Analyzers/4.1.0) | 4.1.0 |
| [MSTest.TestAdapter](https://www.nuget.org/packages/MSTest.TestAdapter/4.1.0) | 4.1.0 |
| [MSTest.TestFramework](https://www.nuget.org/packages/MSTest.TestFramework/4.1.0) | 4.1.0 |

`MSTest.Sdk/4.1.0` currently resolves
`Microsoft.Testing.Extensions.CodeCoverage/18.4.1`. This is the version recorded
in the restored graph even though the project contains an `Update` item naming
18.5.2; the update does not currently alter the resolved SDK-supplied reference.
The test project also consumes `protobuf-net` and `protobuf-net.Core`, already
listed under runtime software.

## Benchmark-only software

The following packages are resolved by the BenchmarkDotNet project. They are not
part of the current GUI or CLI application output. All are licensed under the
[MIT License](https://licenses.nuget.org/MIT).

| Package | Resolved version |
|---|---:|
| [BenchmarkDotNet](https://www.nuget.org/packages/BenchmarkDotNet/0.15.8) | 0.15.8 |
| [BenchmarkDotNet.Annotations](https://www.nuget.org/packages/BenchmarkDotNet.Annotations/0.15.8) | 0.15.8 |
| [CommandLineParser](https://www.nuget.org/packages/CommandLineParser/2.9.1) | 2.9.1 |
| [Gee.External.Capstone](https://www.nuget.org/packages/Gee.External.Capstone/2.3.0) | 2.3.0 |
| [Iced](https://www.nuget.org/packages/Iced/1.21.0) | 1.21.0 |
| [Microsoft.CodeAnalysis.Analyzers](https://www.nuget.org/packages/Microsoft.CodeAnalysis.Analyzers/3.11.0) | 3.11.0 |
| [Microsoft.CodeAnalysis.Common](https://www.nuget.org/packages/Microsoft.CodeAnalysis.Common/4.14.0) | 4.14.0 |
| [Microsoft.CodeAnalysis.CSharp](https://www.nuget.org/packages/Microsoft.CodeAnalysis.CSharp/4.14.0) | 4.14.0 |
| [Microsoft.Diagnostics.NETCore.Client](https://www.nuget.org/packages/Microsoft.Diagnostics.NETCore.Client/0.2.510501) | 0.2.510501 |
| [Microsoft.Diagnostics.Runtime](https://www.nuget.org/packages/Microsoft.Diagnostics.Runtime/3.1.512801) | 3.1.512801 |
| [Microsoft.Diagnostics.Tracing.TraceEvent](https://www.nuget.org/packages/Microsoft.Diagnostics.Tracing.TraceEvent/3.1.21) | 3.1.21 |
| [Microsoft.DotNet.PlatformAbstractions](https://www.nuget.org/packages/Microsoft.DotNet.PlatformAbstractions/3.1.6) | 3.1.6 |
| [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/6.0.0) | 6.0.0 |
| [Microsoft.Extensions.DependencyInjection.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection.Abstractions/6.0.0) | 6.0.0 |
| [Microsoft.Extensions.Logging](https://www.nuget.org/packages/Microsoft.Extensions.Logging/6.0.0) | 6.0.0 |
| [Microsoft.Extensions.Logging.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/6.0.0) | 6.0.0 |
| [Microsoft.Extensions.Options](https://www.nuget.org/packages/Microsoft.Extensions.Options/6.0.0) | 6.0.0 |
| [Microsoft.Extensions.Primitives](https://www.nuget.org/packages/Microsoft.Extensions.Primitives/6.0.0) | 6.0.0 |
| [Perfolizer](https://www.nuget.org/packages/Perfolizer/0.6.1) | 0.6.1 |
| [Pragmastat](https://www.nuget.org/packages/Pragmastat/3.2.4) | 3.2.4 |
| [System.CodeDom](https://www.nuget.org/packages/System.CodeDom/9.0.5) | 9.0.5 |
| [System.Management](https://www.nuget.org/packages/System.Management/9.0.5) | 9.0.5 |
| [System.Reflection.TypeExtensions](https://www.nuget.org/packages/System.Reflection.TypeExtensions/4.7.0) | 4.7.0 |

The benchmark project also consumes `protobuf-net` and `protobuf-net.Core`,
already listed under runtime software.

Principal MIT copyright and author statements in the current package metadata
include:

- .NET Foundation and BenchmarkDotNet contributors for BenchmarkDotNet packages.
- Oleksii Holub for CliFx.
- Tyler Brinks for ExCSS.
- Giacomo Stelluti Scala and contributors for CommandLineParser.
- Ahmed Garhy for Gee.External.Capstone.
- The iced project and contributors for Iced.
- Microsoft Corporation for the listed Microsoft and System packages.
- Andrey Akinshin for Perfolizer and Pragmastat.

The common MIT permission and warranty terms are:

```text
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## License references and release packaging

- The complete Apache License 2.0 text used by `protobuf-net` and
  `protobuf-net.Core` is present in [LICENSE.md](LICENSE.md).
- The complete Microsoft Public License text for Svg is available from the
  [NuGet license service](https://licenses.nuget.org/MS-PL).
- Package-specific source repositories, authors, and license metadata are linked
  from every exact NuGet package page above.

The repository contains `LICENSE.md` and this notice file, but the current
`publish-gui.ps1` output contains exactly one executable and does not copy either
file beside it. Release packaging must therefore add the project license, this
notice, the Microsoft Public License text, and the matching .NET runtime
third-party notices before a binary release is published.
