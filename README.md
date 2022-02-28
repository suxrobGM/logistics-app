# LogisticsApp
LogisticsApp is a system for managing drivers and dispatchers in a logistics company.

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
4. Run the Admin App:
```
$ dotnet run --project Apps/Logistics.AdminApp
```

By default, the API project runs on localhost https://127.0.0.1:7182 and the admin app runs on https://127.0.0.1:7015