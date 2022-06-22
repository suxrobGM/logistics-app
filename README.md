# LogisticsApp
LogisticsApp is a system for managing drivers and dispatchers in a logistics company.

## Project architecture
![Project architecture diagram](./docs/project_architecture.jpg?raw=true)

## How to run?
1. [Download](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) and install the .NET 6 SDK. 
2. Clone this repository:
```
$ git clone https://github.com/suxrobGM/LogisticsApp.git
```
3. Run the Web API project:
```
$ cd LogisticsApp/src
$ dotnet run --project Api/Logistics.WebApi
```

4. Seed database 
```
$ dotnet run --project Core/Logistics.DbMigrator
```

5. Run the Identity Server:
```
$ dotnet run --project Apps/Logistics.IdentityServer
```

6. Run the Admin App:
```
$ dotnet run --project Apps/Logistics.AdminApp
```

7. Run the Office App:
```
$ dotnet run --project Apps/Logistics.OfficeApp
```

Project local URLs:
- Web API: https://127.0.0.1:7000
- Identity Server: https://127.0.0.1:7001
- Admin app: https://127.0.0.1:7002
- Office app: https://127.0.0.1:7003