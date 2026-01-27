FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /logistics

# Copy solution and project files
COPY *.slnx ./
COPY src/Core/ src/Core/
COPY src/Shared/ src/Shared/
COPY src/Infrastructure/ src/Infrastructure/
COPY src/Aspire/ src/Aspire/
COPY src/Presentation/Logistics.DbMigrator/ src/Presentation/Logistics.DbMigrator/

# Restore dependencies
RUN dotnet restore src/Presentation/Logistics.DbMigrator/Logistics.DbMigrator.csproj

# Build and publish
WORKDIR /logistics/src/Presentation/Logistics.DbMigrator
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Logistics.DbMigrator.dll"]
