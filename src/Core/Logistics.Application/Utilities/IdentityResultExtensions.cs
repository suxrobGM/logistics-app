using Microsoft.AspNetCore.Identity;

namespace Logistics.Application.Utilities;

public static class IdentityResultExtensions
{
    extension(IdentityResult result)
    {
        /// <summary>The failure descriptions joined into one message.</summary>
        public string DescribeErrors(string separator = ", ") =>
            string.Join(separator, result.Errors.Select(e => e.Description));
    }
}
