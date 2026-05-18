# Phase 7 cleanup — remove dead `using Logistics.Application.Commands/Queries/Events;` lines
# after all 6 modules have moved out of those flat namespaces.

$ErrorActionPreference = 'Stop'

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

$deadPatterns = @(
    '(?m)^using\s+Logistics\.Application\.Commands\s*;\r?\n',
    '(?m)^using\s+Logistics\.Application\.Queries\s*;\r?\n',
    '(?m)^using\s+Logistics\.Application\.Events\s*;\r?\n'
)

$count = 0
Get-ChildItem -Path 'src','test' -Recurse -Filter *.cs -File | ForEach-Object {
    $file = $_.FullName
    $f = Read-File $file
    $orig = $f.Text
    $text = $orig
    foreach ($p in $deadPatterns) {
        $text = [regex]::Replace($text, $p, '')
    }
    if ($text -ne $orig) {
        Write-File $file $text $f.HadBom
        $count++
    }
}
Write-Host ("Files cleaned: {0}" -f $count)
