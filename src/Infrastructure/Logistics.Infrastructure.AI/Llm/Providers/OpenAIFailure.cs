using System.ClientModel;

namespace Logistics.Infrastructure.AI.Llm.Providers;

internal static class OpenAIFailure
{
    /// <summary>
    /// System.ClientModel reports a transport failure as status 0. The SDK has already retried 429
    /// and 5xx by now.
    /// </summary>
    public static LlmProviderException From(ClientResultException ex) => new(
        ex.Status == 0 ? LlmFailureKind.Unavailable : LlmProviderException.KindFor(ex.Status),
        ex.Message,
        ex);
}
