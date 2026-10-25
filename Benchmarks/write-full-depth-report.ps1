#requires -Version 5.1
<#
.SYNOPSIS
Builds the local CSV, Markdown summary, and SVG charts for a full-depth run.
#>
[CmdletBinding(DefaultParameterSetName = 'Name')]
param(
    [Parameter(ParameterSetName = 'Name')]
    [ValidatePattern('^[A-Za-z0-9_-]+$')]
    [String]$RunName = 'MinMaxConsolidated',

    [Parameter(Mandatory = $true, ParameterSetName = 'Path')]
    [String]$RunDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'Benchmark.Common.psm1') -Force

$repo = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$runRoot = if ($PSCmdlet.ParameterSetName -eq 'Path') {
    (Resolve-Path -LiteralPath $RunDirectory).Path
}
else {
    (Resolve-Path -LiteralPath (Join-Path $repo ('Benchmarks/Results/' + $RunName))).Path
}

function Get-PropertyValue {
    param(
        [AllowNull()]$Object,
        [Parameter(Mandatory = $true)][String]$Name,
        [AllowNull()]$Default = $null
    )

    if ($null -eq $Object -or $null -eq $Object.PSObject.Properties[$Name]) {
        return $Default
    }
    return $Object.$Name
}

function Format-Number {
    param(
        [AllowNull()]$Value,
        [String]$Format = '0.####'
    )

    if ($null -eq $Value) {
        return 'N/A'
    }
    return ([Double]$Value).ToString($Format, [Globalization.CultureInfo]::InvariantCulture)
}

function ConvertTo-XmlEscapedText {
    param([AllowNull()]$Value)

    if ($null -eq $Value) { return '' }
    return [Security.SecurityElement]::Escape([String]$Value)
}

function Format-SvgNumber {
    param([Double]$Value)

    $Value.ToString('0.###', [Globalization.CultureInfo]::InvariantCulture)
}

