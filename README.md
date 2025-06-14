# Logistics TMS 🚚: Automated Transport Management Solution

[![Build Status](https://github.com/suxrobgm/logistics-app/actions/workflows/build.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/build.yml)
[![Deployment](https://github.com/suxrobgm/logistics-app/actions/workflows/deploy-ssh.yml/badge.svg)](https://github.com/suxrobgm/logistics-app/actions/workflows/deploy-ssh.yml)

[![CC BY-NC 4.0][cc-by-nc-shield]][cc-by-nc]

[cc-by-nc]: https://creativecommons.org/licenses/by-nc/4.0/
[cc-by-nc-image]: https://licensebuttons.net/l/by-nc/4.0/88x31.png
[cc-by-nc-shield]: https://img.shields.io/badge/License-CC%20BY--NC%204.0-lightgrey.svg

> **Logistics TMS** is a multi‑tenant platform that automates every step of your transport workflow – from quoting and dispatching to real‑time GPS tracking, invoicing and payroll. Built with modern, cloud‑native technologies and a clean Domain‑Driven‑Design architecture, it scales from a single fleet to nationwide operations.

## 📑 Table of Contents

1. [Overview](#overview)
    - [Current Features](#current-features)
    - [Planned Features](#planned-features)
2. [Live Demo](#live-demo)
    - [Test Credentials](#test-credentials)
3. [Architecture](#architecture)
4. [Getting Started](#getting-started)
   - [Prerequisites](#prerequisites)
   - [Local Development](#local-development)
   - [Running with Docker](#running-with-docker)
5. [Tech Stack & Architecture](#tech-stack--architecture)
   - [Tech Stack](#tech-stack)
   - [Design Patterns](#design-patterns)
   - [Architecture](#architecture)
6. [Contributing](#contributing)
7. [License](#license)
8. [Contact](#contact)
9. [Screenshots](#screenshots)


## Overview
Logistics TMS is **purpose‑built for trucking fleets that specialise in moving large intermodal containers and finished vehicles**. Whether you are shuttling 40‑foot high‑cubes from the port to inland depots or running multi‑deck car carriers across state lines, the platform gives dispatchers, drivers, and customers a single source of truth.

Logistics TMS targets trucking companies that want to **replace error‑prone Excel workflows with an end‑to‑end digital system**—automating load planning, invoicing, payroll, and more. The platform is composed of:

| Layer         | Apps & Services                                                                       |
|---------------|---------------------------------------------------------------------------------------|
| **Front‑end** | *Admin Web*, *Office Web*, *Driver Mobile*                                            |
| **Back‑end**  | *Logistics API* (REST), *Identity Server* (OIDC/OAuth 2.0), *Real‑Time Hub* (SignalR) |
| **Data**      | Multi‑tenant PostgreSQL (one master + tenant shards)                                  |

All services are container‑ready and can be orchestrated on Kubernetes or run locally with .NET [Aspire](https://learn.microsoft.com/dotnet/aspire/).


### Current Features
- **Multi-Tenant Architecture**: Separate databases for each company, allowing for isolated data management.
- **Role-Based Access Control**: Different roles for users, including super admin, owner, manager, dispatcher, and driver.
- **Applications**:
  - **Admin Web App**: For super admins to manage users, tenants, and configurations.
  - **Office Web App**: For owners, managers, and dispatchers to manage transport operations.
  - **Driver Mobile App**: For drivers to manage their tasks and communicate with the office.
    - **Backend**:
      - **Web API**: Provides RESTful services for the applications.
      - **Identity Server**: Manages authentication and authorization.
- **Payment Integration**: Supports Stripe for payment processing.
- **Load Management**: Allows dispatchers to create, manage, and track loads such as big containers, car loads, and other shipments.
- **Employee Management**: Manage employees, including drivers and their tasks.
- **Notifications**: Real-time notifications for drivers and office users.
- **Payroll Management**: Manage employee payrolls and payments.
- **Analytics Dashboard**: Provides insight into operations, including load statistics, driver performance, and financial metrics.
- **GPS Tracking**: Real-time GPS tracking of drivers and vehicles.
- **Invoicing**: Generate and manage invoices for loads.

### Planned Features
- **Trip Management**: Manage trips, including start and end locations for trucks.
- **Document Management**: Upload and manage documents related to loads.
- **Reporting**: Generate detailed reports on loads, drivers, and financials.
- **Mobile App Enhancements**: Improve the driver mobile app with additional features like route optimization and offline capabilities.
- **Integration with Third-Party Services**: Integrate with other logistics and transportation services for enhanced functionality.
- **AI-Powered Features**: Implement AI-driven features for load optimization, route planning, and predictive analytics.


## Live Demo

A live demo of the application is available at [https://logistics-office.suxrobgm.net](https://logistics-office.suxrobgm.net). Use one of the following test credentials to log in to the application:

### Test Credentials

| Role        | E‑mail            | Password     | Access                 |
|-------------|-------------------|--------------|------------------------|
| Owner       | `Test1@gmail.com` | `Test12345#` | Office Web App         |
| Manager     | `Test2@gmail.com` | `Test12345#` | Office Web App         |
| Dispatcher  | `Test3@gmail.com` | `Test12345#` | Office Web App         |
| Driver      | `Test6@gmail.com` | `Test12345#` | Driver Mobile App only |
| Super Admin | `admin@gmail.com` | `Test12345#` | Admin Web App only     |

> [!NOTE]
> The demo uses **fake Stripe keys** and sample data. Do **not** enter real payment information.

## Getting Started

### Prerequisites

To run the Logistics TMS application locally, you need to have the following prerequisites installed on your machine:
   - [Download](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) and install the .NET 9 SDK.
   - Install Bun runtime to run an Angular project. Follow [these](https://bun.sh/docs/installation) instructions.
   - Download and install the PostgreSQL database from [here](https://www.postgresql.org/download/).
   - (Optional) Install Docker to run the application in containers. Follow [these](https://docs.docker.com/get-docker/) instructions.
   
> [!NOTE]
> If you prefer to run the application in containers, you can skip the steps related to installing dependencies and configuring the database connection strings. 
> Instead, you can use the .NET Aspire app to run the application in containers in a single command.
> To do this, follow the instructions in the [Running with Docker](#running-with-docker) section below.

### Local Development

1. Clone this repository:

   ```
   git clone https://github.com/suxrobgm/logistics-app.git
   cd logistics-app
   ```
   
2. Install Angular app dependencies:

   ```
   cd src\Client\Logistics.OfficeApp
   bun install
   ```

3. Update database connection strings:

   - Modify local or remote `PostgreSQL` database connection strings in the [Web API appsettings.json](./src/Presentation/Logistics.API/appsettings.json) and the [IdentityServer appsettings.json](./src/Presentation/Logistics.IdentityServer/appsettings.json) under the `ConnectionStrings:MasterDatabase` and `ConnectionStrings:DefaultTenantDatabase` section. Update tenant databases configuration in the [Web API appsettings.json](./src/Presentation/Logistics.API/appsettings.json) under the `TenantsConfig` section.

4. Seed databases:
   To initialize and populate the databases, run the `seed-databases.cmd` script provided in the repository.
   Alternatively, you can run the [Logistics.DbMigrator](./src/Presentation/Logistics.DbMigrator) project to seed the databases.

5. Run applications:
   Launch all the applications in the project using the respective `.cmd` scripts in the repository.

6. Stipe CLI (Optional):
    - If you want to test the Stripe payment integration, you need to run the Stripe CLI.
    - First, get your Stripe secret key and publishable key from your [Stripe dashboard](https://dashboard.stripe.com/apikeys).
    - Update the Stripe keys in the [Web API appsettings.json](./src/Presentation/Logistics.API/appsettings.json) under the `StripeConfig:SecretKey` and `StripeConfig:PublishableKey` sections.
    - The Stripe CLI is already included in the project, and you can run it using the provided [listen-stripe-webhook.cmd](./scripts/listen-stripe-webhook.cmd) script.
    - After running the script, Stripe will provide you a webhook secret key, which you need to update in the [Web API appsettings.json](./src/Presentation/Logistics.API/appsettings.json) under the `StripeConfig:WebhookSecret` section.

7. Access the applications:
   Use the following local URLs to access the apps:

   - Web API: <https://127.0.0.1:7000>
   - Identity Server: <https://127.0.0.1:7001>
   - Admin app: <https://127.0.0.1:7002>
   - Office app: <https://127.0.0.1:7003>

8. Login to the applications:
  Use the test credentials in the [Test Credentials](#test-credentials) section to log in to the applications. 
  See the access column in the table to determine which app you can access with each role.

#### Running with Docker
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

## Tech Stack & Architecture

### Tech Stack

- .NET 9
- ASP.NET Core
- Entity Framework Core
- Deunde Identity Server
- FluentValidator
- MediatR
- PostgreSQL
- xUnit
- Moq
- Angular 20
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

### Architecture
The Logistics TMS application follows a Domain-Driven Design architecture with a focus on separation of concerns and modularity. The architecture consists of the following layers:
- **Presentation Layer**: Contains API host, Identity Server, and Database Migrator projects.
- **Application Layer**: Contains the core business logic, including services, commands, and queries.
- **Domain Layer**: Contains the domain entities, value objects, and domain events.
- **Infrastructure Layer**: Contains the data access layer and repositories.
- **Client Apps**: Contains the Angular and Blazor applications for the Office and Admin interfaces, as well as the MAUI application for the Driver interface, and the HTTP client library for API communication.
- **Aspire App**: The .NET Aspire app orchestrates the entire applications and services, allowing for easy deployment and telemetry.

#### Architecture Diagram
![Architecture Diagram](./docs/project_architecture.jpg?raw=true)

## Contributing

> [!NOTE]
> I am working on this project in my spare time, so I may not always be available. However, I am open to collaboration and contributions.

Pull requests are welcome – whether it is a bug‑fix, new feature, or doc tweak.

1. Fork → Feature Branch → PR
2. Follow the existing coding conventions.

If you would like to discuss a larger change, open an issue first or ping me directly.

## License
This project is distributed under the [Creative Commons Attribution-NonCommercial 4.0 International License][cc-by-nc] license.

[![CC BY-NC 4.0][cc-by-nc-image]][cc-by-nc]

For commercial licensing, please contact me.

## Contact
Created with passion by **Sukhrob Ilyosbekov**.
- E‑mail: [suxrobgm@gmail.com](mailto:suxrobgm@gmail.com)
- Telegram: [@suxrobgm](https://t.me/suxrobgm)

## Screenshots

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
