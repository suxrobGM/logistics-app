@echo off
title Create Migrations
cd ../src/Core/Logistics.Infrastructure.EF

call :CreateMigration "master database" "MasterDbContext" "Master"
call :CreateMigration "tenant database" "TenantDbContext" "Tenant"

pause
goto :eof

:CreateMigration
set "DbType=%~1"
set "DbContext=%~2"
set "MigrationFolder=%~3"

set /p CreateMigration="Do you want to create migration for the %DbType% (y/n): "
if /I "%CreateMigration%" neq "y" goto :eof

:prompt
set "MigrationName="
set /p MigrationName="Enter migration name for the %DbType%: "
if "%MigrationName%" == "" (
    echo Error: Migration name cannot be empty.
    goto prompt
)

echo Running migration for the %DbType%...
dotnet ef migrations add %MigrationName% -c %DbContext% -o Migrations/%MigrationFolder%
echo Migrations completed.
goto :eof
