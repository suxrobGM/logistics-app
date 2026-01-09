FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /logistics

# Copy solution and project files
COPY *.sln ./
COPY src/Core/ src/Core/
COPY src/Shared/ src/Shared/
COPY src/Presentation/Logistics.API/ src/Presentation/Logistics.API/

# Restore dependencies
RUN dotnet restore src/Presentation/Logistics.API/Logistics.API.csproj

# Build and publish
WORKDIR /logistics/src/Presentation/Logistics.API
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 7000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Logistics.API.dll"]
