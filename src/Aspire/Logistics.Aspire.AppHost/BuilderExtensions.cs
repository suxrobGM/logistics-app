using Aspire.Hosting.Docker.Resources.ComposeNodes;

namespace Logistics.Aspire.AppHost;

public static class BuilderExtensions
{
    /// <summary>
    ///     Sets the Docker Compose <c>restart</c> policy for the resource so the container is
    ///     brought back up automatically after a crash or a server reboot. Only takes effect in
    ///     publish mode (when the docker-compose.yaml is generated); a no-op in run mode.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="policy">The restart policy: "no", "always", "on-failure", or "unless-stopped".</param>
    public static IResourceBuilder<T> WithComposeRestartPolicy<T>(
        this IResourceBuilder<T> builder, string policy = "unless-stopped")
        where T : IComputeResource
    {
        return builder.PublishAsDockerComposeService((_, service) => service.Restart = policy);
    }

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
