using System.Text.Json.Nodes;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;

namespace Logistics.Infrastructure.AI.Services;

internal sealed class AIDispatchToolRegistry : IAIDispatchToolRegistry
{
    private static readonly List<AIDispatchToolDefinition> Tools =
    [
        // ── Read Tools ──

        new("get_unassigned_loads",
            "Get all Draft loads that are not assigned to any trip. Returns load ID, name, type, origin, destination, distance, delivery cost, and customer.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject(),
                ["required"] = new JsonArray()
            }),
            RequiredPermission: Permission.Dispatch.View,
            DispatchAgent: true),

        new("get_available_trucks",
            "Get all trucks with Available status along with their driver info, HOS (Hours of Service) status, and a fleet summary (total trucks, available trucks, active trips, drivers in violation). Returns truck ID, number, type, current location, driver name, and remaining driving/on-duty hours.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject(),
                ["required"] = new JsonArray()
            }),
            RequiredPermission: Permission.Dispatch.View,
            DispatchAgent: true),

        new("get_driver_hos_status",
            "Get detailed HOS (Hours of Service) status for a specific driver. Returns current duty status, driving minutes remaining, on-duty minutes remaining, cycle minutes remaining, violation status, and next mandatory break time.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["driver_id"] = Prop("string", "The driver's employee ID (GUID)")
                },
                ["required"] = new JsonArray("driver_id")
            }),
            RequiredPermission: Permission.Dispatch.View,
            DispatchAgent: true),

        new("check_hos_feasibility",
            "Check if a driver can feasibly complete a trip given the estimated driving distance. Returns whether the driver has enough HOS hours remaining and details about any constraints.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["driver_id"] = Prop("string", "The driver's employee ID (GUID)"),
                    ["distance_km"] = Prop("number", "Estimated driving distance in kilometers")
                },
                ["required"] = new JsonArray("driver_id", "distance_km")
            }),
            RequiredPermission: Permission.Dispatch.View,
            DispatchAgent: true),

        new("check_dispatch_eligibility",
            "Check if a truck (and optionally a specific driver) is eligible to carry a load based on driver license class + endorsements, US Hazmat / EU ADR rules, ADR cert validity, truck Hazmat-placarding, and DOT medical certificate. Returns is_eligible and a list of issues with reason codes (severity: error blocks dispatch, warning is informational). Call this BEFORE dispatch_trip or assign_load_to_truck on hazmat / ADR loads, and whenever the human asks 'can driver X carry load Y'.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["truck_id"] = Prop("string", "The truck ID (GUID)"),
                    ["load_id"] = Prop("string", "The load ID (GUID)"),
                    ["driver_id"] = Prop("string",
                        "Optional driver ID (GUID). When omitted, the truck's currently assigned main driver is used.")
                },
                ["required"] = new JsonArray("truck_id", "load_id")
            }),
            RequiredPermission: Permission.Dispatch.View,
            DispatchAgent: true),

        new("batch_check_hos_feasibility",
            "Check HOS feasibility for multiple driver-distance pairs in a single call. More efficient than calling check_hos_feasibility multiple times. Returns feasibility result for each pair.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["checks"] = new JsonObject
                    {
                        ["type"] = "array",
                        ["description"] = "Array of driver/distance pairs to check",
                        ["items"] = new JsonObject
                        {
                            ["type"] = "object",
                            ["properties"] = new JsonObject
                            {
                                ["driver_id"] = Prop("string", "The driver's employee ID (GUID)"),
                                ["distance_km"] = Prop("number", "Estimated driving distance in kilometers")
                            },
                            ["required"] = new JsonArray("driver_id", "distance_km")
                        }
                    }
                },
                ["required"] = new JsonArray("checks")
            }),
            RequiredPermission: Permission.Dispatch.View,
            DispatchAgent: true),

        new("calculate_distance",
            "Calculate the driving distance and estimated duration between two geographic points.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["origin_lat"] = Prop("number", "Origin latitude"),
                    ["origin_lng"] = Prop("number", "Origin longitude"),
                    ["dest_lat"] = Prop("number", "Destination latitude"),
                    ["dest_lng"] = Prop("number", "Destination longitude")
                },
                ["required"] = new JsonArray("origin_lat", "origin_lng", "dest_lat", "dest_lng")
            }),
            RequiredPermission: Permission.Dispatch.View,
            DispatchAgent: true),

        new("calculate_assignment_metrics",
            "Calculate revenue per mile/km, deadhead ratio, and profitability for candidate truck-load pairs. Use this when multiple trucks are candidates for a load to pick the most profitable option.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["candidates"] = new JsonObject
                    {
                        ["type"] = "array",
                        ["description"] = "Array of truck/load pairs to evaluate",
                        ["items"] = new JsonObject
                        {
                            ["type"] = "object",
                            ["properties"] = new JsonObject
                            {
                                ["load_id"] = Prop("string", "The load ID (GUID)"),
                                ["truck_id"] = Prop("string", "The truck ID (GUID)")
                            },
                            ["required"] = new JsonArray("load_id", "truck_id")
                        }
                    }
                },
                ["required"] = new JsonArray("candidates")
            }),
            RequiredPermission: Permission.Dispatch.View,
            DispatchAgent: true),

        new("preview_tax_calculation",
            "Compute VAT / sales tax / GST for a hypothetical set of line items without persisting an invoice. Returns per-line tax amount, aggregate breakdown by jurisdiction, and reverse-charge / not-collecting flags. Use when quoting a customer or sanity-checking that tax setup will work before creating the invoice. Read-only.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["customer_id"] = Prop("string", "The customer ID (GUID) - drives jurisdiction + reverse-charge"),
                    ["currency"] = Prop("string", "ISO-4217 currency code, e.g. 'USD' or 'EUR'"),
                    ["line_items"] = new JsonObject
                    {
                        ["type"] = "array",
                        ["description"] = "Line items to score",
                        ["items"] = new JsonObject
                        {
                            ["type"] = "object",
                            ["properties"] = new JsonObject
                            {
                                ["description"] = Prop("string", "Human-readable label for the line"),
                                ["type"] = Prop("string", "InvoiceLineItemType (BaseRate, FuelSurcharge, Detention, Lumper, Other, ...)"),
                                ["amount"] = Prop("number", "Per-unit net amount in the invoice currency"),
                                ["quantity"] = Prop("integer", "Quantity (defaults to 1)"),
                                ["tax_code"] = Prop("string", "Optional Stripe Tax product code (txcd_*); leave blank to use the tenant default")
                            },
                            ["required"] = new JsonArray("description", "amount")
                        }
                    }
                },
                ["required"] = new JsonArray("customer_id", "currency", "line_items")
            }),
            RequiredPermission: Permission.Dispatch.View,
            DispatchAgent: true),

        new("get_container_status",
            "Look up an intermodal container by its ISO 6346 number. Returns status (Empty, Loaded, AtPort, InTransit, Delivered, Returned), ISO type, laden flag, gross weight, seal, booking reference, bill of lading, the terminal the box is currently at, and the load it is linked to. Call this for any load that reports a container_number before assigning it.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["container_number"] = Prop("string", "ISO 6346 container number, e.g. 'MSCU1234567'"),
                    ["container_id"] = Prop("string", "Container ID (GUID) - use only when the number is unknown")
                },
                ["required"] = new JsonArray()
            }),
            TenantFeature.IntermodalContainers,
            RequiredPermission: Permission.Dispatch.View,
            DispatchAgent: true),

        new("get_terminal_info",
            "Look up an intermodal terminal by UN/LOCODE. Returns name, type (SeaPort, RailTerminal, InlandDepot, AirCargo, BorderCrossing), country, street address, and how many containers are currently sitting there. Terminals carry no coordinates - keep using the load's origin_lat/origin_lng for deadhead math.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["code"] = Prop("string", "UN/LOCODE, e.g. 'USLAX' (Los Angeles), 'BEANR' (Antwerp)"),
                    ["terminal_id"] = Prop("string", "Terminal ID (GUID) - use only when the code is unknown")
                },
                ["required"] = new JsonArray()
            }),
            TenantFeature.IntermodalContainers,
            RequiredPermission: Permission.Dispatch.View,
            DispatchAgent: true),

        new("search_loadboard",
            "Search load boards (DAT, Truckstop, 123Loadboard) for available loads matching criteria. Use this when trucks have capacity gaps to find revenue opportunities.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["origin_city"] = Prop("string", "Origin city name"),
                    ["origin_state"] = Prop("string", "Origin state code (e.g., 'TX', 'CA')"),
                    ["radius_miles"] = Prop("number", "Search radius in miles from origin (default: 100)"),
                    ["destination_state"] = Prop("string", "Optional destination state filter")
                },
                ["required"] = new JsonArray("origin_city", "origin_state")
            }),
            TenantFeature.LoadBoard,
            RequiredPermission: Permission.Dispatch.View,
            DispatchAgent: true),

        new("check_broker_credit",
            "Check a load-board broker's credit standing (score 0-100, days-to-pay, FMCSA authority status) by MC number. Always call this before book_loadboard_load; never book when the score is below the tenant minimum or the authority is inactive.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["mc_number"] = Prop("string", "The broker's MC number, e.g. 'MC123456' or '123456'")
                },
                ["required"] = new JsonArray("mc_number")
            }),
            TenantFeature.LoadBoard,
            RequiredPermission: Permission.Dispatch.View,
            DispatchAgent: true),

        new("search_loads",
            "Search loads by status, type, customer, truck, date range, or free text. Returns up to 20 loads per page with number, status, origin/destination, delivery cost, and customer. Use for load history questions ('delivered loads last week', 'loads for customer X').",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["search"] = Prop("string", "Free-text search over load name and reference fields"),
                    ["statuses"] = new JsonObject
                    {
                        ["type"] = "array",
                        ["items"] = new JsonObject { ["type"] = "string" },
                        ["description"] = "Filter by statuses: Draft, Dispatched, PickedUp, Delivered, Cancelled"
                    },
                    ["types"] = new JsonObject
                    {
                        ["type"] = "array",
                        ["items"] = new JsonObject { ["type"] = "string" },
                        ["description"] = "Filter by load types, e.g. GeneralFreight, RefrigeratedGoods, IntermodalContainer"
                    },
                    ["customer_id"] = Prop("string", "Filter by customer ID (GUID) - find it with search_customers"),
                    ["truck_id"] = Prop("string", "Filter by assigned truck ID (GUID)"),
                    ["start_date"] = Prop("string", "Loads created on or after this date (ISO 8601)"),
                    ["end_date"] = Prop("string", "Loads created on or before this date (ISO 8601)"),
                    ["page"] = Prop("integer", "Page number when a previous call returned truncated: true")
                },
                ["required"] = new JsonArray()
            }),
            TenantFeature.Loads,
            RequiredPermission: Permission.Load.View),

        new("get_load",
            "Get one load by ID: status, addresses, delivery cost, customer (with email), assigned truck, delivery timestamps, and whether it already has an invoice. ALWAYS call this before create_load_invoice - it supplies the delivery cost and shows whether the load is Delivered.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["load_id"] = Prop("string", "The load ID (GUID)")
                },
                ["required"] = new JsonArray("load_id")
            }),
            TenantFeature.Loads,
            RequiredPermission: Permission.Load.View),

        new("search_customers",
            "Search customers by name. The match is a case-sensitive substring - if nothing is found, retry with a shorter fragment (e.g. 'Acme' instead of 'acme logistics'). Returns up to 20 customers with ID, name, email, and phone.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["search"] = Prop("string", "Customer name or name fragment (case-sensitive substring match)")
                },
                ["required"] = new JsonArray()
            }),
            TenantFeature.Customers,
            RequiredPermission: Permission.Customer.View),

        new("get_invoices",
            "List load invoices filtered by load, customer, status, overdue flag, or date range. Returns up to 20 per page with number, status, total, due date, and customer.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["load_id"] = Prop("string", "Filter by load ID (GUID)"),
                    ["customer_id"] = Prop("string", "Filter by customer ID (GUID)"),
                    ["status"] = Prop("string", "Filter by status: Draft, Issued, Sent, PartiallyPaid, Paid, Overdue, Cancelled"),
                    ["overdue_only"] = Prop("boolean", "Only invoices past their due date"),
                    ["start_date"] = Prop("string", "Invoices created on or after this date (ISO 8601)"),
                    ["end_date"] = Prop("string", "Invoices created on or before this date (ISO 8601)"),
                    ["page"] = Prop("integer", "Page number when a previous call returned truncated: true")
                },
                ["required"] = new JsonArray()
            }),
            TenantFeature.Invoices,
            RequiredPermission: Permission.Invoice.View),

        new("get_invoice",
            "Get one invoice by ID: status, totals, amount paid, due date, send history, customer (with email), and the load it bills.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["invoice_id"] = Prop("string", "The invoice ID (GUID)")
                },
                ["required"] = new JsonArray("invoice_id")
            }),
            TenantFeature.Invoices,
            RequiredPermission: Permission.Invoice.View),

        new("search_expenses",
            "List expense line items filtered by type (Company, Truck, BodyShop), status, truck, date range, or vendor/notes text. Returns up to 20 per page. There is no category filter - for spend-by-category questions use get_expense_stats instead.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["type"] = Prop("string", "Expense type: Company, Truck, or BodyShop"),
                    ["status"] = Prop("string", "Expense status: Pending, Approved, Rejected, or Paid"),
                    ["truck_id"] = Prop("string", "Filter by truck ID (GUID)"),
                    ["from_date"] = Prop("string", "Expenses on or after this date (ISO 8601)"),
                    ["to_date"] = Prop("string", "Expenses on or before this date (ISO 8601)"),
                    ["search"] = Prop("string", "Search vendor name or notes"),
                    ["page"] = Prop("integer", "Page number when a previous call returned truncated: true")
                },
                ["required"] = new JsonArray()
            }),
            TenantFeature.Expenses,
            RequiredPermission: Permission.Expense.View),

        new("get_expense_stats",
            "Expense rollups for a date range: totals by approval status, by type, by company/truck category, 12-month trend, and top trucks by spend. Prefer this over search_expenses for 'how much did we spend on X' questions.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["from_date"] = Prop("string", "Start of the period (ISO 8601)"),
                    ["to_date"] = Prop("string", "End of the period (ISO 8601)")
                },
                ["required"] = new JsonArray()
            }),
            TenantFeature.Expenses,
            RequiredPermission: Permission.Expense.View),

        new("get_upcoming_maintenance",
            "Trucks with maintenance due within the next N days (default 30), including overdue items. Date-based schedules only: mileage and engine-hour intervals are NOT evaluated - say so when answering maintenance questions.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["days_ahead"] = Prop("integer", "Look-ahead window in days (default 30)"),
                    ["truck_id"] = Prop("string", "Limit to one truck (GUID)"),
                    ["include_overdue"] = Prop("boolean", "Include already-overdue schedules (default true)")
                },
                ["required"] = new JsonArray()
            }),
            TenantFeature.Maintenance,
            RequiredPermission: Permission.Maintenance.View),

        // ── Write Tools ──

        new("assign_load_to_truck",
            "Assign a specific load to a specific truck. In human-in-the-loop mode, this creates a suggestion for dispatcher approval. In autonomous mode, the assignment is executed immediately.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["load_id"] = Prop("string", "The load ID (GUID) to assign"),
                    ["truck_id"] = Prop("string", "The truck ID (GUID) to assign the load to"),
                    ["reasoning"] = Prop("string", "Brief explanation of why this assignment is optimal")
                },
                ["required"] = new JsonArray("load_id", "truck_id", "reasoning")
            }),
            IsWrite: true,
            RequiredPermission: Permission.Dispatch.Manage,
            DecisionType: AIDispatchDecisionType.AssignLoad,
            DispatchAgent: true),

        new("create_trip",
            "Create a new trip from a set of loads assigned to a truck. Groups multiple loads into an optimized multi-stop trip. In human-in-the-loop mode, creates a suggestion for approval.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["truck_id"] = Prop("string", "The truck ID (GUID) for this trip"),
                    ["load_ids"] = new JsonObject
                    {
                        ["type"] = "array",
                        ["items"] = new JsonObject { ["type"] = "string" },
                        ["description"] = "List of load IDs (GUIDs) to include in the trip"
                    },
                    ["name"] = Prop("string", "A descriptive name for the trip"),
                    ["optimize_stops"] = Prop("boolean", "Whether to optimize stop ordering (default: true)")
                },
                ["required"] = new JsonArray("truck_id", "load_ids", "name")
            }),
            IsWrite: true,
            RequiredPermission: Permission.Dispatch.Manage,
            DecisionType: AIDispatchDecisionType.CreateTrip,
            DispatchAgent: true),

        new("dispatch_trip",
            "Dispatch a trip, transitioning it from Draft to Dispatched status. This notifies the driver and starts the trip. In human-in-the-loop mode, creates a suggestion for approval.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["trip_id"] = Prop("string", "The trip ID (GUID) to dispatch")
                },
                ["required"] = new JsonArray("trip_id")
            }),
            IsWrite: true,
            RequiredPermission: Permission.Dispatch.Manage,
            DecisionType: AIDispatchDecisionType.DispatchTrip,
            DispatchAgent: true),

        new("book_loadboard_load",
            "Book a load from a load board. This claims the load and creates it in the system. In human-in-the-loop mode, creates a suggestion for approval.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["external_listing_id"] = Prop("string", "The load board listing ID to book"),
                    ["provider"] = Prop("string", "The load board provider (DAT, Truckstop, OneTwo3Loadboard)"),
                    ["truck_id"] = Prop("string", "The truck ID (GUID) to assign the booked load to")
                },
                ["required"] = new JsonArray("external_listing_id", "provider", "truck_id")
            }),
            TenantFeature.LoadBoard,
            IsWrite: true,
            RequiredPermission: Permission.Dispatch.Manage,
            DecisionType: AIDispatchDecisionType.BookLoadBoardLoad,
            DispatchAgent: true),

        new("create_load_invoice",
            "Create an UNPAID invoice for a load, billed to the load's customer. Call get_load first: the amount defaults to the load's delivery cost, and you should warn the user when the load is not yet Delivered. Fails if the load already has an invoice. In human-in-the-loop mode, creates a suggestion for approval.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["load_id"] = Prop("string", "The load ID (GUID) to invoice"),
                    ["amount"] = Prop("number", "Invoice total. Omit to use the load's delivery cost - only set when the user explicitly names a different amount"),
                    ["reasoning"] = Prop("string", "Brief explanation of why this invoice should be created")
                },
                ["required"] = new JsonArray("load_id", "reasoning")
            }),
            TenantFeature.Invoices,
            IsWrite: true,
            RequiredPermission: Permission.Invoice.Manage,
            DecisionType: AIDispatchDecisionType.CreateInvoice),

        new("send_invoice",
            "Email an invoice to a recipient. This mints a 30-day payment link and includes it in the email - do NOT also call create_payment_link for the same invoice. In human-in-the-loop mode, creates a suggestion for approval.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["invoice_id"] = Prop("string", "The invoice ID (GUID) to send"),
                    ["recipient_email"] = Prop("string", "Recipient email address - use the customer's email from get_load or get_invoice unless the user gives another"),
                    ["personal_message"] = Prop("string", "Optional note included in the email body"),
                    ["reasoning"] = Prop("string", "Brief explanation of why this invoice should be sent")
                },
                ["required"] = new JsonArray("invoice_id", "recipient_email", "reasoning")
            }),
            TenantFeature.Invoices,
            IsWrite: true,
            RequiredPermission: Permission.Invoice.Manage,
            DecisionType: AIDispatchDecisionType.SendInvoice),

        new("create_payment_link",
            "Create a public payment link URL for an invoice without emailing anything. Use when the user wants a link to share themselves; send_invoice already includes one. In human-in-the-loop mode, creates a suggestion for approval.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["invoice_id"] = Prop("string", "The invoice ID (GUID) to create the link for"),
                    ["expiration_days"] = Prop("integer", "Days until the link expires (default 30)"),
                    ["reasoning"] = Prop("string", "Brief explanation of why this link should be created")
                },
                ["required"] = new JsonArray("invoice_id", "reasoning")
            }),
            TenantFeature.Payments,
            IsWrite: true,
            RequiredPermission: Permission.Payment.Manage,
            DecisionType: AIDispatchDecisionType.CreatePaymentLink)
    ];

    private static readonly Dictionary<string, AIDispatchToolDefinition> ToolsByName =
        Tools.ToDictionary(t => t.Name);

    public IReadOnlyList<AIDispatchToolDefinition> GetToolDefinitions(
        IReadOnlySet<TenantFeature> enabledFeatures,
        IReadOnlySet<string>? callerPermissions = null,
        bool forDispatchAgent = false) =>
        [.. Tools
            .Where(t => !forDispatchAgent || t.DispatchAgent)
            .Where(t => t.RequiredFeature is null || enabledFeatures.Contains(t.RequiredFeature.Value))
            .Where(t => callerPermissions is null
                || t.RequiredPermission is null
                || callerPermissions.Contains(t.RequiredPermission))];

    public IReadOnlyList<AIDispatchToolDefinition> GetAllToolDefinitions() => Tools;

    public AIDispatchToolDefinition? TryGetDefinition(string name) =>
        ToolsByName.GetValueOrDefault(name);

    private static JsonNode BuildSchema(JsonObject schema) => schema;

    private static JsonObject Prop(string type, string description) =>
        new() { ["type"] = type, ["description"] = description };
}
