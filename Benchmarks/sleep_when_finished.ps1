#requires -Version 5.1
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

$ErrorActionPreference = 'Stop'
$runRoot = (Resolve-Path -LiteralPath $RunDirectory).Path
$statusPath = Join-Path $runRoot 'sleep-watcher.json'
$script:reason = 'Benchmarks are still running.'
Add-Type -AssemblyName System.Windows.Forms

function Read-Json([String]$Path) {
    return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
}

function Write-State([String]$State, [String]$Detail) {
    $value = [Ordered]@{
        Pid = $PID
        State = $State
        Detail = $Detail
        UpdatedUtc = [DateTime]::UtcNow.ToString('o')
    }
    $temporary = $statusPath + '.tmp'
    $value | ConvertTo-Json | Set-Content -LiteralPath $temporary -Encoding UTF8
    Move-Item -LiteralPath $temporary -Destination $statusPath -Force
}

function Test-Completion {
    $progress = Read-Json (Join-Path $runRoot 'progress.json')
    if ($progress.Phase -ne 'Finished') {
        $script:reason = 'Benchmarks are still running or have not finished normally.'
        return $false
    }
    $info = Read-Json (Join-Path $runRoot 'run-info.json')
    if ($info.MeasurementIterations -ne 16 -or $info.SearchPolicy -ne 'FixedDepthNoExtensions') {
        throw 'Unexpected benchmark run identity or settings.'
    }
    # Windows PowerShell 5.1 emits the JSON array as one pipeline object.
    # Assign it directly instead of nesting it in another array expression.
    $catalog = Read-Json (Join-Path $runRoot 'scenarios.json')
    $algorithms = @($info.Algorithms)
    $timedCases = $catalog.Count * $algorithms.Count
    $directories = @(Get-ChildItem -LiteralPath (Join-Path $runRoot 'cases') -Directory)
    if ($catalog.Count -ne 53 -or $info.TimedCases -ne $timedCases -or $directories.Count -ne $timedCases) {
        throw 'The complete requested scenario matrix is missing.'
    }
    foreach ($scenario in $catalog) {
        foreach ($algorithm in $algorithms) {
            $caseDirectory = Join-Path (Join-Path $runRoot 'cases') ($scenario.Scenario + '--' + $algorithm)
            $case = Read-Json (Join-Path $caseDirectory 'case.json')
            if ($case.Scenario -ne $scenario.Scenario -or $case.Algorithm -ne $algorithm -or $case.Status -ne 'Completed') {
                throw 'An expected benchmark case is incomplete or failed.'
            }
            if ($case.Timing.RetainedMeasurements -ne 16 -or @($case.MemorySamples).Count -ne 3 -or $case.Result.SearchPolicy -ne 'FixedDepthNoExtensions') {
                throw 'A benchmark case is missing retained measurements or memory samples.'
            }
            $timing = Read-Json (Join-Path $caseDirectory 'timing.json')
            if (@($timing.Benchmarks).Count -ne 1 -or $timing.Benchmarks[0].Statistics.N -ne 16) {
                throw 'A raw timing export is incomplete.'
            }
        }
    }
    if (Test-Path -LiteralPath (Join-Path $runRoot 'report-error.json')) {
        throw 'The final report has an unresolved generation error.'
    }
    $summaryPath = Join-Path $runRoot 'SUMMARY.md'
    $summary = Get-Content -LiteralPath $summaryPath -Raw
    if (-not $summary.Contains('**FINISHED**') -or -not $summary.Contains("$timedCases/$timedCases cases complete; 0 failed; 53/53")) {
        throw 'The final summary does not confirm successful completion.'
    }
    if ((Get-Item -LiteralPath $summaryPath).LastWriteTimeUtc -lt (Get-Item -LiteralPath (Join-Path $runRoot 'progress.json')).LastWriteTimeUtc) {
        throw 'The summary has not been refreshed after completion.'
    }
    foreach ($chart in @('depth-scaling', 'time-vs-pieces', 'time-vs-legal-moves', 'memory-scaling')) {
        foreach ($extension in @('png', 'svg')) {
            if (-not (Test-Path -LiteralPath (Join-Path $runRoot ('charts/' + $chart + '.' + $extension)))) {
                throw 'A final chart export is missing.'
            }
        }
    }
    # The runner holds this byte lock until its final report refresh has returned.
    $lock = [IO.File]::Open((Join-Path $runRoot 'runner.lock'), [IO.FileMode]::Open, [IO.FileAccess]::ReadWrite, [IO.FileShare]::ReadWrite)
    try {
        $lock.Lock(0, 1)
        $lock.Unlock(0, 1)
    }
    finally {
        $lock.Dispose()
    }
    $script:reason = "All $timedCases cases, retained samples, final summary and charts are complete."
    return $true
}

if ($CheckOnly) {
    $ready = $false
    try {
        $ready = Test-Completion
    }
    catch {
        $script:reason = $_.Exception.Message
    }
    @{ Ready = $ready; Reason = $script:reason; Action = 'Check only; no sleep requested.' } | ConvertTo-Json
    exit 0
}

$created = $false
$mutex = [Threading.Mutex]::new($true, 'Local\Triglav.FixedDepth16.SleepOnCompletion', [Ref]$created)
if (-not $created) {
    $mutex.Dispose()
    throw 'A benchmark sleep watcher is already active.'
}
try {
    if (Test-Path -LiteralPath $statusPath) {
        $previous = Read-Json $statusPath
        if ($previous.State -in @('SleepScheduled', 'SleepRequested', 'SleepReturned')) {
            throw 'Sleep has already been requested for this run.'
        }
    }
    while ($true) {
        $ready = $false
        try {
            $ready = Test-Completion
        }
        catch {
            $script:reason = $_.Exception.Message
        }
        if ($ready) {
            if ($SleepDelaySeconds -gt 0) {
                Write-State 'SleepScheduled' ('All checks passed; automatic sleep in ' + $SleepDelaySeconds + ' seconds.')
                Start-Sleep -Seconds $SleepDelaySeconds
                if (-not (Test-Completion)) {
                    continue
                }
            }
            Write-State 'SleepRequested' $script:reason
            # Windows Forms handles the suspend privilege. This call has no user
            # prompt and requests sleep, without forcing applications to close.
            $accepted = [System.Windows.Forms.Application]::SetSuspendState([System.Windows.Forms.PowerState]::Suspend, $false, $false)
            if (-not $accepted) {
                throw 'Windows rejected the sleep request.'
            }
            Write-State 'SleepReturned' 'Windows accepted the sleep request; the watcher will not repeat it.'
            break
        }
        Write-State 'Waiting' $script:reason
        Start-Sleep -Seconds $PollSeconds
    }
}
catch {
    Write-State 'Error' $_.Exception.Message
    throw
}
finally {
    $mutex.ReleaseMutex()
    $mutex.Dispose()
}
