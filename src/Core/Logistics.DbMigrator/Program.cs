using Logistics.DbMigrator;
using Logistics.EntityFramework;
using Logistics.EntityFramework.Data;

var connectionString = DefualtConnection.ConnectionString;

Console.WriteLine("Connection string: " + connectionString);
Console.WriteLine("Initializing database...");

await SeedData.InitializeAsync(new DatabaseContext(connectionString));

Console.WriteLine("Finished!");
Console.ReadLine();
