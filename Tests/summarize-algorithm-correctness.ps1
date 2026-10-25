#requires -Version 5.1
<#
.SYNOPSIS
Summarizes MSTest TRX results against the application's copying-tree reference.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [String]$Trx,

    [Parameter(Mandatory = $true)]
    [String]$Output,

    [Switch]$Partial,

    [Switch]$Stopped,

    [String]$RegressionTrx
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$referenceAlgorithm = 'PositionTreeWithCopying'
$algorithms = @('Original', 'MinMaxUnified', 'PositionTree', $referenceAlgorithm)
$comparisonFields = @('Move', 'Piece', 'ResultCode', 'InNMoves', 'EvaluationValue')
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$trxNamespace = 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'

function Add-ReportLines {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][AllowEmptyString()][String[]]$Text
    )

    foreach ($line in $Text) {
        $script:reportLines.Add($line)
    }
}

function ConvertTo-OrderedValue {
    param([AllowNull()]$Value)

    if ($null -eq $Value) {
        return $null
    }
    if ($Value -is [Collections.IDictionary]) {
        $ordered = [Ordered]@{}
        foreach ($key in @($Value.Keys | Sort-Object)) {
            $ordered[[String]$key] = ConvertTo-OrderedValue $Value[$key]
        }
        return $ordered
    }
    if ($Value -is [Management.Automation.PSCustomObject]) {
        $ordered = [Ordered]@{}
        foreach ($property in @($Value.PSObject.Properties | Sort-Object Name)) {
            $ordered[$property.Name] = ConvertTo-OrderedValue $property.Value
        }
        return $ordered
    }
    if ($Value -is [Collections.IEnumerable] -and $Value -isnot [String]) {
        return @($Value | ForEach-Object { ConvertTo-OrderedValue $_ })
    }
    return $Value
}

function ConvertTo-CanonicalJson {
    param([Parameter(Mandatory = $true)][AllowNull()]$Value)

    ConvertTo-OrderedValue $Value | ConvertTo-Json -Depth 100 -Compress
}

function Read-TrxResults {
    param([Parameter(Mandatory = $true)][String]$Path)

    [Xml]$document = [IO.File]::ReadAllText($Path)
    $namespaceManager = [Xml.XmlNamespaceManager]::new($document.NameTable)
    $namespaceManager.AddNamespace('t', $trxNamespace)
    $tests = @($document.SelectNodes('//t:UnitTestResult', $namespaceManager))
    $definitions = @{}
    foreach ($test in @($document.SelectNodes('//t:UnitTest', $namespaceManager))) {
        $method = $test.SelectSingleNode('t:TestMethod', $namespaceManager)
        $className = if ($null -eq $method) { 'Unknown' } else { ([String]$method.className).Split(',')[0] }
        $definitions[[String]$test.id] = $className
    }

    $cases = @{}
    foreach ($outputNode in @($document.SelectNodes('//t:StdOut', $namespaceManager))) {
        foreach ($line in ([String]$outputNode.InnerText -split "`r?`n")) {
            $trimmed = $line.Trim()
            if (-not $trimmed.StartsWith('ALGORITHM_RESULT_JSON ')) {
                continue
            }
            $case = $trimmed.Substring('ALGORITHM_RESULT_JSON '.Length) | ConvertFrom-Json
            $name = [String]$case.Scenario
            if ($cases.ContainsKey($name) -and
                (ConvertTo-CanonicalJson $cases[$name]) -ne (ConvertTo-CanonicalJson $case)) {
                throw "Conflicting snapshots for $name"
            }
            $cases[$name] = $case
        }
    }

    [PSCustomObject]@{
        Tests = $tests
        Definitions = $definitions
        Cases = $cases
    }
}

function Get-Differences {
    param(
        [Parameter(Mandatory = $true)]$Actual,
        [Parameter(Mandatory = $true)]$Expected
    )

    @($comparisonFields | Where-Object { $Actual.$_ -ne $Expected.$_ })
}

function Get-ClassName {
    param(
        [Parameter(Mandatory = $true)]$Test,
        [Parameter(Mandatory = $true)][Hashtable]$Definitions
    )

    $testId = [String]$Test.testId
    if ($Definitions.ContainsKey($testId)) {
        return [String]$Definitions[$testId]
    }
    return 'Unknown'
}

