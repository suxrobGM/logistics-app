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
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage - use nginx to serve static Blazor WASM files
FROM nginx:alpine
WORKDIR /usr/share/nginx/html

# Remove default nginx static assets
RUN rm -rf ./*

# Copy published Blazor WASM files
COPY --from=build /app/publish/wwwroot .

# Copy nginx configuration
COPY src/Client/Logistics.AdminApp/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 7002

CMD ["nginx", "-g", "daemon off;"]
