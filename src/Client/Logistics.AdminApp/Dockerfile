FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /logistics

# Copy solution and project files
COPY *.slnx ./
COPY src/Core/Logistics.Domain.Primitives/ src/Core/Logistics.Domain.Primitives/
COPY src/Shared/ src/Shared/
COPY src/Client/Logistics.AdminApp/ src/Client/Logistics.AdminApp/
COPY src/Client/Logistics.HttpClient/ src/Client/Logistics.HttpClient/

# Restore dependencies
RUN dotnet restore src/Client/Logistics.AdminApp/Logistics.AdminApp.csproj

# Build and publish
WORKDIR /logistics/src/Client/Logistics.AdminApp
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 7002

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Logistics.AdminApp.dll"]