function Get-TestErrorMessage {
    param([Parameter(Mandatory = $true)]$Test)

    $namespaceManager = [Xml.XmlNamespaceManager]::new($Test.OwnerDocument.NameTable)
    $namespaceManager.AddNamespace('t', $trxNamespace)
    $messageNode = $Test.SelectSingleNode('t:Output/t:ErrorInfo/t:Message', $namespaceManager)
    if ($null -eq $messageNode -or [String]::IsNullOrWhiteSpace($messageNode.InnerText)) {
        return 'No message'
    }
    return @($messageNode.InnerText -split "`r?`n")[0]
}

function Get-RepositoryRelativePath {
    param([Parameter(Mandatory = $true)][String]$Path)

    $fullPath = [IO.Path]::GetFullPath($Path)
    $rootPrefix = $repositoryRoot.TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
    if (-not $fullPath.StartsWith($rootPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Path is outside the repository: $fullPath"
    }
    return $fullPath.Substring($rootPrefix.Length).Replace('\', '/')
}

function Get-WarsawTimestamp {
    $timeZone = $null
    foreach ($identifier in @('Central European Standard Time', 'Europe/Warsaw')) {
        try {
            $timeZone = [TimeZoneInfo]::FindSystemTimeZoneById($identifier)
            break
        }
        catch {
            # Try the identifier used by the other platform family.
        }
    }
    if ($null -eq $timeZone) {
        throw 'The Europe/Warsaw time zone is unavailable.'
    }
    [TimeZoneInfo]::ConvertTime([DateTimeOffset]::UtcNow, $timeZone).ToString('yyyy-MM-ddTHH:mm:sszzz')
}

function Get-ResolvedInputPath {
    param([Parameter(Mandatory = $true)][String]$Path)

    (Resolve-Path -LiteralPath $Path).Path
}

$trxPath = Get-ResolvedInputPath $Trx
$outputPath = if ([IO.Path]::IsPathRooted($Output)) {
    [IO.Path]::GetFullPath($Output)
}
else {
    [IO.Path]::GetFullPath((Join-Path (Get-Location).Path $Output))
}
$regressionPath = if ([String]::IsNullOrWhiteSpace($RegressionTrx)) { $null } else { Get-ResolvedInputPath $RegressionTrx }
$effectivePartial = $Partial -or $Stopped

$primary = Read-TrxResults $trxPath
$tests = @($primary.Tests)
$definitions = $primary.Definitions
$cases = $primary.Cases
if ($null -ne $regressionPath) {
    $regression = Read-TrxResults $regressionPath
    if ($regression.Cases.Count -gt 0) {
        throw 'Regression input also contains comparison snapshots; keep the runs separate to avoid double counting.'
    }
    $tests += @($regression.Tests)
    foreach ($entry in $regression.Definitions.GetEnumerator()) {
        $definitions[$entry.Key] = $entry.Value
    }
}

$counts = @{}
foreach ($test in $tests) {
    $outcome = [String]$test.outcome
    if (-not $counts.ContainsKey($outcome)) { $counts[$outcome] = 0 }
    $counts[$outcome]++
}
foreach ($standardOutcome in @('Passed', 'Failed', 'NotExecuted')) {
    if (-not $counts.ContainsKey($standardOutcome)) { $counts[$standardOutcome] = 0 }
}
$comparisonTests = @($tests | Where-Object { (Get-ClassName $_ $definitions).Contains('BenchmarkScenarioComparisonTests') })

$policies = @($cases.Values | ForEach-Object {
    if ($null -eq $_.PSObject.Properties['SearchPolicy']) { 'LegacyExtensions' } else { [String]$_.SearchPolicy }
} | Select-Object -Unique)
if ($policies.Count -gt 1) {
    throw 'Do not combine legacy and fixed-depth comparisons in one assessment.'
}
$policyNote = if ($policies -contains 'LegacyExtensions') {
    '**Historical results:** these snapshots used the original MinMax depth policy. Current correctness comparisons disable capture/check extensions and the extra two-ply allowance, and reject depths above 5 full moves / 10 plies. These counts have not been rerun under that policy.'
}
else {
    '**Fixed-depth policy:** capture/check extensions and the extra MinMax depth allowance are disabled. Comparison searches are limited to 5 full moves / 10 plies.'
}

$status = if ($effectivePartial) { 'COMPLETED focused comparison - partial depth coverage' } else { 'COMPLETED correctness run' }
if ($Stopped) {
    $status = "STOPPED at the user's request - partial comparison results"
}
$otherOutcomes = @($counts.Keys | Where-Object { $_ -notin @('Passed', 'Failed', 'NotExecuted') -and $counts[$_] -gt 0 } | Sort-Object)
$otherOutcomeText = if ($otherOutcomes.Count -eq 0) {
    'none'
}
else {
    '{' + (($otherOutcomes | ForEach-Object { "'$($_)': $($counts[$_])" }) -join ', ') + '}'
}

$lines = [Collections.Generic.List[String]]::new()
$script:reportLines = $lines
Add-ReportLines -Text @(
    '# Algorithm correctness assessment',
    '',
    "Updated: $(Get-WarsawTimestamp). **$status**.",
    '',
    $policyNote,
    '',
    'This assesses returned results, not speed. Per the user''s instruction, the application algorithm is the reference: **PositionTreeWithCopying**, called by `BreadthFirstTreeSearch`. Direct PositionTree is also compared. Production search policies are unchanged; comparison entry points use the policy stated above. Equality assertions have not been relaxed.',
    '',
    '## Test scope and outcomes',
    '',
    ('TRX: `{0}`. Recorded results: **{1}**, **{2} passed**, **{3} failed**, **{4} skipped**. Other outcomes: {5}.' -f (Get-RepositoryRelativePath $trxPath), $tests.Count, $counts.Passed, $counts.Failed, $counts.NotExecuted, $otherOutcomeText),
    '',
    ("**$($comparisonTests.Count) comparison tests executed; $($cases.Count)/53 comparison scenarios** have complete four-backend snapshots. All selected comparisons retain strict move/piece/result-code/distance/raw-score checks, legal-move assertions, input preservation and prior-history preservation."),
    ''
)
if ($effectivePartial) {
    if ($Stopped) {
        Add-ReportLines -Text @('These are the completed depth-1-3 comparison results. The unfinished full comparison was stopped at the user''s request. Its runner had not flushed a final TRX, so no additional cases from that interrupted run are counted. Deeper-case results remain unknown; no tests are still running.', '')
    }
    else {
        Add-ReportLines -Text @('This focused run covers depths 1-3 full moves only. Depths 4-5 were not run, and their results are not implied by this subset. No deeper comparison is running.', '')
    }
}
if ($null -ne $regressionPath) {
    Add-ReportLines -Text @("Regression TRX: ``$(Get-RepositoryRelativePath $regressionPath)``. Its results are combined with the isolated comparison run, without rerunning or double-counting the comparison class.", '')
}
Add-ReportLines -Text @(
    'The initial unrestricted whole-project attempt was stopped during an unrelated unpruned tree-construction stress run, before a final TRX was produced. A second broad attempt was stopped during long legacy smoke execution. The final regression selection excludes three unpruned construction tests: `PositionTreeTests.SimpleTests.StartingPosition_MaxDepth5`, `StartingPosition_MaxDepth6`, and `StartingPosition_MaxDepth8_NoLoop`; it also excludes `MinMaxTests.EndGameTests.FromLysia` and `MinMaxUnifiedTests.EndGameTests.FromLysia`, whose result assertions are entirely commented out. These five exclusions do not remove any of the 53 algorithm result comparisons. Existing `[Ignore]` tests remain ignored. No one should describe this as every test in the repository passing or completing.',
    '',
    '## Agreement with the app algorithm',
    '',
    '| Algorithm | Exact matches | Same move and piece | Same result code | Same terminal marker | Same distance | Same raw score |',
    '| --- | ---: | ---: | ---: | ---: | ---: | ---: |'
)

foreach ($algorithm in $algorithms[0..($algorithms.Count - 2)]) {
    $score = @{ Exact = 0; Move = 0; Result = 0; Terminal = 0; Distance = 0; Score = 0 }
    foreach ($case in $cases.Values) {
        $byAlgorithm = @{}
        foreach ($result in @($case.Results)) { $byAlgorithm[[String]$result.Algorithm] = $result }
        $actual = $byAlgorithm[$algorithm]
        $expected = $byAlgorithm[$referenceAlgorithm]
        $score.Exact += [Int32](@(Get-Differences $actual $expected).Count -eq 0)
        $score.Move += [Int32]($actual.Move -eq $expected.Move -and $actual.Piece -eq $expected.Piece)
        $score.Result += [Int32]($actual.ResultCode -eq $expected.ResultCode)
        $score.Terminal += [Int32](($actual.EvaluationValue -eq -32768) -eq ($expected.EvaluationValue -eq -32768))
        $score.Distance += [Int32]($actual.InNMoves -eq $expected.InNMoves)
        $score.Score += [Int32]($actual.EvaluationValue -eq $expected.EvaluationValue)
    }
    $values = @('Exact', 'Move', 'Result', 'Terminal', 'Distance', 'Score') | ForEach-Object { "$($score[$_])/$($cases.Count)" }
    $lines.Add("| $algorithm | $($values -join ' | ') |")
}

$parity = 0
$distanceOnly = 0
foreach ($case in $cases.Values) {
    $results = @($case.Results)
    $parity += [Int32](@(Get-Differences $results[0] $results[1]).Count -eq 0)
    $original = $results | Where-Object Algorithm -eq 'Original' | Select-Object -First 1
    $reference = $results | Where-Object Algorithm -eq $referenceAlgorithm | Select-Object -First 1
    $differences = @(Get-Differences $original $reference)
    $distanceOnly += [Int32]($differences.Count -eq 1 -and $differences[0] -eq 'InNMoves')
}
Add-ReportLines -Text @(
    '',
    "Original and Unified MinMax agree exactly in **$parity/$($cases.Count)** scenarios. Agreement between those two implementations alone does not establish agreement with the application.",
    '',
    "Original MinMax differs from the application tree **only in Distance in $distanceOnly/$($cases.Count) scenarios**. These remain strict equality failures, but should be distinguished from differences in move, score, terminal marker or result code.",
    '',
    'The terminal marker is the native `EvaluationNaN` raw value, **-32768**. `DrawOrUknown` is also the code for an ordinary nonterminal heuristic evaluation: equal result codes alone do not prove equal outcomes. Distance and raw terminal-marker mismatches remain failures; they are not normalized away. A different legal move has not been proven equally good merely because its result code matches.',
    '',
    '## Source observations to investigate next',
    '',
    'These are read-only findings, not fixes or proofs that one line explains every mismatch:',
    '',
    '1. **Legacy depth policy differs from fixed-depth comparisons.** The original production recursion does not decrement `depthToGo` and can terminate at the separate `StartingDepth + 2` limit. Fixed-depth comparison entry points instead stop at the actual requested ply count, disable extensions, and assert the deepest visited ply. Legacy depth handling is not an explanation for discrepancies in a fixed-depth snapshot.',
    '2. **Returned distance uses different conventions.** The nonterminal native evaluator returns `InNMoves = 0`; the tree rewrites propagated nonterminal distances and the application exposes the best child''s evaluation. Preserve terminal mate distance and terminal marker while separating this convention from genuine move/score differences.',
    '3. **Terminal handling and initial best values differ.** Plain MinMax returns its heuristic leaf evaluation before checking terminal moves there. Its initial best sentinels use real winner codes with distance zero and raw score zero, and the function returns the accumulated best value. The tree checks terminal conditions before returning a leaf and ends an expanded node with its sorted best child''s actual evaluation. Investigate a sentinel surviving the MinMax loop where a winner code arrives with distance zero and a nonterminal raw score; this is a source-level explanation to verify, not a fix already applied.',
    '4. **Repetition policies differ.** Plain MinMax contains a short fixed-offset stack-pattern check, while the tree checks repeated positions near the root. Rc5 comparisons carry the full prior game history; do not replace it with an empty stack to make tests agree.',
    '5. **Copying tree above depth 3 is a separate issue.** Its iterative copying/pruning branch differs from direct build. Use copying tree as the requested reference when a direct-tree result diverges; shallow agreement does not establish deeper agreement.',
    '',
    '## Per-scenario comparison',
    '',
    'Each detailed snapshot contains the actual result returned by that run. Full-move depth maps to twice as many input plies for MinMax. Passing legality/history checks do not make a differing score or move correct.',
    ''
)

$caseEntries = @($cases.GetEnumerator() | ForEach-Object {
    [PSCustomObject]@{ Name = [String]$_.Key; Case = $_.Value }
} | Sort-Object { [Int32]$_.Case.FullMoveDepth }, Name)
foreach ($entry in $caseEntries) {
    $case = $entry.Case
    $expected = @($case.Results | Where-Object Algorithm -eq $referenceAlgorithm)[0]
    Add-ReportLines -Text @(
        "### $($entry.Name)",
        '',
        "Depth: $($case.FullMoveDepth) full moves / $($case.Plies) input plies; prior positions: $($case.HistoryPositions).",
        '',
        "FEN: ``$($case.Fen)``",
        '',
        '| Algorithm | Move | Piece | Result | Terminal | Distance | Raw score | Differences from app tree |',
        '| --- | --- | --- | --- | --- | ---: | ---: | --- |'
    )
    foreach ($result in @($case.Results)) {
        $differences = @(Get-Differences $result $expected)
        $differenceText = if ($differences.Count -eq 0) { 'none' } else { $differences -join ', ' }
        $terminal = if ($result.EvaluationValue -eq -32768) { 'yes' } else { 'no' }
        $lines.Add("| $($result.Algorithm) | $($result.Move) | $($result.Piece) | $($result.Result) | $terminal | $($result.InNMoves) | $($result.EvaluationValue) | $differenceText |")
    }
    $lines.Add('')
}

Add-ReportLines -Text @('## Results by test class', '', '| Class | Passed | Failed | Skipped |', '| --- | ---: | ---: | ---: |')
$classes = @{}
foreach ($test in $tests) {
    $className = Get-ClassName $test $definitions
    if (-not $classes.ContainsKey($className)) {
        $classes[$className] = @{ Passed = 0; Failed = 0; NotExecuted = 0 }
    }
    $outcome = [String]$test.outcome
    if (-not $classes[$className].ContainsKey($outcome)) { $classes[$className][$outcome] = 0 }
    $classes[$className][$outcome]++
}
foreach ($className in @($classes.Keys | Sort-Object)) {
    $shortName = $className -replace '^Triglav\.Chess\.Engine\.Tests\.', ''
    $outcomes = $classes[$className]
    $lines.Add("| $shortName | $($outcomes.Passed) | $($outcomes.Failed) | $($outcomes.NotExecuted) |")
}

$nonComparison = @($tests | Where-Object {
    $_.outcome -eq 'Failed' -and -not (Get-ClassName $_ $definitions).Contains('BenchmarkScenarioComparisonTests')
})
if ($nonComparison.Count -gt 0) {
    Add-ReportLines -Text @('', '## Other failing tests', '')
    foreach ($test in $nonComparison) {
        $message = Get-TestErrorMessage $test
        if ($message.Length -gt 500) { $message = $message.Substring(0, 500) }
        $lines.Add("- ``$($test.testName)``: $message")
    }
}

$incomplete = @($tests | Where-Object {
    if ($_.outcome -ne 'Failed' -or -not (Get-ClassName $_ $definitions).Contains('BenchmarkScenarioComparisonTests')) {
        return $false
    }
    foreach ($caseName in $cases.Keys) {
        if (([String]$_.testName).Contains("($caseName)")) { return $false }
    }
    return $true
})
if ($incomplete.Count -gt 0) {
    Add-ReportLines -Text @(
        '',
        '## Comparison cases without complete four-backend snapshots',
        '',
        'These tests failed before all four result/legality/history checks completed. They are not counted as matches or silently omitted from the failure totals.',
        ''
    )
    foreach ($test in $incomplete) {
        $message = Get-TestErrorMessage $test
        if ($message.Length -gt 500) { $message = $message.Substring(0, 500) }
        $lines.Add("- ``$($test.testName)``: $message")
    }
}

$nativeDll = Join-Path $repositoryRoot 'Core/Triglav.Chess.Interop/Triglav.Chess.Native.dll'
$nativeHash = (Get-FileHash -LiteralPath $nativeDll -Algorithm SHA256).Hash.ToLowerInvariant()
Add-ReportLines -Text @(
    '',
    '## Reproduction',
    '',
    "Build and run the test project in **Release x64**. No benchmark runner is used. Current local native DLL SHA-256 (not proof of the DLL used by an older TRX): ``$nativeHash``.",
    '',
    'The comparison test emits `ALGORITHM_RESULT_JSON` snapshots even on success; the report generator deduplicates repeated class stdout in TRX. All 53 cases remain enabled by default and are ordered by depth to expose early discrepancies first.',
    '',
    '```powershell',
    'dotnet build Tests/Triglav.Chess.Engine.Tests/Triglav.Chess.Engine.Tests.csproj -c Release --no-restore',
    "dotnet run --project Tests/Triglav.Chess.Engine.Tests/Triglav.Chess.Engine.Tests.csproj -c Release --no-build -- --filter 'FullyQualifiedName~BenchmarkScenarioComparisonTests' --report-trx --report-trx-filename comparisons.trx --results-directory artifacts/algorithm-consistency --no-progress",
    './Tests/summarize-algorithm-correctness.ps1 -Trx artifacts/algorithm-consistency/comparisons.trx -Output docs/ALGORITHM_CORRECTNESS_RESULTS.md',
    '```',
    ''
)

$outputDirectory = Split-Path -Parent $outputPath
if ($outputDirectory) {
    [IO.Directory]::CreateDirectory($outputDirectory) | Out-Null
}
[IO.File]::WriteAllText($outputPath, ($lines -join [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
Write-Output "$($tests.Count) test results; $($cases.Count)/53 comparison snapshots; report: $outputPath"
