using Logistics.Application.Modules.Compliance.Privacy.Commands;
using Logistics.Application.Modules.Compliance.Privacy.Queries;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Logistics.IdentityServer.Pages.Account.Manage.Privacy;

/// <summary>
/// GDPR self-service: data export, account deletion and consent history. Lives here rather than in a
/// tenant portal because these rights belong to the user - any signed-in user reaches it, regardless
/// of role, tenant or subscription.
/// </summary>
public class PrivacyModel(IMediator mediator) : PageModel
{
    /// <summary>Single source for the grace-period copy, so the page can't drift from the job that enforces it.</summary>
    public int GracePeriodDays => PrivacyDefaults.DeletionGracePeriod.Days;

    public IReadOnlyList<DataExportRequestDto> Exports { get; private set; } = [];
    public IReadOnlyList<ConsentRecordDto> Consents { get; private set; } = [];
    public DataDeletionRequestDto? PendingDeletion { get; private set; }

    [TempData] public string? StatusMessage { get; set; }

    [BindProperty] public string? DeleteReason { get; set; }

    public Task OnGetAsync(CancellationToken ct) => LoadAsync(ct);

    public async Task<IActionResult> OnPostRequestExportAsync(CancellationToken ct) =>
        Done(await mediator.Send(new RequestDataExportCommand(), ct),
            "Data export requested. We'll email you when it's ready (usually within a few minutes).");

    /// <summary>Re-fetches to mint a fresh signed URL; the rendered one would already be stale.</summary>
    public async Task<IActionResult> OnPostDownloadAsync(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetDataExportRequestQuery { Id = id }, ct);

        if (result.IsSuccess && string.IsNullOrEmpty(result.Value?.DownloadUrl))
        {
            StatusMessage = "Error: Download link unavailable. Try again in a moment.";
            return RedirectToPage();
        }

        return result.IsSuccess ? Redirect(result.Value!.DownloadUrl!) : Done(result, string.Empty);
    }

    public async Task<IActionResult> OnPostRequestDeletionAsync(CancellationToken ct)
    {
        var command = new RequestDataDeletionCommand
        {
            Reason = string.IsNullOrWhiteSpace(DeleteReason) ? null : DeleteReason.Trim()
        };

        return Done(await mediator.Send(command, ct),
            $"Deletion scheduled. You have {GracePeriodDays} days to cancel before your data is anonymized.");
    }

    public async Task<IActionResult> OnPostCancelDeletionAsync(Guid id, CancellationToken ct) =>
        Done(await mediator.Send(new CancelDataDeletionCommand { Id = id }, ct),
            "Deletion request cancelled.");

    /// <summary>Outcome into the status banner, then POST-redirect-GET.</summary>
    private IActionResult Done(Logistics.Shared.Models.IResult result, string successMessage)
    {
        StatusMessage = result.IsSuccess ? successMessage : $"Error: {result.Error}";
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        // Sequential by necessity - they share the request-scoped MasterDbContext, so Task.WhenAll
        // would trip EF's "second operation on this context" guard.
        var exports = await mediator.Send(new GetMyDataExportsQuery(), ct);
        var deletions = await mediator.Send(new GetMyDataDeletionsQuery(), ct);
        var consents = await mediator.Send(new GetConsentHistoryQuery(), ct);

        Exports = exports.Value ?? [];
        Consents = consents.Value ?? [];
        PendingDeletion = deletions.Value?.FirstOrDefault(d => d.Status == DataDeletionStatus.Pending);
    }

    /// <summary>Bootstrap contextual class for an export status badge.</summary>
    public string ExportStatusClass(DataExportStatus status) => status switch
    {
        DataExportStatus.Ready => "bg-success",
        DataExportStatus.Pending or DataExportStatus.Processing => "bg-info",
        DataExportStatus.Failed => "bg-danger",
        _ => "bg-secondary"
    };
}
