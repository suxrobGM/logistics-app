FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Api/Logistics.WebApi.Client/*.csproj", "Api/Logistics.WebApi.Client/"]
COPY ["src/Apps/Logistics.AdminApp/*.csproj", "Apps/Logistics.AdminApp/"]
COPY ["src/Apps/Logistics.Blazor/*.csproj", "Apps/Logistics.Blazor/"]
COPY ["src/Core/Logistics.Application.Main/*.csproj", "Core/Logistics.Application.Main/"]
COPY ["src/Core/Logistics.Application.Main.Contracts/*.csproj", "Core/Logistics.Application.Main.Contracts/"]
COPY ["src/Core/Logistics.Application.Shared/*.csproj", "Core/Logistics.Application.Shared/"]
COPY ["src/Core/Logistics.Application.Tenant/*.csproj", "Core/Logistics.Application.Tenant/"]
COPY ["src/Core/Logistics.Application.Tenant.Contracts/*.csproj", "Core/Logistics.Application.Tenant.Contracts/"]
COPY ["src/Core/Logistics.Domain/*.csproj", "Core/Logistics.Domain/"]
COPY ["src/Core/Logistics.Domain.Shared/*.csproj", "Core/Logistics.Domain.Shared/"]
COPY ["src/Core/Logistics.EntityFramework/*.csproj", "Core/Logistics.EntityFramework/"]
RUN dotnet restore "Apps/Logistics.AdminApp/Logistics.AdminApp.csproj"
WORKDIR /
COPY . ./
WORKDIR "/src/Apps/Logistics.AdminApp"
RUN dotnet build "Logistics.AdminApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Logistics.AdminApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Logistics.AdminApp.dll"]
