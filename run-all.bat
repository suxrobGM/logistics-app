start "API Server" /d "." dotnet run --project ./src/Api/Logistics.WebApi && pause
start "Identity Server" /d "." dotnet run --project ./src/Apps/Logistics.IdentityServer && pause
start "Admin App" /d "." dotnet run --project ./src/Apps/Logistics.AdminApp && pause
exit