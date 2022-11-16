FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Server/Logistics.API/*.csproj", "Server/Logistics.API/"]
COPY ["src/Core/Logistics.Application.Main/*.csproj", "Core/Logistics.Application.Main/"]
COPY ["src/Core/Logistics.Application.Main.Contracts/*.csproj", "Core/Logistics.Application.Main.Contracts/"]
COPY ["src/Core/Logistics.Application.Shared/*.csproj", "Core/Logistics.Application.Shared/"]
COPY ["src/Core/Logistics.Application.Tenant/*.csproj", "Core/Logistics.Application.Tenant/"]
COPY ["src/Core/Logistics.Application.Tenant.Contracts/*.csproj", "Core/Logistics.Application.Tenant.Contracts/"]
COPY ["src/Core/Logistics.Domain/*.csproj", "Core/Logistics.Domain/"]
COPY ["src/Core/Logistics.Domain.Shared/*.csproj", "Core/Logistics.Domain.Shared/"]
COPY ["src/Core/Logistics.EntityFramework/*.csproj", "Core/Logistics.EntityFramework/"]
RUN dotnet restore "Server/Logistics.API/Logistics.API.csproj"
WORKDIR /
COPY . ./
WORKDIR "/src/Server/Logistics.API"
RUN dotnet build "Logistics.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Logistics.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Logistics.API.dll"]