function Write-SvgChart {
    param(
        [Parameter(Mandatory = $true)][String]$Path,
        [Parameter(Mandatory = $true)][String]$Title,
        [Parameter(Mandatory = $true)][String]$XLabel,
        [Parameter(Mandatory = $true)][String]$YLabel,
        [Parameter(Mandatory = $true)][Object[]]$Series,
        [Switch]$LogY
    )

    $allPoints = @($Series | ForEach-Object { @($_.Points) } | Where-Object {
        $null -ne $_.X -and $null -ne $_.Y -and (-not $LogY -or [Double]$_.Y -gt 0)
    })
    if ($allPoints.Count -eq 0) {
        return
    }

    $width = 1100.0
    $height = 650.0
    $left = 95.0
    $right = 250.0
    $top = 70.0
    $bottom = 80.0
    $plotWidth = $width - $left - $right
    $plotHeight = $height - $top - $bottom
    $xValues = @($allPoints | ForEach-Object { [Double]$_.X })
    $rawYValues = @($allPoints | ForEach-Object { [Double]$_.Y })
    $yValues = if ($LogY) { @($rawYValues | ForEach-Object { [Math]::Log10($_) }) } else { $rawYValues }
    $xMin = [Double](($xValues | Measure-Object -Minimum).Minimum)
    $xMax = [Double](($xValues | Measure-Object -Maximum).Maximum)
    $yMin = [Double](($yValues | Measure-Object -Minimum).Minimum)
    $yMax = [Double](($yValues | Measure-Object -Maximum).Maximum)
    if ($xMax -eq $xMin) { $xMin -= 0.5; $xMax += 0.5 }
    if ($yMax -eq $yMin) { $yMin -= 0.5; $yMax += 0.5 }
    $xPadding = ($xMax - $xMin) * 0.04
    $yPadding = ($yMax - $yMin) * 0.08
    $xMin -= $xPadding
    $xMax += $xPadding
    $yMin -= $yPadding
    $yMax += $yPadding

    $mapX = { param([Double]$Value) $left + (($Value - $xMin) / ($xMax - $xMin)) * $plotWidth }
    $mapY = { param([Double]$Value) $top + $plotHeight - (($Value - $yMin) / ($yMax - $yMin)) * $plotHeight }
    $palette = @('#2563eb', '#dc2626', '#059669', '#9333ea', '#d97706', '#0891b2', '#4f46e5', '#be123c')
    $svg = [Collections.Generic.List[String]]::new()
    $svg.Add('<?xml version="1.0" encoding="UTF-8"?>')
    $svg.Add(('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {0} {1}" role="img" aria-labelledby="title desc">' -f $width, $height))
    $svg.Add('<style>text{font-family:Segoe UI,Arial,sans-serif;fill:#1f2937}.grid{stroke:#d1d5db;stroke-width:1}.axis{stroke:#374151;stroke-width:1.5}.legend{font-size:14px}.tick{font-size:12px}.label{font-size:15px;font-weight:600}</style>')
    $svg.Add('<rect width="100%" height="100%" fill="#fff"/>')
    $svg.Add(('<title id="title">{0}</title><desc id="desc">Generated from completed benchmark cases.</desc>' -f (ConvertTo-XmlEscapedText $Title)))
    $svg.Add(('<text x="{0}" y="34" text-anchor="middle" font-size="22" font-weight="600">{1}</text>' -f ($width / 2), (ConvertTo-XmlEscapedText $Title)))

    for ($tick = 0; $tick -le 5; $tick++) {
        $fraction = $tick / 5.0
        $x = $left + $fraction * $plotWidth
        $y = $top + $plotHeight - $fraction * $plotHeight
        $xValue = $xMin + $fraction * ($xMax - $xMin)
        $scaledY = $yMin + $fraction * ($yMax - $yMin)
        $yValue = if ($LogY) { [Math]::Pow(10.0, $scaledY) } else { $scaledY }
        $svg.Add(('<line class="grid" x1="{0}" y1="{1}" x2="{0}" y2="{2}"/>' -f (Format-SvgNumber $x), $top, ($top + $plotHeight)))
        $svg.Add(('<line class="grid" x1="{0}" y1="{1}" x2="{2}" y2="{1}"/>' -f $left, (Format-SvgNumber $y), ($left + $plotWidth)))
        $svg.Add(('<text class="tick" x="{0}" y="{1}" text-anchor="middle">{2}</text>' -f (Format-SvgNumber $x), ($top + $plotHeight + 22), (Format-Number $xValue '0.##')))
        $svg.Add(('<text class="tick" x="{0}" y="{1}" text-anchor="end">{2}</text>' -f ($left - 10), (Format-SvgNumber ($y + 4)), (Format-Number $yValue '0.###')))
    }
    $svg.Add(('<line class="axis" x1="{0}" y1="{1}" x2="{2}" y2="{1}"/>' -f $left, ($top + $plotHeight), ($left + $plotWidth)))
    $svg.Add(('<line class="axis" x1="{0}" y1="{1}" x2="{0}" y2="{2}"/>' -f $left, $top, ($top + $plotHeight)))
    $svg.Add(('<text class="label" x="{0}" y="{1}" text-anchor="middle">{2}</text>' -f ($left + $plotWidth / 2), ($height - 25), (ConvertTo-XmlEscapedText $XLabel)))
    $svg.Add(('<text class="label" transform="translate(24 {0}) rotate(-90)" text-anchor="middle">{1}</text>' -f ($top + $plotHeight / 2), (ConvertTo-XmlEscapedText $YLabel)))

    for ($seriesIndex = 0; $seriesIndex -lt $Series.Count; $seriesIndex++) {
        $item = $Series[$seriesIndex]
        $color = $palette[$seriesIndex % $palette.Count]
        $points = @($item.Points | Where-Object { $null -ne $_.X -and $null -ne $_.Y -and (-not $LogY -or [Double]$_.Y -gt 0) } | Sort-Object X)
        $coordinates = @($points | ForEach-Object {
            $displayY = if ($LogY) { [Math]::Log10([Double]$_.Y) } else { [Double]$_.Y }
            [PSCustomObject]@{
                X = & $mapX ([Double]$_.X)
                Y = & $mapY $displayY
                Label = Get-PropertyValue $_ 'Label' ''
                RawX = [Double]$_.X
                RawY = [Double]$_.Y
            }
        })
        if ((Get-PropertyValue $item 'Connect' $false) -and $coordinates.Count -gt 1) {
            $polyline = ($coordinates | ForEach-Object { (Format-SvgNumber $_.X) + ',' + (Format-SvgNumber $_.Y) }) -join ' '
            $svg.Add(('<polyline fill="none" stroke="{0}" stroke-width="2.5" points="{1}"/>' -f $color, $polyline))
        }
        foreach ($point in $coordinates) {
            $tooltip = '{0}: x={1}, y={2}' -f $item.Name, (Format-Number $point.RawX), (Format-Number $point.RawY)
            if ($point.Label) { $tooltip += ', ' + $point.Label }
            $svg.Add(('<circle cx="{0}" cy="{1}" r="4.5" fill="{2}" fill-opacity="0.82"><title>{3}</title></circle>' -f (Format-SvgNumber $point.X), (Format-SvgNumber $point.Y), $color, (ConvertTo-XmlEscapedText $tooltip)))
        }
        $legendY = $top + 18 + $seriesIndex * 24
        $svg.Add(('<line x1="{0}" y1="{1}" x2="{2}" y2="{1}" stroke="{3}" stroke-width="3"/><circle cx="{4}" cy="{1}" r="4" fill="{3}"/><text class="legend" x="{5}" y="{6}">{7}</text>' -f ($left + $plotWidth + 20), $legendY, ($left + $plotWidth + 48), $color, ($left + $plotWidth + 34), ($left + $plotWidth + 58), ($legendY + 5), (ConvertTo-XmlEscapedText $item.Name)))
    }
    $svg.Add('</svg>')
    Write-BenchmarkText -Path $Path -Text (($svg -join [Environment]::NewLine) + [Environment]::NewLine)
}

