using Logistics.DbMigrator;
using Logistics.EntityFramework.Data;

var connectionString = "Server=20.74.27.42; Database=rate-pic; Uid=frostpixel; Pwd=DO8N6BvdEQwMIKHN; Connect Timeout=10";

Console.WriteLine("Connection string: " + connectionString);
Console.WriteLine("Initializing database...");

await SeedData.InitializeAsync(new DatabaseContext(connectionString));

Console.WriteLine("Finished!");
Console.ReadLine();
