using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.Financial.Invoices.Commands;
using Logistics.Application.Modules.Financial.Invoices.Queries;
using Logistics.Application.Modules.Operations.Loads.Queries;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;
using Logistics.Mediator;

namespace Logistics.Infrastructure.AI.Tools.Financial;

internal sealed class CreateLoadInvoiceTool(IMediator mediator)
    : AgentTool<CreateLoadInvoiceTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The load ID (GUID) to invoice")]
        [AgentEntityId(AgentEntityKind.Load)]
        public required Guid LoadId { get; init; }

        [Description("Brief explanation of why this invoice should be created")]
        public required string Reasoning { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "create_load_invoice",
        "Create an UNPAID invoice for a load that has none, billed to the load's customer with one line per load charge (linehaul, fuel surcharge, accessorials). Call get_load first: new loads usually already have an invoice, and you should warn the user when the load is not yet Delivered. To bill a different amount the user changes the load's charges. Fails if the load already has an invoice.")
    {
        RequiredFeature = TenantFeature.Invoices,
        RequiredPermission = Permission.Invoice.Manage,
        DecisionType = AgentDecisionType.CreateInvoice
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        var loadResult = await mediator.Send(new GetLoadByIdQuery { Id = input.LoadId }, ct);
        if (!loadResult.IsSuccess || loadResult.Value is null)
            return ToolResult.Error(loadResult.Error ?? "Load not found");

        var load = loadResult.Value;
        if (load.Customer is null)
            return ToolResult.Error("The load has no customer to invoice");

        if (load.Invoice is not null)
        {
            return ToolResult.Ok(new
            {
                error = "The load already has an invoice",
                invoice_id = load.Invoice.Id,
                invoice_status = load.Invoice.Status.ToString()
            });
        }

        var result = await mediator.Send(new CreateLoadInvoiceCommand
        {
            LoadId = input.LoadId
        }, ct);

        if (!result.IsSuccess)
            return ToolResult.WriteFailed(result);

        // Read the created invoice back for its number and totals, which the command does not return.
        var created = await mediator.Send(new GetInvoiceByIdQuery { Id = result.Value }, ct);

        var invoice = created.Value;
        return ToolResult.Ok(new
        {
            success = true,
            invoice_id = result.Value,
            invoice_number = invoice?.Number,
            total = invoice?.Total.Amount,
            currency = invoice?.Total.Currency,
            load_number = load.Number,
            customer = load.Customer.Name
        });
    }
}