function New-DepthSeries {
    param(
        [Parameter(Mandatory = $true)][Object[]]$Rows,
        [Parameter(Mandatory = $true)][String]$ValueProperty,
        [Parameter(Mandatory = $true)][String[]]$Algorithms
    )

    $series = @()
    foreach ($algorithm in $Algorithms) {
        $points = @()
        $algorithmRows = @($Rows | Where-Object Algorithm -eq $algorithm)
        foreach ($group in @($algorithmRows | Group-Object FullMoveDepth | Sort-Object { [Int32]$_.Name })) {
            $median = Get-BenchmarkMedian @($group.Group | ForEach-Object { $_.$ValueProperty })
            if ($null -ne $median) {
                $points += [PSCustomObject]@{ X = [Double]$group.Name; Y = $median; Label = 'median' }
            }
        }
        $series += [PSCustomObject]@{ Name = $algorithm; Points = $points; Connect = $true }
    }
    return $series
}

function New-ScatterSeries {
    param(
        [Parameter(Mandatory = $true)][Object[]]$Rows,
        [Parameter(Mandatory = $true)][String]$XProperty,
        [Parameter(Mandatory = $true)][String]$YProperty,
        [Parameter(Mandatory = $true)][String[]]$Algorithms
    )

    $series = @()
    foreach ($algorithm in $Algorithms) {
        $points = @($Rows | Where-Object Algorithm -eq $algorithm | ForEach-Object {
            [PSCustomObject]@{
                X = $_.$XProperty
                Y = $_.$YProperty
                Label = $_.Scenario
            }
        })
        $series += [PSCustomObject]@{ Name = $algorithm; Points = $points; Connect = $false }
    }
    return $series
}

