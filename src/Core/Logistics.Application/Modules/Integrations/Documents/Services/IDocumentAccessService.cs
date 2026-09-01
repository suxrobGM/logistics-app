using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.Documents.Services;

/// <summary>
///     The caller a document request is being answered for. Management sees every document in the
///     tenant; a driver sees only their own record, their trucks and the loads they drive.
/// </summary>
public sealed record DocumentCaller(Guid CallerId, bool IsManagement, bool IsDriver);

/// <summary>
///     Decides which documents the current user may read or change.
/// </summary>
public interface IDocumentAccessService : IApplicationService
{
    /// <summary>Reads the current user's identity and role, or null when neither can be resolved.</summary>
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
