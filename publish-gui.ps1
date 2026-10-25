#requires -Version 5.1
<#
.SYNOPSIS
Builds the native engine and publishes a self-contained Windows x64 GUI executable.
.EXAMPLE
.\publish-gui.ps1
.EXAMPLE
.\publish-gui.ps1 -OutputDirectory C:\Releases\Triglav
#>
[CmdletBinding()]
param(
    [ValidateNotNullOrEmpty()]
    [String]$OutputDirectory = (Join-Path $PSScriptRoot 'artifacts/publish/gui-win-x64'),
    [String]$MSBuildPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([Environment]::OSVersion.Platform -ne [PlatformID]::Win32NT) {
    throw 'Publishing this WinForms application requires Windows and the Visual C++ build tools.'
}

$dotnet = (Get-Command dotnet -ErrorAction Stop).Source
if ([String]::IsNullOrWhiteSpace($MSBuildPath)) {
    $availableMSBuild = Get-Command MSBuild.exe -ErrorAction SilentlyContinue
    if ($null -ne $availableMSBuild) {
        $MSBuildPath = $availableMSBuild.Source
    }
    else {
        $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
        if (Test-Path -LiteralPath $vswhere -PathType Leaf) {
            $discovered = @(& $vswhere -latest -products '*' -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -find 'MSBuild\Current\Bin\MSBuild.exe')
            if ($LASTEXITCODE -eq 0 -and $discovered.Count -gt 0) {
                $MSBuildPath = $discovered[0]
            }
        }
    }
}
if ([String]::IsNullOrWhiteSpace($MSBuildPath) -or -not (Test-Path -LiteralPath $MSBuildPath -PathType Leaf)) {
    throw 'Visual C++ MSBuild was not found. Install Visual Studio/Build Tools with the v143 C++ toolset and Windows SDK, or pass -MSBuildPath.'
}

$publishDirectory = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
$executableName = 'Triglav.Chess.App.GUI.exe'
if (Test-Path -LiteralPath $publishDirectory) {
    if (-not (Test-Path -LiteralPath $publishDirectory -PathType Container)) {
        throw 'OutputDirectory must name a directory.'
    }
    $unrelatedFiles = @(Get-ChildItem -LiteralPath $publishDirectory -Force | Where-Object {
        $_.PSIsContainer -or $_.Name -ne $executableName
    })
    if ($unrelatedFiles.Count -gt 0) {
        throw 'Choose an empty output directory, or one containing only the previously published GUI executable. Existing files will not be deleted.'
    }
}

$nativeProject = Join-Path $PSScriptRoot 'Native/Triglav.Chess.Native/Triglav.Chess.Native.vcxproj'
$guiProject = Join-Path $PSScriptRoot 'App/Triglav.Chess.App.GUI/Triglav.Chess.App.GUI.csproj'
$solutionDirectory = $PSScriptRoot.Replace('\', '/') + '/'

Write-Host 'Building the Release x64 native engine with its C runtime linked statically...'
$nativeArguments = @(
    $nativeProject
    '/t:Rebuild'
    '/p:Configuration=Release'
    '/p:Platform=x64'
    ('/p:SolutionDir=' + $solutionDirectory)
    '/p:UseStaticRuntime=true'
    '/nologo'
    '/verbosity:quiet'
    '/clp:ErrorsOnly'
)
& $MSBuildPath @nativeArguments
if ($LASTEXITCODE -ne 0) {
    throw "Native build failed with exit code $LASTEXITCODE."
}

Write-Host 'Publishing the self-contained GUI for win-x64...'
$publishArguments = @(
    'publish'
    $guiProject
    '--configuration', 'Release'
    '--runtime', 'win-x64'
    '--self-contained', 'true'
    '--output', $publishDirectory
    '--nologo'
    '--verbosity', 'minimal'
    '/p:Platform=x64'
    '/p:PublishSingleFile=true'
    '/p:IncludeNativeLibrariesForSelfExtract=true'
    '/p:IncludeAllContentForSelfExtract=false'
    '/p:PublishTrimmed=false'
    '/p:PublishReadyToRun=false'
    '/p:DebugType=embedded'
    '/p:CopyOutputSymbolsToPublishDirectory=false'
)
& $dotnet @publishArguments
if ($LASTEXITCODE -ne 0) {
    throw "GUI publish failed with exit code $LASTEXITCODE."
}

$publishedFiles = @(Get-ChildItem -LiteralPath $publishDirectory -Force)
if ($publishedFiles.Count -ne 1 -or $publishedFiles[0].PSIsContainer -or $publishedFiles[0].Name -ne $executableName) {
    throw "Publish did not produce exactly one executable. Inspect $publishDirectory."
}
Write-Host ('Published: ' + $publishedFiles[0].FullName)
Write-Host ('Size: {0:N1} MiB' -f ($publishedFiles[0].Length / 1MB))