$runInfo = Read-BenchmarkJson (Join-Path $runRoot 'run-info.json')
$reportRunName = [String](Get-PropertyValue $runInfo 'RunName' (Split-Path -Leaf $runRoot))
$reportPolicy = [String](Get-PropertyValue $runInfo 'SearchPolicy' 'Historical fixed-depth policy')
$canResumeWithCurrentRunner = $null -ne $runInfo.PSObject.Properties['RunName'] -and $null -ne $runInfo.PSObject.Properties['SearchPolicy']
$catalogDocument = Read-BenchmarkJson (Join-Path $runRoot 'scenarios.json')
$catalog = @($catalogDocument)
$algorithms = @($runInfo.Algorithms | ForEach-Object { [String]$_ })
$rows = @()
$casesByKey = @{}

foreach ($scenario in $catalog) {
    foreach ($algorithm in $algorithms) {
        $casePath = Join-Path (Join-Path (Join-Path $runRoot 'cases') ($scenario.Scenario + '--' + $algorithm)) 'case.json'
        $case = if (Test-Path -LiteralPath $casePath) { Read-BenchmarkJson $casePath } else { $null }
        if ($null -ne $case) {
            $casesByKey[$scenario.Scenario + [Char]31 + $algorithm] = $case
        }
        $timing = Get-PropertyValue $case 'Timing'
        $samples = @((Get-PropertyValue $case 'MemorySamples' @()))
        $committedMiB = if ($samples.Count -gt 0) { (Get-BenchmarkMean @($samples | ForEach-Object PeakCommittedBytes)) / 1MB } else { $null }
        $residentMiB = if ($samples.Count -gt 0) { (Get-BenchmarkMean @($samples | ForEach-Object PeakResidentBytes)) / 1MB } else { $null }
        $rows += [PSCustomObject][Ordered]@{
            Scenario = $scenario.Scenario
            Kind = $scenario.Kind
            Side = $scenario.Side
            FullMoveDepth = [Int32]$scenario.FullMoveDepth
            Plies = [Int32]$scenario.Plies
            PieceCount = [Int32]$scenario.PieceCount
            LegalMoveCount = [Int32]$scenario.LegalMoveCount
            HistoryPositions = [Int32]$scenario.HistoryPositions
            Algorithm = $algorithm
            Status = if ($null -eq $case) { 'Pending' } else { [String]$case.Status }
            MeanMs = if ($null -eq $timing) { $null } else { [Double]$timing.MeanMs }
            StandardDeviationMs = if ($null -eq $timing) { $null } else { [Double]$timing.StandardDeviationMs }
            ManagedBytesPerOperation = if ($null -eq $timing) { $null } else { [Double]$timing.ManagedBytesPerOperation }
            PeakCommittedMiB = $committedMiB
            PeakResidentMiB = $residentMiB
            Error = Get-PropertyValue $case 'Error' ''
        }
    }
}

$csvText = ($rows | ConvertTo-Csv -NoTypeInformation) -join [Environment]::NewLine
Write-BenchmarkText -Path (Join-Path $runRoot 'summary.csv') -Text ($csvText + [Environment]::NewLine)

$completed = @($rows | Where-Object Status -eq 'Completed')
$failures = @($rows | Where-Object Status -eq 'Failed')
$timedCases = $catalog.Count * $algorithms.Count
$matchedGroups = @($completed | Group-Object Scenario | Where-Object {
    @($_.Group.Algorithm | Select-Object -Unique).Count -eq $algorithms.Count
})
$progressPath = Join-Path $runRoot 'progress.json'
$progress = if (Test-Path -LiteralPath $progressPath) { Read-BenchmarkJson $progressPath } else { $null }
$phase = if ($null -eq $progress) { 'Not started' } else { [String]$progress.Phase }
$isFinal = $phase -eq 'Finished'

