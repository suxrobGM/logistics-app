using Microsoft.IdentityModel.Tokens;

using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;

namespace Logistics.IdentityServer.Services.SigningKeys;

/// <summary>
///     Replaces the automatic key management the Duende package used to provide, which the
///     Open.IdentityServer fork does not carry.
///     <see cref="SigningKeyMaintenance"/> owns every write; this only reads.
/// </summary>
public class RotatingSigningKeyStore(SigningKeyCache cache) : ISigningCredentialStore, IValidationKeysStore
{
    public async Task<SigningCredentials> GetSigningCredentialsAsync()
    {
        return await cache.GetActiveAsync()
               ?? throw new InvalidOperationException(
                   "No signing key is available. SigningKeyMaintenance should have created one at startup.");
    }

    public Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync() => cache.GetValidationKeysAsync();
}
