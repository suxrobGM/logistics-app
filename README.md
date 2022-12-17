[![BuildShield](https://github.com/suxrobgm/logistics-app/actions/workflows/dotnet-build.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/dotnet-build.yml)
[![BuildShield](https://github.com/suxrobgm/logistics-app/actions/workflows/dotnet-test.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/dotnet-test.yml)
[![BuildShield](https://github.com/suxrobgm/logistics-app/actions/workflows/deploy-ftp.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/deploy-ftp.yml)
[![BuildShield](https://github.com/suxrobgm/logistics-app/actions/workflows/officeapp-build.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/officeapp-build.yml)
[![BuildShield](https://github.com/suxrobgm/logistics-app/actions/workflows/driverapp-build.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/driverapp-build.yml)

# Logistics TMS
Automate your entire transportation logistics operations with the transportation management system (TMS).

## How to run?
### 1. [Download](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) and install the .NET 7 SDK. 
### 2. Clone this repository:
```
$ git clone https://github.com/suxrobGM/logistics-app.git
$ cd logistics-app
```
### 3. Update database connection strings:

You can use a local or remote `MySQL` database.
Update database connection strings in the [Web API appsettings.json](./src/Api/Logistics.WebApi/appsettings.json) and the [IdentityServer appsettings.json](./src/Apps/Logistics.IdentityServer/appsettings.json) under the `ConnectionStrings:MainDatabase` section.

Change tenants' databases configuration in the [Web API appsettings.json](./src/Api/Logistics.WebApi/appsettings.json) under the `TenantsConfig` section. Specify the database host address, root username, and passwords.


### 4. Seed databases 

First, you need to update the `DbMigrator` project configurations in the [appsettings.json](./src/Core/Logistics.DbMigrator/appsettings.json). Modify the `ConnectionStrings` and `TenantsConfig` sections. For testing purposes, you can populate databases with test data, so change the `PopulateTestData` to `true`.

Then run the following script to initialize and populate databases.
```
$ ./scripts/seed-databases.bat
```

### 5. Run apps:
Run the following scripts to fully launch the project's applications.
```
$ ./scripts/run-identity.bat
$ ./scripts/run-api.bat
$ ./scripts/run-adminapp.bat
$ ./scripts/run-officeapp.bat
```

Project local URLs:
- Web API: https://127.0.0.1:7000
- Identity Server: https://127.0.0.1:7001
- Admin app: https://127.0.0.1:7002
- Office app: https://127.0.0.1:7003

## Project architecture
![Project architecture diagram](./docs/project_architecture.jpg?raw=true)

## Demo Office App
![Office App](./docs/office_app_1.jpg?raw=true)
![Office App](./docs/office_app_2.jpg?raw=true)
![Office App](./docs/office_app_3.jpg?raw=true)
![Office App](./docs/office_app_4.jpg?raw=true)
![Office App](./docs/office_app_5.jpg?raw=true)