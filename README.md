# Logistics TMS: Automated Transport Management Solution

[![Build Status](https://github.com/suxrobgm/logistics-app/actions/workflows/dotnet-build.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/dotnet-build.yml)
[![Tests](https://github.com/suxrobgm/logistics-app/actions/workflows/dotnet-test.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/dotnet-test.yml)
[![Deployment](https://github.com/suxrobgm/logistics-app/actions/workflows/deploy-ftp.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/deploy-ftp.yml)
[![OfficeApp Build](https://github.com/suxrobgm/logistics-app/actions/workflows/officeapp-build.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/officeapp-build.yml)
[![DriverApp Build](https://github.com/suxrobgm/logistics-app/actions/workflows/driverapp-build.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/driverapp-build.yml)

[![CC BY-NC 4.0][cc-by-nc-shield]][cc-by-nc]

[cc-by-nc]: https://creativecommons.org/licenses/by-nc/4.0/
[cc-by-nc-image]: https://licensebuttons.net/l/by-nc/4.0/88x31.png
[cc-by-nc-shield]: https://img.shields.io/badge/License-CC%20BY--NC%204.0-lightgrey.svg

Logistics TMS is an ultimate solution for all transport management needs. With a focus on automation, this Transportation Management System (TMS) is designed to streamline logistics, offering an efficient, optimized way to manage inbound and outbound transport operations.

## Overview

Logistics TMS primarily targets logistics and trucking companies seeking to streamline their operations. It offers a comprehensive suite that encompasses an administrator web application, a management web application, and a driver mobile application. The backend is powered by a robust REST API and an Identity Server application.

Operating on a multi-tenant architecture, Logistics TMS features a primary database for storing user credentials and tenant data, including company name, subdomain name, database connection string, and billing periods. Each tenant or company has a dedicated database.

## Development status
The project is actively under development. I have completed 85% of the work required for the first version of the MVP.

## Getting Started

Follow these steps to get the project up and running:

1. Install SDKs 
   - [Download](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) and install the .NET 8 SDK. 
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
   Modify local or remote `MS SQL` database connection strings in the [Web API appsettings.json](./src/Server/Logistics.API/appsettings.json) and the [IdentityServer appsettings.json](./src/Server/Logistics.IdentityServer/appsettings.json) under the `ConnectionStrings:MainDatabase` section. Update tenant databases configuration in the [Web API appsettings.json](./src/Server/Logistics.API/appsettings.json) under the `TenantsConfig` section.

5. Seed databases:
   To initialize and populate the databases, run the `seed-databases.bat` script provided in the repository.

6. Run applications:
   Launch all the applications in the suite using the respective `.cmd` scripts in the repository.

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
- .NET 8
- ASP.NET Core
- Entity Framework Core
- Deunde Identity Server
- FluentValidator
- MediatR
- MS SQL
- xUnit
- Moq
- Angular 18
- PrimeNG
- Blazor
- MAUI
- Firebase
- SignalR
- Docker
- CI/CD

### Design Patterns
- Multi-Tenant Architecture
- Domain-Driven Design
- CQRS
- Domain Events
- Event Sourcing
- Unit Of Work
- Repository & Generic Repository
- Inversion of Control / Dependency injection
- Specification Pattern

## Copyright
This work is licensed under a
[Creative Commons Attribution-NonCommercial 4.0 International License][cc-by-nc].

[![CC BY-NC 4.0][cc-by-nc-image]][cc-by-nc]

For commercial use, please contact me at **suxrobgm@gmail.com** or Telegram **@suxrobgm**

## Architecture Diagram
For a deeper understanding of the project structure, refer to the architecture diagram:
![Project architecture diagram](./docs/project_architecture.jpg?raw=true)

## Office Web App Preview
Here is a sneak peek into the Office Application:

![Office App](./docs/office_app_1.jpg?raw=true)
![Office App](./docs/office_app_2.jpg?raw=true)
![Office App](./docs/office_app_3.jpg?raw=true)
![Office App](./docs/office_app_4.jpg?raw=true)
![Office App](./docs/office_app_5.jpg?raw=true)
![Office App](./docs/office_app_6.jpg?raw=true)
![Office App](./docs/office_app_7.jpg?raw=true)
![Office App](./docs/office_app_8.jpg?raw=true)
![Office App](./docs/office_app_9.jpg?raw=true)


## Driver Mobile App Preview
![Driver App](./docs/driver_app_1.jpg?raw=true)
![Driver App](./docs/driver_app_2.jpg?raw=true)
![Driver App](./docs/driver_app_3.jpg?raw=true)
![Driver App](./docs/driver_app_4.jpg?raw=true)
![Driver App](./docs/driver_app_5.jpg?raw=true)
![Driver App](./docs/driver_app_6.jpg?raw=true)
![Driver App](./docs/driver_app_7.jpg?raw=true)
