# Logistics TMS: Automated Transport Management Solution

[![Build Status](https://github.com/suxrobgm/logistics-app/actions/workflows/build.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/build.yml)
[![Deployment](https://github.com/suxrobgm/logistics-app/actions/workflows/deploy-ssh.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/deploy-ssh.yml)

[![CC BY-NC 4.0][cc-by-nc-shield]][cc-by-nc]

[cc-by-nc]: https://creativecommons.org/licenses/by-nc/4.0/
[cc-by-nc-image]: https://licensebuttons.net/l/by-nc/4.0/88x31.png
[cc-by-nc-shield]: https://img.shields.io/badge/License-CC%20BY--NC%204.0-lightgrey.svg

Logistics TMS is an ultimate solution for all transport management needs. With a focus on automation, this Transportation Management System (TMS) is designed to streamline logistics, offering an efficient, optimized way to manage inbound and outbound transport operations.

## Overview

Logistics TMS primarily targets logistics and trucking companies seeking to streamline their operations. It offers a comprehensive suite that encompasses an administrator web application, a management web application, and a driver mobile application. The backend is powered by a robust REST API and an Identity Server application.

Operating on a multi-tenant architecture, Logistics TMS features a primary database for storing user credentials and tenant data, including company name, subdomain name, database connection string, and billing periods. Each tenant or company has a dedicated database.

## Demo

A live demo of the application is available at [https://logistics-office.suxrobgm.net](https://logistics-office.suxrobgm.net). Use one of the following test credentials to log in to the application:

- Owner user: email: `Test1@gmail.com`, password: `Test12345#`
- Manager user: email: `Test2@gmail.com`, password: `Test12345#`
- Dispatcher user: email: `Test3@gmail.com`, password: `Test12345#`

## Development status

I work on this project in my free time, so it is not actively maintained. However, I am open to collaboration and contributions. If you are interested in contributing to this project, please feel free to reach out to me at **<suxrobgm@gmail.com>** or [Telegram](https://t.me/suxrobgm).

## Getting Started

Follow these steps to get the project up and running:

1. Install SDKs

   - [Download](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) and install the .NET 9 SDK.
   - Install Bun runtime to run an Angular project. Follow [these](https://bun.sh/docs/installation) instructions.
   - Download and install the PostgreSQL database from [here](https://www.postgresql.org/download/).
   - (Optional) Install Docker to run the application in containers. Follow [these](https://docs.docker.com/get-docker/) instructions.
   
> [!NOTE]
> If you prefer to run the application in containers, you can skip the steps related to installing dependencies and configuring the database connection strings. 
> Instead, you can use the .NET Aspire app to run the application in containers in a single command.
> To do this, follow the instructions in the [Running in Docker](#running-in-docker) section below.

2. Clone this repository:

   ```
   git clone https://github.com/suxrobgm/logistics-app.git
   cd logistics-app
   ```

3. Install Angular app dependencies:

   ```
   cd src\Client\Logistics.OfficeApp
   bun install
   ```

4. Update database connection strings:

   - Modify local or remote `PostgreSQL` database connection strings in the [Web API appsettings.json](./src/Presentation/Logistics.API/appsettings.json) and the [IdentityServer appsettings.json](./src/Presentation/Logistics.IdentityServer/appsettings.json) under the `ConnectionStrings:MasterDatabase` and `ConnectionStrings:DefaultTenantDatabase` section. Update tenant databases configuration in the [Web API appsettings.json](./src/Presentation/Logistics.API/appsettings.json) under the `TenantsConfig` section.

5. Seed databases:
   To initialize and populate the databases, run the `seed-databases.cmd` script provided in the repository.
   Alternatively, you can run the [Logistics.DbMigrator](./src/Presentation/Logistics.DbMigrator) project to seed the databases.

6. Run applications:
   Launch all the applications in the project using the respective `.cmd` scripts in the repository.

7. Stipe CLI (Optional):
    - If you want to test the Stripe payment integration, you need to run the Stripe CLI.
    - First, get your Stripe secret key and publishable key from your [Stripe dashboard](https://dashboard.stripe.com/apikeys).
    - Update the Stripe keys in the [Web API appsettings.json](./src/Presentation/Logistics.API/appsettings.json) under the `StripeConfig:SecretKey` and `StripeConfig:PublishableKey` sections.
    - The Stripe CLI is already included in the project, and you can run it using the provided [listen-stripe-webhook.cmd](./scripts/listen-stripe-webhook.cmd) script.
    - After running the script, Stripe will provide you a webhook secret key, which you need to update in the [Web API appsettings.json](./src/Presentation/Logistics.API/appsettings.json) under the `StripeConfig:WebhookSecret` section.

8. Access the applications:
   Use the following local URLs to access the apps:

   - Web API: <https://127.0.0.1:7000>
   - Identity Server: <https://127.0.0.1:7001>
   - Admin app: <https://127.0.0.1:7002>
   - Office app: <https://127.0.0.1:7003>

9. Login to the applications:
   Use the following test credentials to log in to the applications:
   - Admin web app:
     - Super admin user: email: `admin@gmail.com`, password: `Test12345#`
   - Office web app:
     - Owner user: email: `Test1@gmail.com`, password: `Test12345#`
     - Manager user: email: `Test2@gmail.com`, password: `Test12345#`
     - Dispatcher user: email: `Test3@gmail.com`, password: `Test12345#`
   - Driver mobile app:
     - Driver user: email: `Test6@gmail.com`, password: `Test12345#`

#### Running in Docker
To run the application in Docker containers, follow these steps:
1. Ensure you have Docker installed and running on your machine.
2. Open a terminal and run the following command to build and run the Docker containers:
```shell
dotnet run --project src/Aspire/Logistics.Aspire.AppHost
```

> [!NOTE]
> If you want to test payment integration, you need to obtain a Stripe secret key and update the Aspire [appsettings.json](./src/Aspire/Logistics.Aspire.AppHost/appsettings.json) file with your Stripe keys under the `Stripe` section.
> Also, you need to update the [Web API appsettings.json](./src/Presentation/Logistics.API/appsettings.json) with the same Stripe keys under the `StripeConfig` section.

The Aspire app will automatically build the necessary Docker images and start the containers for the Web API, Identity Server, Admin app, Stripe CLI, and Office app.
It will be accessible at the <https://localhost:8100>.

### Technical Stack

- .NET 9
- ASP.NET Core
- Entity Framework Core
- Deunde Identity Server
- FluentValidator
- MediatR
- PostgreSQL
- xUnit
- Moq
- Angular 19
- PrimeNG
- Blazor
- MAUI
- Firebase
- SignalR
- Docker
- CI/CD
- Bun
- Aspire

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

For commercial use, please contact me at **<suxrobgm@gmail.com>** or [Telegram](https://t.me/suxrobgm).

<!-- ## Office Web App Preview

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
![Driver App](./docs/driver_app_7.jpg?raw=true) -->
