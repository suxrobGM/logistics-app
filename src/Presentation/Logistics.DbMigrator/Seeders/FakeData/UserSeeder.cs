using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
///     Seeds test users from fake-dataset.json configuration.
/// </summary>
internal class UserSeeder(ILogger<UserSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(UserSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 100;

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        var testUsers = LoadUsersForRegion(context);
        if (testUsers is null || testUsers.Length == 0)
        {
            return true; // Skip if no users configured for this region
        }

        // Check if first test user already exists
        var firstUser = await context.UserManager.FindByNameAsync(testUsers[0].Email);
        return firstUser is not null;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();
        var testUsers = LoadUsersForRegion(context);
        var usersList = new List<User>();

        if (testUsers is null)
        {
            logger.LogWarning("No test users found in configuration");
            context.CreatedUsers = usersList;
            LogCompleted();
            return;
        }

        foreach (var fakeUser in testUsers)
        {
            var user = await context.UserManager.FindByNameAsync(fakeUser.Email);

            if (user is not null)
            {
                usersList.Add(user);
                continue;
            }

            user = new User
            {
                UserName = fakeUser.Email,
                FirstName = fakeUser.FirstName,
                LastName = fakeUser.LastName,
                Email = fakeUser.Email,
                EmailConfirmed = true
            };

            var result = await context.UserManager.CreateAsync(user, fakeUser.Password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create user {fakeUser.Email}: {result.Errors.First().Description}");
            }

            usersList.Add(user);
            logger.LogInformation("Created user {FirstName} {LastName}", fakeUser.FirstName, fakeUser.LastName);
        }

        context.CreatedUsers = usersList;
        LogCompleted(usersList.Count);
    }

    /// <summary>
    /// Pulls the user array from the region-keyed config section
    /// (e.g. <c>Us:Users</c> or <c>Eu:Users</c>) loaded from <c>SeedData/{us,eu}.json</c>.
    /// </summary>
    private static UserData[]? LoadUsersForRegion(SeederContext context)
    {
        var region = context.Region?.Region.ToString();
        if (string.IsNullOrEmpty(region))
        {
            return null;
        }
        return context.Configuration.GetSection($"{region}:Users").Get<UserData[]>();
    }
}
