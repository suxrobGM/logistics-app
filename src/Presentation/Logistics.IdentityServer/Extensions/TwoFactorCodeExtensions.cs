namespace Logistics.IdentityServer.Extensions;

public static class TwoFactorCodeExtensions
{
    extension(string code)
    {
        /// <summary>Strips the spaces and dashes authenticator apps display between digit groups.</summary>
        public string NormalizeVerificationCode()
        {
            return code.Replace(" ", string.Empty).Replace("-", string.Empty);
        }
    }
}
