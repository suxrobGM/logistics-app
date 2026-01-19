namespace Logistics.Aspire.AppHost;

public static class BuilderExtensions
{
    /// <param name="builder">The distributed application builder.</param>
    extension(IDistributedApplicationBuilder builder)
    {
        /// <summary>
        ///     Gets a configuration value based on execution context.
        ///     In publish mode: returns ${Key__Subkey} syntax for docker-compose env var substitution.
        ///     In run mode: returns the actual value from appsettings.
        /// </summary>
        /// <param name="key">The configuration key in "Section:Key" format (e.g., "Stripe:SecretKey").</param>
        /// <returns>Either the env var reference or actual config value.</returns>
        public string GetConfigValue(string key)
        {
            if (builder.ExecutionContext.IsPublishMode)
            {
                // Convert "Section:Key" to "${Section__Key}" for docker-compose
                var envVarName = key.Replace(":", "__");
                return $"${{{envVarName}}}";
            }

            return builder.Configuration[key] ?? string.Empty;
        }
    }
}
