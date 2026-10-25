#requires -Version 5.1
<#
.SYNOPSIS
Runs the Release-only fixed-depth benchmark matrix with durable per-case checkpoints.

.DESCRIPTION
Build the benchmark project in Release first. The runner has no timeout or
reduced-iteration fallback. Completed cases are skipped when a compatible run
is resumed, and old run directories are never replaced implicitly.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d{4}-\d{2}-\d{2}$')]
    [String]$Date,

    [ValidatePattern('^[A-Za-z0-9_-]+$')]
    [String]$RunName = 'MinMaxConsolidated',

    [Switch]$RetryFailed
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'Benchmark.Common.psm1') -Force

$repo = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$benchmarkExe = Join-Path $repo 'Benchmarks/Triglav.Chess.Engine.Benchmarks/bin/x64/Release/net10.0/Triglav.Chess.Engine.Benchmarks.exe'
$runRoot = Join-Path $repo ('Benchmarks/Results/' + $RunName)
$artifactRoot = Join-Path $repo ('Benchmarks/Triglav.Chess.Engine.Benchmarks/Output/' + $RunName)
$reportScript = Join-Path $PSScriptRoot 'write-full-depth-report.ps1'
$algorithms = @('MinMax', 'PositionTree', 'PositionTreeWithCopying')
$measurementIterations = 16
$memorySamplesPerCase = 3
$searchPolicy = 'FixedDepthNoExtensions'

function Set-BenchmarkProperty {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][String]$Name,
        [AllowNull()]$Value
    )

    if ($null -eq $Object.PSObject.Properties[$Name]) {
        $Object | Add-Member -MemberType NoteProperty -Name $Name -Value $Value
    }
    else {
        $Object.$Name = $Value
    }
}

function Get-BenchmarkProperty {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][String]$Name,
        [AllowNull()]$Default = $null
    )

    if ($null -eq $Object.PSObject.Properties[$Name]) {
        return $Default
    }
    return $Object.$Name
}

function Invoke-BenchmarkProcess {
    param(
        [Parameter(Mandatory = $true)][String]$FilePath,
        [Parameter(Mandatory = $true)][String[]]$ArgumentList,
        [Parameter(Mandatory = $true)][String]$LogPath
    )

    [IO.Directory]::CreateDirectory((Split-Path -Parent $LogPath)) | Out-Null
    Push-Location $repo
    try {
        $previousPreference = $ErrorActionPreference
        $ErrorActionPreference = 'Continue'
        & $FilePath @ArgumentList 2>&1 | Out-File -LiteralPath $LogPath -Encoding utf8
        return [Int32]$LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousPreference
        Pop-Location
    }
}

function Update-BenchmarkReport {
    try {
        & $reportScript -RunDirectory $runRoot
        $reportError = Join-Path $runRoot 'report-error.json'
        if (Test-Path -LiteralPath $reportError) {
            Remove-Item -LiteralPath $reportError -Force
        }
    }
    catch {
        Write-BenchmarkJson -Path (Join-Path $runRoot 'report-error.json') -Value ([Ordered]@{
            Error = $_.Exception.Message
            UpdatedUtc = Get-BenchmarkUtc
        })
        Write-Warning ('Report refresh failed; measurements continue: ' + $_.Exception.Message)
    }
}

function Assert-ResumeCompatible {
    param(
        [Parameter(Mandatory = $true)]$Previous,
        [Parameter(Mandatory = $true)][Object[]]$Scenarios,
        [Parameter(Mandatory = $true)][String]$NativeHash,
        [Parameter(Mandatory = $true)]$ManagedHashes
    )

    $existingScenariosPath = Join-Path $runRoot 'scenarios.json'
    $sameScenarios = Test-Path -LiteralPath $existingScenariosPath
    if ($sameScenarios) {
        $existingScenariosDocument = Read-BenchmarkJson $existingScenariosPath
        $existingScenarios = @($existingScenariosDocument)
        $sameScenarios = (ConvertTo-BenchmarkCanonicalJson $existingScenarios) -eq (ConvertTo-BenchmarkCanonicalJson $Scenarios)
    }
    $sameManaged = (ConvertTo-BenchmarkCanonicalJson $Previous.ManagedAssemblies) -eq (ConvertTo-BenchmarkCanonicalJson $ManagedHashes)
    $sameAlgorithms = (ConvertTo-BenchmarkCanonicalJson @($Previous.Algorithms)) -eq (ConvertTo-BenchmarkCanonicalJson $algorithms)

    if (-not $sameScenarios -or $Previous.NativeDllSha256 -ne $NativeHash -or -not $sameManaged -or
        -not $sameAlgorithms -or $Previous.MeasurementIterations -ne $measurementIterations -or
        $Previous.SearchPolicy -ne $searchPolicy) {
        throw 'Resume refused: binaries, scenario matrix, depth policy or iteration count differ from this run.'
    }
}

