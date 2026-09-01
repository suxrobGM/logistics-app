using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.Documents.Services;

/// <summary>
///     The caller a document request is being answered for.
/// </summary>
/// <param name="CallerId">The caller's user ID.</param>
/// <param name="IsReviewer">
///     Holds <c>Permission.Document.Review</c>, so the whole tenant's documents are in reach.
///     Without it the caller reaches only their own record, their trucks and the loads they drive.
/// </param>
public sealed record DocumentCaller(Guid CallerId, bool IsReviewer);

/// <summary>
///     Decides which documents the current user may read or change.
/// </summary>
public interface IDocumentAccessService : IApplicationService
{
    /// <summary>
    ///     Reads the caller's identity and document permissions. Null when they are unauthenticated
    ///     or hold no document permission at all.
    /// </summary>
    Task<DocumentCaller?> ResolveCallerAsync(CancellationToken ct = default);

    /// <summary>Checks whether the caller may reach one document.</summary>
    Task<bool> CanAccessAsync(DocumentCaller caller, Document document, CancellationToken ct = default);

    /// <summary>Checks whether the caller may reach the documents of one employee, truck or load.</summary>
    Task<bool> CanAccessOwnerAsync(
        DocumentCaller caller, DocumentOwnerType ownerType, Guid ownerId, CancellationToken ct = default);

    /// <summary>
    ///     Keeps only the documents the caller may reach. Resolves the caller's trucks and loads in
    ///     two queries rather than one per document.
    /// </summary>
    Task<List<TDocument>> FilterAccessibleAsync<TDocument>(
        DocumentCaller caller, List<TDocument> documents, CancellationToken ct = default)
        where TDocument : Document;
}