$chartDirectory = Join-Path $runRoot 'charts'
[IO.Directory]::CreateDirectory($chartDirectory) | Out-Null
$plotStatePath = Join-Path $runRoot 'plot-state.json'
$lastPlotted = 0
$plotGenerator = ''
if (Test-Path -LiteralPath $plotStatePath) {
    $plotState = Read-BenchmarkJson $plotStatePath
    $lastPlotted = [Int32]$plotState.CompletedCases
    $plotGenerator = [String](Get-PropertyValue $plotState 'Generator' '')
}
$requiredCharts = @('depth-scaling.svg', 'time-vs-pieces.svg', 'time-vs-legal-moves.svg', 'memory-scaling.svg')
$missingChart = @($requiredCharts | Where-Object { -not (Test-Path -LiteralPath (Join-Path $chartDirectory $_)) }).Count -gt 0
$shouldPlot = $completed.Count -gt 0 -and ($isFinal -or $missingChart -or $plotGenerator -ne 'PowerShellSvgV1' -or ($completed.Count - $lastPlotted) -ge $algorithms.Count)
if ($shouldPlot) {
    Write-SvgChart -Path (Join-Path $chartDirectory 'depth-scaling.svg') -Title 'Median measured runtime by depth' -XLabel 'Nominal full-move depth' -YLabel 'Median mean time (ms, log scale)' -Series (New-DepthSeries $completed 'MeanMs' $algorithms) -LogY
    Write-SvgChart -Path (Join-Path $chartDirectory 'time-vs-pieces.svg') -Title 'Measured runtime vs root piece count' -XLabel 'Root piece count' -YLabel 'Mean time (ms, log scale)' -Series (New-ScatterSeries $completed 'PieceCount' 'MeanMs' $algorithms) -LogY
    Write-SvgChart -Path (Join-Path $chartDirectory 'time-vs-legal-moves.svg') -Title 'Measured runtime vs root legal moves' -XLabel 'Root legal moves' -YLabel 'Mean time (ms, log scale)' -Series (New-ScatterSeries $completed 'LegalMoveCount' 'MeanMs' $algorithms) -LogY
    Write-SvgChart -Path (Join-Path $chartDirectory 'memory-scaling.svg') -Title 'Median peak committed memory by depth' -XLabel 'Nominal full-move depth' -YLabel 'Median peak committed memory (MiB)' -Series (New-DepthSeries $completed 'PeakCommittedMiB' $algorithms)
    Write-BenchmarkJson -Path $plotStatePath -Value ([Ordered]@{
        CompletedCases = $completed.Count
        Generator = 'PowerShellSvgV1'
        UpdatedUtc = Get-BenchmarkUtc
    })
}

$lines = [Collections.Generic.List[String]]::new()
$statusLabel = if ($isFinal) { 'FINISHED' } elseif ($phase.StartsWith('Stopped')) { 'STOPPED' } else { 'IN PROGRESS' }
$lines.Add('# Full-depth algorithm benchmark')
$lines.Add('')
$lines.Add("**$statusLabel** - $($completed.Count)/$timedCases cases complete; $($failures.Count) failed; $($matchedGroups.Count)/$($catalog.Count) matched scenario groups.")
$lines.Add('')
$lines.Add("Run: ``$reportRunName`` on $($runInfo.Date); configuration: $($runInfo.Configuration); policy: ``$reportPolicy``.")
$lines.Add('')

