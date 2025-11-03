using Microsoft.Extensions.Configuration;

namespace Logistics.DriverApp.Extensions;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder AddJsonConfig(this IConfigurationBuilder configurationBuilder, string configFile)
    {
        try
        {
            var config = typeof(App).Assembly.GetManifestResourceStream($"Logistics.DriverApp.{configFile}");

            if (config == null)
                return configurationBuilder;

            configurationBuilder.AddJsonStream(config);
            return configurationBuilder;
        }
        catch (Exception)
        {
            // ignored
            return configurationBuilder;
        }
    }
}
