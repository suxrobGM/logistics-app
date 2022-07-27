start "API Server" /d "." dotnet run --project ./src/Api/Logistics.WebApi
start "Identity Server" /d "." dotnet run --project ./src/Apps/Logistics.IdentityServer
start "Admin App" /d "." dotnet run --project ./src/Apps/Logistics.AdminApp
exit