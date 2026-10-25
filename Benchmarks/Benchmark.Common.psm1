#requires -Version 5.1

Set-StrictMode -Version Latest

function Get-BenchmarkUtc {
    [DateTime]::UtcNow.ToString('o')
}

function Write-BenchmarkText {
    param(
        [Parameter(Mandatory = $true)][String]$Path,
        [Parameter(Mandatory = $true)][AllowEmptyString()][String]$Text
    )

    $parent = Split-Path -Parent $Path
    if ($parent) {
        [IO.Directory]::CreateDirectory($parent) | Out-Null
    }
    [IO.File]::WriteAllText($Path, $Text, [Text.UTF8Encoding]::new($false))
}

function Read-BenchmarkJson {
    param([Parameter(Mandatory = $true)][String]$Path)

    Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
}

function Write-BenchmarkJson {
    param(
        [Parameter(Mandatory = $true)][String]$Path,
        [Parameter(Mandatory = $true)][AllowNull()]$Value
    )

    $parent = Split-Path -Parent $Path
    if ($parent) {
        [IO.Directory]::CreateDirectory($parent) | Out-Null
    }
    $temporary = $Path + '.tmp'
    $json = $Value | ConvertTo-Json -Depth 100
    Write-BenchmarkText -Path $temporary -Text ($json + [Environment]::NewLine)
    Move-Item -LiteralPath $temporary -Destination $Path -Force
}

function ConvertTo-BenchmarkOrderedValue {
    param([AllowNull()]$Value)

    if ($null -eq $Value) {
        return $null
    }
    if ($Value -is [Collections.IDictionary]) {
        $ordered = [Ordered]@{}
        foreach ($key in @($Value.Keys | Sort-Object)) {
            $ordered[[String]$key] = ConvertTo-BenchmarkOrderedValue $Value[$key]
        }
        return $ordered
    }
    if ($Value -is [Management.Automation.PSCustomObject]) {
        $ordered = [Ordered]@{}
        foreach ($property in @($Value.PSObject.Properties | Sort-Object Name)) {
            $ordered[$property.Name] = ConvertTo-BenchmarkOrderedValue $property.Value
        }
        return $ordered
    }
    if ($Value -is [Collections.IEnumerable] -and $Value -isnot [String]) {
        return @($Value | ForEach-Object { ConvertTo-BenchmarkOrderedValue $_ })
    }
    return $Value
}

function ConvertTo-BenchmarkCanonicalJson {
    param([Parameter(Mandatory = $true)][AllowNull()]$Value)

    ConvertTo-BenchmarkOrderedValue $Value | ConvertTo-Json -Depth 100 -Compress
}

function Get-BenchmarkFileSha256 {
    param([Parameter(Mandatory = $true)][String]$Path)

    (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Get-BenchmarkTextSha256 {
    param([Parameter(Mandatory = $true)][String]$Text)

    $sha = [Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [Text.Encoding]::UTF8.GetBytes($Text)
        ([BitConverter]::ToString($sha.ComputeHash($bytes))).Replace('-', '').ToLowerInvariant()
    }
    finally {
        $sha.Dispose()
    }
}

function Get-BenchmarkMean {
    param([Parameter(Mandatory = $true)][Object[]]$Values)

    $numbers = @($Values | Where-Object { $null -ne $_ } | ForEach-Object { [Double]$_ })
    if ($numbers.Count -eq 0) {
        return $null
    }
    return [Double](($numbers | Measure-Object -Average).Average)
}

function Get-BenchmarkMedian {
    param([Parameter(Mandatory = $true)][Object[]]$Values)

    $numbers = @($Values | Where-Object { $null -ne $_ } | ForEach-Object { [Double]$_ } | Sort-Object)
    if ($numbers.Count -eq 0) {
        return $null
    }
    $middle = [Math]::Floor($numbers.Count / 2)
    if ($numbers.Count % 2 -eq 1) {
        return [Double]$numbers[$middle]
    }
    return [Double](($numbers[$middle - 1] + $numbers[$middle]) / 2.0)
}

Export-ModuleMember -Function @(
    'Get-BenchmarkUtc',
    'Write-BenchmarkText',
    'Read-BenchmarkJson',
    'Write-BenchmarkJson',
    'ConvertTo-BenchmarkCanonicalJson',
    'Get-BenchmarkFileSha256',
    'Get-BenchmarkTextSha256',
    'Get-BenchmarkMean',
    'Get-BenchmarkMedian'
)