if ($matchedGroups.Count -gt 0) {
    $wins = [Ordered]@{}
    foreach ($algorithm in $algorithms) { $wins[$algorithm] = 0 }
    foreach ($group in $matchedGroups) {
        $winner = $group.Group | Sort-Object MeanMs | Select-Object -First 1
        $wins[$winner.Algorithm]++
    }
    $lines.Add('## Comparative overview')
    $lines.Add('')
    $lines.Add("| Algorithm | Lowest mean cases | Geometric mean vs $($algorithms[0]) | Mean committed MiB | Mean resident MiB |")
    $lines.Add('| --- | ---: | ---: | ---: | ---: |')
    foreach ($algorithm in $algorithms) {
        $algorithmRows = @($matchedGroups | ForEach-Object { $_.Group | Where-Object Algorithm -eq $algorithm })
        $ratios = @()
        foreach ($group in $matchedGroups) {
            $baseline = $group.Group | Where-Object Algorithm -eq $algorithms[0] | Select-Object -First 1
            $candidate = $group.Group | Where-Object Algorithm -eq $algorithm | Select-Object -First 1
            if ($baseline.MeanMs -gt 0 -and $candidate.MeanMs -gt 0) {
                $ratios += [Double]$candidate.MeanMs / [Double]$baseline.MeanMs
            }
        }
        $geometricMean = if ($ratios.Count -gt 0) {
            [Math]::Exp((($ratios | ForEach-Object { [Math]::Log($_) } | Measure-Object -Average).Average))
        }
        else { $null }
        $meanCommitted = Get-BenchmarkMean @($algorithmRows | ForEach-Object PeakCommittedMiB)
        $meanResident = Get-BenchmarkMean @($algorithmRows | ForEach-Object PeakResidentMiB)
        $lines.Add(('| {0} | {1} | {2}x | {3} | {4} |' -f $algorithm, $wins[$algorithm], (Format-Number $geometricMean '0.###'), (Format-Number $meanCommitted '0.##'), (Format-Number $meanResident '0.##')))
    }
    $lines.Add('')
    $lines.Add("Ratios below 1x are faster than $($algorithms[0]). Compare small timing differences with the per-case standard deviations.")
    $lines.Add('')
}
else {
    $lines.Add('No scenario has completed all requested algorithms yet.')
    $lines.Add('')
}

$lines.Add('## Measurement scope')
$lines.Add('')
$lines.Add("- $($catalog.Count) scenario/depth combinations, $($algorithms.Count) algorithms, and $timedCases timed cases.")
$lines.Add("- $($runInfo.MeasurementIterations) retained measurements, $($runInfo.WarmupIterations) warm-up, and $($runInfo.MemorySamplesPerCase) isolated memory samples per case.")
$lines.Add('- Timing includes history reset/restore, native search allocation, result extraction, and cleanup. Process peaks include the CLR, JIT, loaded code, and native heap.')
$lines.Add("- Native DLL SHA-256: ``$($runInfo.NativeDllSha256)``. Failed or missing cases remain explicit and are never replaced by estimates.")
$lines.Add('')

if (@($requiredCharts | Where-Object { Test-Path -LiteralPath (Join-Path $chartDirectory $_) }).Count -gt 0) {
    $lines.Add('## Charts')
    $lines.Add('')
    $lines.Add('Depth charts use the median across completed roots at each depth. Scatter charts show every completed case. Timing axes are logarithmic; all plotted points are measurements, not projections.')
    $lines.Add('')
    foreach ($chart in @(
        @('depth-scaling', 'Median measured runtime by depth'),
        @('time-vs-pieces', 'Runtime vs root piece count'),
        @('time-vs-legal-moves', 'Runtime vs root legal moves'),
        @('memory-scaling', 'Peak committed memory by depth')
    )) {
        if (Test-Path -LiteralPath (Join-Path $chartDirectory ($chart[0] + '.svg'))) {
            $lines.Add("### $($chart[1])")
            $lines.Add('')
            $lines.Add("![$($chart[1])](charts/$($chart[0]).svg)")
            $lines.Add('')
        }
    }
}

$lines.Add('## Full per-case timing and memory table')
$lines.Add('')
$lines.Add('Mean/SD are milliseconds per search. Memory is the mean committed/resident process peak from isolated samples. `N/A` means not measured.')
$lines.Add('')
$lines.Add('| Scenario | Pieces | Legal moves | History | Algorithm | Status | Mean ms | SD ms | Managed B/op | Process peaks MiB |')
$lines.Add('| --- | ---: | ---: | ---: | --- | --- | ---: | ---: | ---: | ---: |')
foreach ($row in $rows) {
    $memory = if ($null -eq $row.PeakCommittedMiB) { 'N/A' } else { (Format-Number $row.PeakCommittedMiB '0.##') + ' / ' + (Format-Number $row.PeakResidentMiB '0.##') }
    $lines.Add(('| {0} | {1} | {2} | {3} | {4} | {5} | {6} | {7} | {8} | {9} |' -f
        $row.Scenario, $row.PieceCount, $row.LegalMoveCount, $row.HistoryPositions, $row.Algorithm, $row.Status,
        (Format-Number $row.MeanMs), (Format-Number $row.StandardDeviationMs), (Format-Number $row.ManagedBytesPerOperation '0.##'), $memory))
}
$lines.Add('')

