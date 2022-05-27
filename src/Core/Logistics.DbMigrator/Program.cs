using Logistics.DbMigrator;
using Logistics.EntityFramework;
using Logistics.EntityFramework.Data;

var mainDatabaseConnection = ConnectionStrings.LocalMain;
var defaultTenantConnection = ConnectionStrings.LocalDefaultTenant;

Console.WriteLine("Main database connection string: " + mainDatabaseConnection);
Console.WriteLine("Initializing main database...");
await SeedData.InitializeAsync(new MainDbContext(mainDatabaseConnection));
Console.WriteLine();

Console.WriteLine("Default tenant's connection string: " + defaultTenantConnection);
Console.WriteLine("Initializing default tenant's database...");
await SeedData.InitializeAsync(new TenantDbContext(defaultTenantConnection));

Console.WriteLine("Finished!");
Console.ReadLine();
