<#
.SYNOPSIS
    Auto-formatter hook for Claude Code
.DESCRIPTION
    Formats files after Edit/Write operations based on file extension.
    - TypeScript, HTML, SCSS, JSON → Prettier
    - C# → dotnet format
#>

# Read JSON input from stdin
$input = [Console]::In.ReadToEnd()

try {
    $data = $input | ConvertFrom-Json
    $filePath = $data.tool_input.file_path

    if (-not $filePath -or -not (Test-Path $filePath)) {
        exit 0
    }

    $extension = [System.IO.Path]::GetExtension($filePath).ToLower()
    $angularDir = "src\Client\Logistics.Angular"

    switch ($extension) {
        # Frontend files - use Prettier
        { $_ -in @('.ts', '.html', '.scss', '.css', '.json') } {
            # Only format if file is in Angular project
            if ($filePath -like "*$angularDir*") {
                $angularRoot = $filePath.Substring(0, $filePath.IndexOf($angularDir) + $angularDir.Length)
                Push-Location $angularRoot
                try {
                    & bun run prettier --write $filePath 2>$null
                    if ($LASTEXITCODE -eq 0) {
                        Write-Host "Formatted: $filePath"
                    }
                } finally {
                    Pop-Location
                }
            }
        }

        # C# files - use dotnet format
        '.cs' {
            # Find the nearest .csproj file
            $dir = [System.IO.Path]::GetDirectoryName($filePath)
            $projectFile = $null

            while ($dir -and -not $projectFile) {
                $csproj = Get-ChildItem -Path $dir -Filter "*.csproj" -ErrorAction SilentlyContinue | Select-Object -First 1
                if ($csproj) {
                    $projectFile = $csproj.FullName
                } else {
                    $parent = Split-Path $dir -Parent
                    if ($parent -eq $dir) { break }
                    $dir = $parent
                }
            }

            if ($projectFile) {
                & dotnet format $projectFile --include $filePath --verbosity quiet 2>$null
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "Formatted: $filePath"
                }
            }
        }
    }
} catch {
    # Silently ignore errors - formatting is best-effort
    exit 0
}

exit 0