function Invoke-FullDepthExperiment {
    if (-not (Test-Path -LiteralPath $benchmarkExe -PathType Leaf)) {
        throw "Release benchmark executable not found: $benchmarkExe"
    }
    if (-not (Test-Path -LiteralPath $reportScript -PathType Leaf)) {
        throw "Report script not found: $reportScript"
    }

    Write-BenchmarkJson -Path (Join-Path $runRoot 'runner-process.json') -Value ([Ordered]@{
        Pid = $PID
        StartedUtc = Get-BenchmarkUtc
    })

    Push-Location $repo
    try {
        $catalogOutput = @(& $benchmarkExe '--algorithm-scenarios' 2>&1)
        $catalogExitCode = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }
    if ($catalogExitCode -ne 0) {
        throw "Scenario catalog failed with exit code $catalogExitCode."
    }
    $scenarioDocument = (($catalogOutput | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine | ConvertFrom-Json)
    $scenarios = @($scenarioDocument)
    if ($scenarios.Count -ne 38) {
        throw "Expected 38 scenarios, but the benchmark executable returned $($scenarios.Count)."
    }
    foreach ($scenario in $scenarios) {
        $maximumDepth = if ($scenario.Kind -eq 'Start') { 4 } else { 5 }
        if ($scenario.FullMoveDepth -lt 1 -or $scenario.FullMoveDepth -gt $maximumDepth -or
            $scenario.Plies -ne ($scenario.FullMoveDepth * 2)) {
            throw "Scenario '$($scenario.Scenario)' has an invalid depth definition."
        }
    }

    $sourceNativeDll = Join-Path $repo 'Core/Triglav.Chess.Interop/Triglav.Chess.Native.dll'
    $loadedNativeDll = Join-Path (Split-Path -Parent $benchmarkExe) 'Triglav.Chess.Native.dll'
    $nativeHash = Get-BenchmarkFileSha256 $sourceNativeDll
    if ((Get-BenchmarkFileSha256 $loadedNativeDll) -ne $nativeHash) {
        throw 'Benchmark output contains a different native DLL; rebuild before running.'
    }

    $managedHashes = [Ordered]@{}
    Get-ChildItem -LiteralPath (Split-Path -Parent $benchmarkExe) -Filter 'Triglav.Chess.*.dll' -File |
        Where-Object Name -ne 'Triglav.Chess.Native.dll' |
        Sort-Object Name |
        ForEach-Object { $managedHashes[$_.Name] = Get-BenchmarkFileSha256 $_.FullName }

    $runInfoPath = Join-Path $runRoot 'run-info.json'
    if (Test-Path -LiteralPath $runInfoPath) {
        Assert-ResumeCompatible -Previous (Read-BenchmarkJson $runInfoPath) -Scenarios $scenarios -NativeHash $nativeHash -ManagedHashes $managedHashes
    }
    else {
        $catalogJson = ConvertTo-BenchmarkCanonicalJson $scenarios
        Write-BenchmarkJson -Path $runInfoPath -Value ([Ordered]@{
            Date = $Date
            StartedUtc = Get-BenchmarkUtc
            CatalogSha256 = Get-BenchmarkTextSha256 $catalogJson
            NativeDllSha256 = $nativeHash
            Configuration = 'Release x64'
            ManagedAssemblies = $managedHashes
            Scenarios = $scenarios.Count
            Algorithms = $algorithms
            TimedCases = $scenarios.Count * $algorithms.Count
            MeasurementIterations = $measurementIterations
            WarmupIterations = 1
            Launches = 1
            SearchPolicy = $searchPolicy
            RunName = $RunName
            TargetIterationMilliseconds = 150
            Outliers = 'Retained'
            MemorySamplesPerCase = $memorySamplesPerCase
            TimeoutSeconds = $null
            IterationReductionAllowed = $false
            TimingBoundary = 'reset/restore history, native search allocations, result extraction and cleanup'
            Setup = 'initialize selected scenario only; validate last measured result during cleanup'
        })
    }

    Write-BenchmarkJson -Path (Join-Path $runRoot 'scenarios.json') -Value $scenarios
    Update-BenchmarkReport

    $orderedScenarios = @($scenarios | Sort-Object FullMoveDepth, PieceCount, LegalMoveCount, Scenario)
    foreach ($scenario in $orderedScenarios) {
        foreach ($algorithm in $algorithms) {
            if (Test-Path -LiteralPath (Join-Path $runRoot 'stop-after-case')) {
                Write-BenchmarkJson -Path (Join-Path $runRoot 'progress.json') -Value ([Ordered]@{
                    ActiveCase = $null
                    Phase = 'Stopped between cases'
                    UpdatedUtc = Get-BenchmarkUtc
                })
                Update-BenchmarkReport
                Write-Host 'Stopped between cases; remove stop-after-case and rerun to resume.'
                return
            }

            $caseName = $scenario.Scenario + '--' + $algorithm
            $caseDirectory = Join-Path (Join-Path $runRoot 'cases') $caseName
            $statePath = Join-Path $caseDirectory 'case.json'
            if (Test-Path -LiteralPath $statePath) {
                $state = Read-BenchmarkJson $statePath
            }
            else {
                $stateData = [Ordered]@{}
                foreach ($property in $scenario.PSObject.Properties) {
                    $stateData[$property.Name] = $property.Value
                }
                $stateData.Algorithm = $algorithm
                $stateData.Status = 'Pending'
                $stateData.MemorySamples = @()
                $state = [PSCustomObject]$stateData
            }

            if ($state.Status -eq 'Completed' -or ($state.Status -eq 'Failed' -and -not $RetryFailed)) {
                continue
            }

            Write-Host "$(Get-BenchmarkUtc) START $caseName"
            Set-BenchmarkProperty $state 'Status' 'Running'
            if ($null -eq (Get-BenchmarkProperty $state 'StartedUtc')) {
                Set-BenchmarkProperty $state 'StartedUtc' (Get-BenchmarkUtc)
            }
            Set-BenchmarkProperty $state 'UpdatedUtc' (Get-BenchmarkUtc)
            Write-BenchmarkJson -Path $statePath -Value $state
            Write-BenchmarkJson -Path (Join-Path $runRoot 'progress.json') -Value ([Ordered]@{
                ActiveCase = $caseName
                Phase = 'Timing'
                UpdatedUtc = Get-BenchmarkUtc
            })
            Update-BenchmarkReport

            try {
                if ($null -eq (Get-BenchmarkProperty $state 'Timing')) {
                    $logs = Join-Path $artifactRoot $caseName
                    $filter = "*FullDepthComparisonBenchmark.$algorithm(Scenario: $($scenario.Scenario))"
                    $exitCode = Invoke-BenchmarkProcess -FilePath $benchmarkExe -ArgumentList @(
                        '--full-depth-comparison', '--filter', $filter,
                        '--exporters', 'json', 'markdown', '--artifacts', $logs
                    ) -LogPath (Join-Path $logs 'runner.log')
                    $reports = @(Get-ChildItem -LiteralPath (Join-Path $logs 'results') -Filter '*-report-full-compressed.json' -File -ErrorAction SilentlyContinue)
                    if ($exitCode -ne 0 -or $reports.Count -ne 1) {
                        throw "Benchmark process failed (exit $exitCode); inspect $(Join-Path $logs 'runner.log')."
                    }

                    $document = Read-BenchmarkJson $reports[0].FullName
                    $benchmarks = @($document.Benchmarks)
                    if ($benchmarks.Count -ne 1) {
                        throw 'The per-case filter did not select exactly one benchmark.'
                    }
                    $result = $benchmarks[0]
                    $expectedSuffix = "(Scenario: $($scenario.Scenario))"
                    if ($result.Method -ne $algorithm -or -not $result.FullName.EndsWith($expectedSuffix)) {
                        throw 'Benchmark identity differs from the requested algorithm/scenario.'
                    }
                    if ($null -eq $result.Statistics -or $result.Statistics.N -ne $measurementIterations) {
                        throw "Benchmark did not produce all $measurementIterations retained measurements."
                    }

                    $runnerLog = Get-Content -LiteralPath (Join-Path $logs 'runner.log') -Raw
                    $snapshots = [Regex]::Matches($runnerLog, '(?m)^SEARCH_RESULT_JSON (.+)$')
                    if ($snapshots.Count -eq 0) {
                        throw 'No validated last-result snapshot was emitted during cleanup.'
                    }
                    $lastResult = $snapshots[$snapshots.Count - 1].Groups[1].Value | ConvertFrom-Json
                    if ($lastResult.SearchPolicy -ne $searchPolicy) {
                        throw 'Benchmark search policy differs from the fixed-depth run.'
                    }

                    Set-BenchmarkProperty $state 'Timing' ([PSCustomObject][Ordered]@{
                        MeanMs = [Double]$result.Statistics.Mean / 1000000.0
                        StandardDeviationMs = [Double]$result.Statistics.StandardDeviation / 1000000.0
                        RetainedMeasurements = [Int32]$result.Statistics.N
                        ManagedBytesPerOperation = [Double]$result.Memory.BytesAllocatedPerOperation
                    })
                    Set-BenchmarkProperty $state 'Result' $lastResult
                    [IO.Directory]::CreateDirectory($caseDirectory) | Out-Null
                    Copy-Item -LiteralPath $reports[0].FullName -Destination (Join-Path $caseDirectory 'timing.json') -Force
                    $markdownReports = @(Get-ChildItem -LiteralPath (Join-Path $logs 'results') -Filter '*-report-github.md' -File -ErrorAction SilentlyContinue)
                    if ($markdownReports.Count -gt 0) {
                        Copy-Item -LiteralPath $markdownReports[0].FullName -Destination (Join-Path $caseDirectory 'benchmarkdotnet-report.md') -Force
                    }
                    Set-BenchmarkProperty $state 'Status' 'Profiling'
                    Set-BenchmarkProperty $state 'UpdatedUtc' (Get-BenchmarkUtc)
                    Write-BenchmarkJson -Path $statePath -Value $state
                    Update-BenchmarkReport
                }

                $samples = @((Get-BenchmarkProperty $state 'MemorySamples' @()))
                for ($sampleIndex = $samples.Count; $sampleIndex -lt $memorySamplesPerCase; $sampleIndex++) {
                    Write-BenchmarkJson -Path (Join-Path $runRoot 'progress.json') -Value ([Ordered]@{
                        ActiveCase = $caseName
                        Phase = "Memory $($sampleIndex + 1)/$memorySamplesPerCase"
                        UpdatedUtc = Get-BenchmarkUtc
                    })
                    $memoryLog = Join-Path (Join-Path $artifactRoot $caseName) "memory-$($sampleIndex + 1).json"
                    $exitCode = Invoke-BenchmarkProcess -FilePath $benchmarkExe -ArgumentList @(
                        '--algorithm-memory-case', [String]$scenario.Index, $algorithm
                    ) -LogPath $memoryLog
                    if ($exitCode -ne 0) {
                        throw "Memory worker failed (exit $exitCode); inspect $memoryLog."
                    }
                    $measured = Read-BenchmarkJson $memoryLog
                    if ($measured.Scenario -ne $scenario.Scenario -or $measured.Algorithm -ne $algorithm) {
                        throw 'Memory worker returned a different scenario/backend.'
                    }
                    if ($measured.SearchPolicy -ne $searchPolicy) {
                        throw 'Memory worker search policy differs from the timing run.'
                    }
                    $samples += $measured
                    Set-BenchmarkProperty $state 'MemorySamples' $samples
                    Set-BenchmarkProperty $state 'UpdatedUtc' (Get-BenchmarkUtc)
                    Write-BenchmarkJson -Path $statePath -Value $state
                }

                Set-BenchmarkProperty $state 'Status' 'Completed'
                Set-BenchmarkProperty $state 'CompletedUtc' (Get-BenchmarkUtc)
                Set-BenchmarkProperty $state 'UpdatedUtc' (Get-BenchmarkUtc)
                if ($null -ne $state.PSObject.Properties['Error']) {
                    $state.PSObject.Properties.Remove('Error')
                }
            }
            catch {
                Set-BenchmarkProperty $state 'Status' 'Failed'
                Set-BenchmarkProperty $state 'Error' $_.Exception.Message
                Set-BenchmarkProperty $state 'UpdatedUtc' (Get-BenchmarkUtc)
                Write-Warning "$(Get-BenchmarkUtc) FAILED ${caseName}: $($_.Exception.Message)"
            }

            Write-BenchmarkJson -Path $statePath -Value $state
            Update-BenchmarkReport
            Write-Host "$(Get-BenchmarkUtc) $($state.Status.ToUpperInvariant()) $caseName"
        }
    }

    Write-BenchmarkJson -Path (Join-Path $runRoot 'progress.json') -Value ([Ordered]@{
        ActiveCase = $null
        Phase = 'Finished'
        UpdatedUtc = Get-BenchmarkUtc
    })
    Update-BenchmarkReport
    Write-Host 'All requested cases have been attempted. SUMMARY.md records completed and failed cases.'
}

[IO.Directory]::CreateDirectory($runRoot) | Out-Null
$lockPath = Join-Path $runRoot 'runner.lock'
$lockStream = [IO.File]::Open($lockPath, [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::ReadWrite)
try {
    if ($lockStream.Length -eq 0) {
        $lockStream.SetLength(1)
        $lockStream.Flush()
    }
    try {
        $lockStream.Lock(0, 1)
    }
    catch {
        throw 'A runner is already active for this dataset.'
    }
    try {
        Invoke-FullDepthExperiment
    }
    finally {
        $lockStream.Unlock(0, 1)
    }
}
finally {
    $lockStream.Dispose()
}
