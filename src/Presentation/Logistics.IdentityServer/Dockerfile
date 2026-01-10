FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /logistics

# Copy solution and project files
COPY *.sln ./
COPY src/Core/ src/Core/
COPY src/Shared/ src/Shared/
COPY src/Aspire/ src/Aspire/
COPY src/Presentation/Logistics.IdentityServer/ src/Presentation/Logistics.IdentityServer/

# Restore dependencies
RUN dotnet restore src/Presentation/Logistics.IdentityServer/Logistics.IdentityServer.csproj

# Build and publish
WORKDIR /logistics/src/Presentation/Logistics.IdentityServer
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 7001

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Logistics.IdentityServer.dll"]
