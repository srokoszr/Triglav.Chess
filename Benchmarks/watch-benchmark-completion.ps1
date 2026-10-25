#requires -Version 5.1
<#
.SYNOPSIS
Validates a benchmark run and optionally puts Windows to sleep after completion.

.DESCRIPTION
Use -CheckOnly to perform the same validation without waiting or requesting
sleep. Expectations are read from run-info.json instead of being hard-coded for
a particular benchmark matrix.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [String]$RunDirectory,

    [ValidateRange(10, 600)]
    [Int32]$PollSeconds = 60,

    [ValidateRange(0, 300)]
    [Int32]$SleepDelaySeconds = 0,

    [Switch]$CheckOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'Benchmark.Common.psm1') -Force

$runRoot = (Resolve-Path -LiteralPath $RunDirectory).Path
$statusPath = Join-Path $runRoot 'sleep-watcher.json'
$script:reason = 'Benchmarks are still running.'

function Write-WatcherState {
    param(
        [Parameter(Mandatory = $true)][String]$State,
        [Parameter(Mandatory = $true)][String]$Detail
    )

    Write-BenchmarkJson -Path $statusPath -Value ([Ordered]@{
        Pid = $PID
        State = $State
        Detail = $Detail
        UpdatedUtc = Get-BenchmarkUtc
    })
}

