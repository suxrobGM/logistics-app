using System.Security.Cryptography;
using System.Text;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Credit;

/// <summary>
/// Deterministic pseudo-credit derived from a hash of the MC number so demo flows are
/// reproducible offline. Shared by the demo listing generator and the credit service so
/// a listing's badge always matches an on-demand check for the same broker.
/// </summary>
internal static class DemoBrokerCredit
{
    /// <summary>Scores 30-100, days-to-pay 15-59, authority inactive below score 35.</summary>
    public static (int Score, int DaysToPay, bool AuthorityActive) Compute(string mcNumber)
    {
        var digits = new string(mcNumber.Where(char.IsDigit).ToArray());
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(digits));
        var seed = BitConverter.ToUInt32(hash, 0);
        var score = 30 + (int)(seed % 71);

        return (score, 15 + (int)(seed % 45), score >= 35);
    }
}
