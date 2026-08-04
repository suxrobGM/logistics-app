#Requires -Version 5.1
<#
.SYNOPSIS
    Runs Logistics.DbMigrator against PRODUCTION, configured solely from deploy/.env.

.DESCRIPTION
    Loads deploy/.env into the process environment, forces the Production environment,
    shows the target DB host, and requires a typed confirmation before running.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$envFile = Join-Path $PSScriptRoot '.env'
$migratorProject = Join-Path $PSScriptRoot '..\src\Presentation\Logistics.DbMigrator'

if (-not (Test-Path -LiteralPath $envFile)) {
    Write-Host "No .env found at $envFile - copy .env.example and fill in the production values." -ForegroundColor Red
    exit 1
}

if (-not (Test-Path -LiteralPath $migratorProject)) {
    Write-Host "DbMigrator project not found at $migratorProject" -ForegroundColor Red
    exit 1
}

$loaded = 0
foreach ($line in (Get-Content -LiteralPath $envFile)) {
    $trimmed = $line.Trim()
    if ($trimmed.Length -eq 0 -or $trimmed.StartsWith('#')) { continue }

    $split = $trimmed.IndexOf('=')
    if ($split -lt 1) { continue }

    $name = $trimmed.Substring(0, $split).Trim()
    $value = $trimmed.Substring($split + 1).Trim()

    $value = $value -replace '^(["''])(.*)\1$', '$2'

    [Environment]::SetEnvironmentVariable($name, $value, 'Process')
    $loaded++
}

[Environment]::SetEnvironmentVariable('DOTNET_ENVIRONMENT', 'Production', 'Process')
[Environment]::SetEnvironmentVariable('ASPNETCORE_ENVIRONMENT', 'Production', 'Process')

$master = [Environment]::GetEnvironmentVariable('ConnectionStrings__MasterDatabase', 'Process')
if ([string]::IsNullOrWhiteSpace($master)) {
    Write-Host 'ConnectionStrings__MasterDatabase is not set in .env - refusing to run.' -ForegroundColor Red
    exit 1
}

# Display only the Host token - the rest of the string holds the password.
$dbHost = '<unparsed>'
$hostMatch = [regex]::Match($master, '(?i)(?:^|;)\s*Host\s*=\s*([^;]+)')
if ($hostMatch.Success) { $dbHost = $hostMatch.Groups[1].Value.Trim() }

Write-Host ''
Write-Host '################################################################' -ForegroundColor Red
Write-Host '#  WARNING - PRODUCTION DATABASE MIGRATION                      #' -ForegroundColor Red
Write-Host '################################################################' -ForegroundColor Red
Write-Host "About to run DbMigrator against PRODUCTION ($dbHost)" -ForegroundColor Red
Write-Host "Config source: $envFile ($loaded variables loaded)" -ForegroundColor Yellow
Write-Host 'This migrates and seeds the master and every configured tenant database.' -ForegroundColor Yellow
Write-Host ''

$confirmation = Read-Host "Type 'migrate-prod' to continue (anything else aborts)"
if ($confirmation -ne 'migrate-prod') {
    Write-Host 'Aborted - nothing was run.' -ForegroundColor Yellow
    exit 1
}

Write-Host ''
Write-Host "Running DbMigrator against $dbHost ..." -ForegroundColor Cyan

& dotnet run --project $migratorProject -- --exit
$exitCode = $LASTEXITCODE

if ($exitCode -eq 0) {
    Write-Host 'DbMigrator finished successfully.' -ForegroundColor Green
}
else {
    Write-Host "DbMigrator exited with code $exitCode." -ForegroundColor Red
}

exit $exitCode
