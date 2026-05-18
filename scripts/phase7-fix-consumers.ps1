# Phase 7 helper — re-scans all consumers (including files whose own namespace is one of the
# old umbrella namespaces) and adds new specific `using` lines for moved types.
# Idempotent: if the required `using` already exists, skip.

$ErrorActionPreference = 'Stop'
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$appRoot = "src/Core/Logistics.Application"

function Read-File($file) {
    $bytes = [System.IO.File]::ReadAllBytes($file)
    $hadBom = ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
    $text = [System.IO.File]::ReadAllText($file)
    if ($text.Length -gt 0 -and $text[0] -eq [char]0xFEFF) { $text = $text.Substring(1) }
    return @{ Text = $text; HadBom = $hadBom }
}

function Write-File($file, $text, $hadBom) {
    $enc = if ($hadBom) { New-Object System.Text.UTF8Encoding($true) } else { (New-Object System.Text.UTF8Encoding($false)) }
    [System.IO.File]::WriteAllText($file, $text, $enc)
}

# Build a global type->namespace map for everything currently under Modules/
$typeMap = @{}
Get-ChildItem (Join-Path $appRoot "Modules") -Recurse -Filter *.cs | ForEach-Object {
    $text = [System.IO.File]::ReadAllText($_.FullName)
    $nsMatch = [regex]::Match($text, '(?m)^namespace\s+([\w\.]+)\s*;')
    if (-not $nsMatch.Success) { return }
    $ns = $nsMatch.Groups[1].Value
    if ($ns -notlike "Logistics.Application.Modules.*") { return }
    # Skip registrar files (top-level module namespace, not a feature)
    if ($ns -match '^Logistics\.Application\.Modules\.[^.]+$') { return }
    foreach ($m in [regex]::Matches($text, '(?m)^\s*(?:public|internal)\s+(?:sealed\s+|abstract\s+|static\s+|partial\s+|readonly\s+|record\s+)*(?:class|record|struct|interface)\s+(\w+)')) {
        $tn = $m.Groups[1].Value
        if ($typeMap.ContainsKey($tn) -and $typeMap[$tn] -ne $ns) {
            Write-Warning "Type collision: $tn in both $($typeMap[$tn]) and $ns"
        }
        $typeMap[$tn] = $ns
    }
}
Write-Host ("Type map: {0} entries" -f $typeMap.Count)

$oldNamespaces = @('Logistics.Application.Commands','Logistics.Application.Queries','Logistics.Application.Events')

# Allowlist: only projects that reference Logistics.Application
$consumerRoots = @(
    'src/Presentation',
    'src/Infrastructure/Logistics.Infrastructure.AI',
    'src/Core/Logistics.Application/Services',
    'src/Core/Logistics.Application/Commands',
    'src/Core/Logistics.Application/Queries',
    'src/Core/Logistics.Application/Events',
    'src/Core/Logistics.Application/Behaviours',
    'src/Core/Logistics.Application/Validators',
    'src/Core/Logistics.Application/Specifications',
    'src/Core/Logistics.Application/Utilities',
    'src/Core/Logistics.Application/Modules',
    'test'
)

$count = 0
$existingRoots = $consumerRoots | Where-Object { Test-Path $_ }
Get-ChildItem -Path $existingRoots -Recurse -Filter *.cs -File | ForEach-Object {
    $file = $_.FullName
    $f = Read-File $file
    $text = $f.Text

    $ownNsMatch = [regex]::Match($text, '(?m)^namespace\s+([\w\.]+)\s*;')
    $ownNs = if ($ownNsMatch.Success) { $ownNsMatch.Groups[1].Value } else { $null }

    # Skip files inside Modules/ — they have correct namespaces and the cross-feature
    # case is handled because they have explicit usings if they need other modules' types
    # (legacy code did `using Logistics.Application.Commands;` and these still work for
    # not-yet-moved types). After they move, this script picks them up via $ownNs check.

    # Find which moved types this file references
    $needed = @{}
    foreach ($t in $typeMap.Keys) {
        if ([regex]::IsMatch($text, ("\b" + [regex]::Escape($t) + "\b"))) {
            $needed[$typeMap[$t]] = $true
        }
    }
    if ($needed.Count -eq 0) { return }

    # Build list of missing usings (skip file's own namespace)
    $toAdd = @()
    foreach ($ns in ($needed.Keys | Sort-Object)) {
        if ($ns -eq $ownNs) { continue }
        $pattern = "(?m)^using\s+" + [regex]::Escape($ns) + "\s*;"
        if (-not [regex]::IsMatch($text, $pattern)) {
            $toAdd += "using $ns;"
        }
    }
    if ($toAdd.Count -eq 0) { return }

    $newBlock = ($toAdd -join "`r`n")
    $usingBlockPattern = '(?ms)^((?:using [^\r\n]+;\r?\n)+)'
    if ($text -match $usingBlockPattern) {
        $text = [regex]::Replace($text, $usingBlockPattern, ('$1' + $newBlock + "`r`n"), 1)
    } else {
        $text = $newBlock + "`r`n" + $text
    }

    Write-File $file $text $f.HadBom
    $count++
}

# Second pass: rewrite fully-qualified type references like
#   Logistics.Application.Commands.CreateEmployeeCommand  ->  Logistics.Application.Modules.IdentityAccess.Employees.Commands.CreateEmployeeCommand
# This catches `using Alias = Logistics.Application.Commands.X;` and any inline FQN usage.
$fqCount = 0
Get-ChildItem -Path $existingRoots -Recurse -Filter *.cs -File | ForEach-Object {
    $file = $_.FullName
    $f = Read-File $file
    $text = $f.Text
    $orig = $text
    foreach ($t in $typeMap.Keys) {
        $newNs = $typeMap[$t]
        foreach ($oldNs in @('Logistics.Application.Commands','Logistics.Application.Queries','Logistics.Application.Events')) {
            $pattern = "\b" + [regex]::Escape("$oldNs.$t") + "\b"
            $replacement = "$newNs.$t"
            $text = [regex]::Replace($text, $pattern, $replacement)
        }
    }
    if ($text -ne $orig) {
        Write-File $file $text $f.HadBom
        $fqCount++
    }
}
Write-Host ("Fully-qualified refs rewritten in {0} files" -f $fqCount)
Write-Host ("Consumers updated: {0}" -f $count)
