FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 7000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Api/Logistics.WebApi/*.csproj", "Api/Logistics.WebApi/"]
COPY ["src/Core/Logistics.Application.Main/*.csproj", "Core/Logistics.Application.Main/"]
COPY ["src/Core/Logistics.Application.Main.Contracts/*.csproj", "Core/Logistics.Application.Main.Contracts/"]
COPY ["src/Core/Logistics.Application.Shared/*.csproj", "Core/Logistics.Application.Shared/"]
COPY ["src/Core/Logistics.Application.Tenant/*.csproj", "Core/Logistics.Application.Tenant/"]
COPY ["src/Core/Logistics.Application.Tenant.Contracts/*.csproj", "Core/Logistics.Application.Tenant.Contracts/"]
COPY ["src/Core/Logistics.Domain/*.csproj", "Core/Logistics.Domain/"]
COPY ["src/Core/Logistics.Domain.Shared/*.csproj", "Core/Logistics.Domain.Shared/"]
COPY ["src/Core/Logistics.EntityFramework/*.csproj", "Core/Logistics.EntityFramework/"]
RUN dotnet restore "Api/Logistics.WebApi/Logistics.WebApi.csproj"
COPY . ./
WORKDIR "/src/Api/Logistics.WebApi"
RUN dotnet build "Logistics.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Logistics.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Logistics.WebApi.dll"]
