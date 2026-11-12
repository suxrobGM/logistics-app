using Logistics.Domain.Core;
using Logistics.Infrastructure.Persistence.Converters;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Persistence.Extensions;

public static class EncryptedColumnsExtensions
{
    /// <summary>Never change it, or stored secrets can no longer be decrypted.</summary>
    private const string ProtectorPurpose = "Logistics.ProviderSecrets.v1";

    extension(ModelBuilder builder)
    {
        /// <summary>
        ///     Call after the entity configurations so it overrides them. A null provider (design time, tests)
        ///     leaves the columns plaintext with the same schema.
        /// </summary>
        public void ApplyEncryptedSecretColumns(
            IDataProtectionProvider? dataProtectionProvider)
        {
            if (dataProtectionProvider is null)
            {
                return;
            }

            var converter = new EncryptedStringConverter(
                dataProtectionProvider.CreateProtector(ProtectorPurpose));

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(string) &&
                        property.PropertyInfo?.IsDefined(typeof(EncryptedSecretAttribute), inherit: false) == true)
                    {
                        property.SetValueConverter(converter);
                    }
                }
            }
        }
    }
}