function Test-BenchmarkCompletion {
    $progressPath = Join-Path $runRoot 'progress.json'
    if (-not (Test-Path -LiteralPath $progressPath)) {
        $script:reason = 'The benchmark has not written progress yet.'
        return $false
    }
    $progress = Read-BenchmarkJson $progressPath
    if ($progress.Phase -ne 'Finished') {
        $script:reason = 'Benchmarks are still running or did not finish normally.'
        return $false
    }

    $info = Read-BenchmarkJson (Join-Path $runRoot 'run-info.json')
    $catalogDocument = Read-BenchmarkJson (Join-Path $runRoot 'scenarios.json')
    $catalog = @($catalogDocument)
    $algorithms = @($info.Algorithms)
    $timedCases = $catalog.Count * $algorithms.Count
    $measurementIterations = [Int32]$info.MeasurementIterations
    $memorySamplesPerCase = [Int32]$info.MemorySamplesPerCase
    $searchPolicy = [String]$info.SearchPolicy
    if ($catalog.Count -ne $info.Scenarios -or $timedCases -ne $info.TimedCases) {
        throw 'The scenario catalog does not match the run metadata.'
    }

    $casesDirectory = Join-Path $runRoot 'cases'
    $directories = @(Get-ChildItem -LiteralPath $casesDirectory -Directory)
    if ($directories.Count -ne $timedCases) {
        throw "Expected $timedCases case directories, but found $($directories.Count)."
    }
    foreach ($scenario in $catalog) {
        foreach ($algorithm in $algorithms) {
            $caseDirectory = Join-Path $casesDirectory ($scenario.Scenario + '--' + $algorithm)
            $case = Read-BenchmarkJson (Join-Path $caseDirectory 'case.json')
            if ($case.Scenario -ne $scenario.Scenario -or $case.Algorithm -ne $algorithm -or $case.Status -ne 'Completed') {
                throw "Case '$($scenario.Scenario)--$algorithm' is incomplete or failed."
            }
            if ($case.Timing.RetainedMeasurements -ne $measurementIterations -or
                @($case.MemorySamples).Count -ne $memorySamplesPerCase -or
                $case.Result.SearchPolicy -ne $searchPolicy) {
                throw "Case '$($scenario.Scenario)--$algorithm' is missing required timing, memory, or policy evidence."
            }
            $timing = Read-BenchmarkJson (Join-Path $caseDirectory 'timing.json')
            if (@($timing.Benchmarks).Count -ne 1 -or $timing.Benchmarks[0].Statistics.N -ne $measurementIterations) {
                throw "Raw timing export for '$($scenario.Scenario)--$algorithm' is incomplete."
            }
        }
    }

    if (Test-Path -LiteralPath (Join-Path $runRoot 'report-error.json')) {
        throw 'The final report has an unresolved generation error.'
    }
    $summaryPath = Join-Path $runRoot 'SUMMARY.md'
    $summary = Get-Content -LiteralPath $summaryPath -Raw
    $expectedStatus = "$timedCases/$timedCases cases complete; 0 failed; $($catalog.Count)/$($catalog.Count) matched scenario groups"
    if (-not $summary.Contains('**FINISHED**') -or -not $summary.Contains($expectedStatus)) {
        throw 'The final summary does not confirm successful completion.'
    }
    if ((Get-Item -LiteralPath $summaryPath).LastWriteTimeUtc -lt (Get-Item -LiteralPath $progressPath).LastWriteTimeUtc) {
        throw 'The summary has not been refreshed after completion.'
    }
    foreach ($chart in @('depth-scaling.svg', 'time-vs-pieces.svg', 'time-vs-legal-moves.svg', 'memory-scaling.svg')) {
        if (-not (Test-Path -LiteralPath (Join-Path (Join-Path $runRoot 'charts') $chart))) {
            throw "Final chart '$chart' is missing."
        }
    }

    $lock = [IO.File]::Open((Join-Path $runRoot 'runner.lock'), [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::ReadWrite)
    try {
        try {
            $lock.Lock(0, 1)
            $lock.Unlock(0, 1)
        }
        catch {
            $script:reason = 'The runner still owns its completion lock.'
            return $false
        }
    }
    finally {
        $lock.Dispose()
    }

    $script:reason = "All $timedCases cases, retained samples, final summary, and charts are complete."
    return $true
}

if ($CheckOnly) {
    $ready = $false
    try {
        $ready = Test-BenchmarkCompletion
    }
    catch {
        $script:reason = $_.Exception.Message
    }
    [Ordered]@{
        Ready = $ready
        Reason = $script:reason
        Action = 'Check only; no sleep requested.'
    } | ConvertTo-Json
    return
}

Add-Type -AssemblyName System.Windows.Forms
$mutexSuffix = (Get-BenchmarkTextSha256 $runRoot.ToLowerInvariant()).Substring(0, 16)
$created = $false
$mutex = [Threading.Mutex]::new($true, ('Local\Triglav.Benchmark.SleepOnCompletion.' + $mutexSuffix), [Ref]$created)
if (-not $created) {
    $mutex.Dispose()
    throw 'A benchmark sleep watcher is already active for this run.'
}

try {
    if (Test-Path -LiteralPath $statusPath) {
        $previous = Read-BenchmarkJson $statusPath
        if ($previous.State -in @('SleepScheduled', 'SleepRequested', 'SleepReturned')) {
            throw 'Sleep has already been requested for this run.'
        }
    }
    while ($true) {
        $ready = $false
        try {
            $ready = Test-BenchmarkCompletion
        }
        catch {
            $script:reason = $_.Exception.Message
        }
        if ($ready) {
            if ($SleepDelaySeconds -gt 0) {
                Write-WatcherState 'SleepScheduled' ("All checks passed; automatic sleep in $SleepDelaySeconds seconds.")
                Start-Sleep -Seconds $SleepDelaySeconds
                if (-not (Test-BenchmarkCompletion)) {
                    continue
                }
            }
            Write-WatcherState 'SleepRequested' $script:reason
            $accepted = [Windows.Forms.Application]::SetSuspendState([Windows.Forms.PowerState]::Suspend, $false, $false)
            if (-not $accepted) {
                throw 'Windows rejected the sleep request.'
            }
            Write-WatcherState 'SleepReturned' 'Windows accepted the sleep request; the watcher will not repeat it.'
            break
        }
        Write-WatcherState 'Waiting' $script:reason
        Start-Sleep -Seconds $PollSeconds
    }
}
catch {
    Write-WatcherState 'Error' $_.Exception.Message
    throw
}
finally {
    $mutex.ReleaseMutex()
    $mutex.Dispose()
}