$lines.Add('## Result agreement')
$lines.Add('')
$lines.Add('Exact matches compare move, piece, result code, distance, and raw evaluation for completed cases.')
$lines.Add('')
$lines.Add('| Pair | Comparable scenarios | Exact matches |')
$lines.Add('| --- | ---: | ---: |')
for ($leftIndex = 0; $leftIndex -lt $algorithms.Count; $leftIndex++) {
    for ($rightIndex = $leftIndex + 1; $rightIndex -lt $algorithms.Count; $rightIndex++) {
        $left = $algorithms[$leftIndex]
        $right = $algorithms[$rightIndex]
        $comparable = 0
        $same = 0
        foreach ($scenario in $catalog) {
            $leftCase = $casesByKey[$scenario.Scenario + [Char]31 + $left]
            $rightCase = $casesByKey[$scenario.Scenario + [Char]31 + $right]
            $leftResult = Get-PropertyValue $leftCase 'Result'
            $rightResult = Get-PropertyValue $rightCase 'Result'
            if ($null -ne $leftCase -and $null -ne $rightCase -and $leftCase.Status -eq 'Completed' -and $rightCase.Status -eq 'Completed' -and $null -ne $leftResult -and $null -ne $rightResult) {
                $comparable++
                $leftSignature = [Ordered]@{ Move = $leftResult.Move; Piece = $leftResult.Piece; ResultCode = $leftResult.ResultCode; InNMoves = $leftResult.InNMoves; EvaluationValue = $leftResult.EvaluationValue }
                $rightSignature = [Ordered]@{ Move = $rightResult.Move; Piece = $rightResult.Piece; ResultCode = $rightResult.ResultCode; InNMoves = $rightResult.InNMoves; EvaluationValue = $rightResult.EvaluationValue }
                if ((ConvertTo-BenchmarkCanonicalJson $leftSignature) -eq (ConvertTo-BenchmarkCanonicalJson $rightSignature)) {
                    $same++
                }
            }
        }
        $lines.Add("| $left vs $right | $comparable | $same |")
    }
}
$lines.Add('')

if ($failures.Count -gt 0) {
    $lines.Add('## Failed measurements')
    $lines.Add('')
    foreach ($failure in $failures) {
        $lines.Add("- ``$($failure.Scenario)`` / ``$($failure.Algorithm)``: $($failure.Error)")
    }
    $lines.Add('')
}

$lines.Add('## Data and reproduction')
$lines.Add('')
$lines.Add('- [Scenario catalog](scenarios.json), [case summary](summary.csv), [run identity](run-info.json), and [live progress](progress.json).')
$lines.Add("- Each ``cases/<scenario>--<algorithm>`` directory retains the raw timing export, BenchmarkDotNet report, validated result, and $($runInfo.MemorySamplesPerCase) memory probes.")
if ($canResumeWithCurrentRunner) {
    $lines.Add("- Resume: ``./Benchmarks/run-full-depth.ps1 -Date $($runInfo.Date) -RunName $reportRunName``. Add ``-RetryFailed`` to retry failed cases.")
}
else {
    $lines.Add('- This historical matrix is report-only and cannot be resumed with the current runner.')
}
$lines.Add("- Regenerate this report: ``./Benchmarks/write-full-depth-report.ps1 -RunName $reportRunName``.")
$lines.Add('')

Write-BenchmarkText -Path (Join-Path $runRoot 'SUMMARY.md') -Text (($lines -join [Environment]::NewLine) + [Environment]::NewLine)
