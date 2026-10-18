using System.ClientModel;

namespace Logistics.Infrastructure.AI.Llm.Providers;

/// <summary>Translates the OpenAI SDK's failures, shared by both OpenAI-shaped providers.</summary>
internal static class OpenAIFailure
{
    /// <remarks>
    /// System.ClientModel reports a transport failure (no response at all) as status 0. The SDK has
    /// already retried 429 and 5xx by the time this sees them.
    /// </remarks>
    public static LlmProviderException From(ClientResultException ex) => new(
        ex.Status == 0 ? LlmFailureKind.Unavailable : LlmProviderException.KindFor(ex.Status),
        ex.Message,
        ex);
}
