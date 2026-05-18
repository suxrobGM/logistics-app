# Phase 7 helper — move one bounded-context module's files into Modules/{Module}/{Feature}/...
# and rewrite the namespace declaration in each moved file. Also adds new `using` lines to
# consumers that reference the moved types. Old `using Logistics.Application.Commands` lines
# are left in place until the final module empties those namespaces.

param(
    [Parameter(Mandatory=$true)] [string] $Module,
    # Hashtable: SourceEntityName -> TargetFeatureName (pluralized)
    [Parameter(Mandatory=$true)] [hashtable] $Features
)

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

# ---- Step 1: git mv each feature's Commands/Queries/Events folders -------------------------
foreach ($pair in $Features.GetEnumerator()) {
    $src = $pair.Key
    $dst = $pair.Value
    foreach ($kind in 'Commands', 'Queries', 'Events') {
        $from = Join-Path $appRoot "$kind/$src"
        $to   = Join-Path $appRoot "Modules/$Module/$dst/$kind"
        if (Test-Path $from) {
            $parent = Split-Path $to
            if (-not (Test-Path $parent)) { New-Item -ItemType Directory -Force -Path $parent | Out-Null }
            git mv $from $to
            if ($LASTEXITCODE -ne 0) { throw "git mv failed: $from -> $to" }
            Write-Host "Moved: $from -> $to"
        }
    }
}

# ---- Step 2: rewrite namespace declarations in moved files --------------------------------
$moduleRoot = Join-Path $appRoot "Modules/$Module"
Get-ChildItem $moduleRoot -Directory | ForEach-Object {
    $feature = $_.Name
    foreach ($kind in 'Commands','Queries','Events') {
        $dir = Join-Path $_.FullName $kind
        if (-not (Test-Path $dir)) { continue }
        $oldNs = "Logistics.Application.$kind"
        $newNs = "Logistics.Application.Modules.$Module.$feature.$kind"
        Get-ChildItem $dir -Recurse -Filter *.cs | ForEach-Object {
            $f = Read-File $_.FullName
            $pattern = "(?m)^namespace\s+" + [regex]::Escape($oldNs) + "\s*;"
            $newText = [regex]::Replace($f.Text, $pattern, "namespace $newNs;")
            if ($newText -ne $f.Text) {
                Write-File $_.FullName $newText $f.HadBom
            }
        }
    }
}
Write-Host "Namespaces rewritten under $moduleRoot"

# ---- Step 3: build type -> new-namespace map for this module's moved types ---------------
$typeMap = @{}
Get-ChildItem $moduleRoot -Recurse -Filter *.cs | ForEach-Object {
    $text = [System.IO.File]::ReadAllText($_.FullName)
    $nsMatch = [regex]::Match($text, '(?m)^namespace\s+([\w\.]+)\s*;')
    if (-not $nsMatch.Success) { return }
    $ns = $nsMatch.Groups[1].Value
    # Only include namespaces under this module (skip the registrar etc.)
    if ($ns -notlike "Logistics.Application.Modules.$Module.*") { return }
    foreach ($m in [regex]::Matches($text, '(?m)^\s*(?:public|internal)\s+(?:sealed\s+|abstract\s+|static\s+|partial\s+|readonly\s+|record\s+)*(?:class|record|struct|interface)\s+(\w+)')) {
        $tn = $m.Groups[1].Value
        if ($typeMap.ContainsKey($tn) -and $typeMap[$tn] -ne $ns) {
            Write-Warning "Type name collision: $tn in both $($typeMap[$tn]) and $ns"
        }
        $typeMap[$tn] = $ns
    }
}
Write-Host ("Built type map: {0} types from module {1}" -f $typeMap.Count, $Module)

# ---- Step 4: add new usings to consumer files that reference these types -----------------
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
# Projects that do NOT reference Logistics.Application — never add Application usings here.
# (src/Shared/*, src/Client/*, src/Core/Logistics.Domain/*, etc. — excluded by allowlist above.)
$oldNamespaces = @('Logistics.Application.Commands','Logistics.Application.Queries','Logistics.Application.Events')

$consumerCount = 0
foreach ($root in $consumerRoots) {
    if (-not (Test-Path $root)) { continue }
    Get-ChildItem $root -Recurse -Filter *.cs -File | ForEach-Object {
        $file = $_.FullName
        $f = Read-File $file
        $text = $f.Text

        # The file's own namespace - don't add a redundant using for it
        $ownNsMatch = [regex]::Match($text, '(?m)^namespace\s+([\w\.]+)\s*;')
        $ownNs = if ($ownNsMatch.Success) { $ownNsMatch.Groups[1].Value } else { $null }

        # We only operate on files that still have one of the old umbrella usings,
        # OR files inside the module that have cross-feature refs (they may not have an old using either).
        $hasOld = $false
        foreach ($ns in $oldNamespaces) {
            if ($text -match ("(?m)^using\s+" + [regex]::Escape($ns) + "\s*;")) { $hasOld = $true; break }
        }
        $inThisModule = $file -like "*\Modules\$Module\*"
        if (-not $hasOld -and -not $inThisModule) { return }

        # Find which of THIS module's types are referenced in this file
        $needed = @{}
        foreach ($t in $typeMap.Keys) {
            if ([regex]::IsMatch($text, ("\b" + [regex]::Escape($t) + "\b"))) {
                $needed[$typeMap[$t]] = $true
            }
        }
        if ($needed.Count -eq 0) { return }

        # Determine which new usings are missing
        $toAdd = @()
        foreach ($ns in ($needed.Keys | Sort-Object)) {
            if ($ns -eq $ownNs) { continue }
            $pattern = "(?m)^using\s+" + [regex]::Escape($ns) + "\s*;"
            if (-not [regex]::IsMatch($text, $pattern)) {
                $toAdd += "using $ns;"
            }
        }
        if ($toAdd.Count -eq 0) { return }

        # Insert the new usings after the last existing `using` line
        $newBlock = ($toAdd -join "`r`n")
        $usingBlockPattern = '(?ms)^((?:using [^\r\n]+;\r?\n)+)'
        if ($text -match $usingBlockPattern) {
            $text = [regex]::Replace($text, $usingBlockPattern, ('$1' + $newBlock + "`r`n"), 1)
        } else {
            $text = $newBlock + "`r`n" + $text
        }

        Write-File $file $text $f.HadBom
        $consumerCount++
    }
}
Write-Host ("Consumers updated: {0}" -f $consumerCount)
