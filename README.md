# Logistics TMS: Automated Transport Management Solution

[![Build Status](https://github.com/suxrobgm/logistics-app/actions/workflows/dotnet-build.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/dotnet-build.yml)
[![Tests](https://github.com/suxrobgm/logistics-app/actions/workflows/dotnet-test.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/dotnet-test.yml)
[![Deployment](https://github.com/suxrobgm/logistics-app/actions/workflows/deploy-ftp.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/deploy-ftp.yml)
[![OfficeApp Build](https://github.com/suxrobgm/logistics-app/actions/workflows/officeapp-build.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/officeapp-build.yml)
[![DriverApp Build](https://github.com/suxrobgm/logistics-app/actions/workflows/driverapp-build.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/driverapp-build.yml)

Logistics TMS is an ultimate solution for all transport management needs. With a focus on automation, this Transportation Management System (TMS) is designed to streamline logistics, offering an efficient, optimized way to manage inbound and outbound transport operations.

## Overview

Logistics TMS primarily targets logistics and trucking companies seeking to streamline their operations. It offers a comprehensive suite that encompasses an administrator web application, a management web application, and a driver mobile application. The backend is powered by a robust REST API and an Identity Server application.

Operating on a multi-tenant architecture, Logistics TMS features a primary database for storing user credentials and tenant data, including company name, subdomain name, database connection string, and billing periods. Each tenant or company has a dedicated database.

## Getting Started

Follow these steps to get the project up and running:

1. Install SDKs 
   - [Download](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) and install the .NET 7 SDK. 
   - [Download](https://nodejs.org) and install the Node.js runtime.

2. Clone this repository: 
    ```
    $ git clone https://github.com/suxrobGM/logistics-app.git
    $ cd logistics-app
    ```

3. Install Angular app NPM packages:
   ```
   cd src\Client\Logistics.OfficeApp
   npm install
   ```

4. Update database connection strings: 
   Modify local or remote `MS SQL` database connection strings in the [Web API appsettings.json](./src/Api/Logistics.WebApi/appsettings.json) and the [IdentityServer appsettings.json](./src/Apps/Logistics.IdentityServer/appsettings.json) under the `ConnectionStrings:MainDatabase` section. Update tenant databases configuration in the [Web API appsettings.json](./src/Api/Logistics.WebApi/appsettings.json) under the `TenantsConfig` section.

5. Seed databases:
   To initialize and populate the databases, run the `seed-databases.bat` script provided in the repository.

6. Run applications:
   Launch all the applications in the suite using the respective `.bat` scripts in the repository.

7. Access the applications:
   Use the following local URLs to access the apps:
    - Web API: https://127.0.0.1:7000
    - Identity Server: https://127.0.0.1:7001
    - Admin app: https://127.0.0.1:7002
    - Office app: https://127.0.0.1:7003

## Demo application
The sample deployed application is available at http://office.jfleets.com

User email: Test1@gmail.com

User password: Test12345#

## Architectural Overview

### Technical Stack
- .NET 7
- ASP.NET Core
- Entity Framework Core
- Identity Server
- FluentValidator
- MediatR
- MS SQL
- xUnit
- Moq
- Angular 16
- PrimeNG
- Blazor
- MAUI
- Docker
- CI/CD

### Design Patterns
- Domain-Driven Design
- CQRS
- Domain Events
- Event Sourcing
- Unit Of Work
- Repository & Generic Repository
- Inversion of Control / Dependency injection
- Specification Pattern

For a deeper understanding of the project structure, refer to the architecture diagram:
![Project architecture diagram](./docs/project_architecture.jpg?raw=true)

## Sneak Peek into the Office App

Here are some screenshots of the Office Application for a better understanding:
![Office App](./docs/office_app_1.jpg?raw=true)
![Office App](./docs/office_app_2.jpg?raw=true)
![Office App](./docs/office_app_3.jpg?raw=true)
