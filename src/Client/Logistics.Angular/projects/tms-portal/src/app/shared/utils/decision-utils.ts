import type { AgentDecisionType } from "@logistics/shared/api";
import type { IconName } from "@logistics/shared/ui";

/**
 * Parsed tool input for the decision cards and timeline. Unlike the result this stays a raw string
 * on the wire - it is the model's own arguments, so there is no server type to project it through.
 */
interface ParsedToolInput {
  loadId?: string;
  truckId?: string;
  reasoning?: string;
}

export function parseToolInput(json: string | null | undefined): ParsedToolInput {
  if (!json) return {};
  try {
    const parsed = JSON.parse(json);
    return {
      loadId: parsed.load_id,
      truckId: parsed.truck_id,
      reasoning: parsed.reasoning,
    };
  } catch {
    return {};
  }
}

interface ToolMeta {
  label: string;
  icon: IconName;
}

/**
 * Display metadata per tool, keyed by the backend's snake_case name. One table because the label
 * and icon drifted apart as two. Whether a tool writes is not here - that is the decision's `type`.
 */
const TOOL_META: Record<string, ToolMeta> = {
  get_unassigned_loads: { label: "Unassigned Loads", icon: "box" },
  get_available_trucks: { label: "Available Trucks & Fleet Overview", icon: "truck" },
  get_driver_hos_status: { label: "Driver HOS Status", icon: "clock" },
  check_hos_feasibility: { label: "HOS Feasibility Check", icon: "shield" },
  batch_check_hos_feasibility: { label: "Batch HOS Feasibility Check", icon: "shield" },
  calculate_distance: { label: "Distance Calculation", icon: "map" },
  calculate_assignment_metrics: { label: "Assignment Metrics", icon: "chart-column" },
  check_broker_credit: { label: "Broker Credit Check", icon: "circle" },
  check_dispatch_eligibility: { label: "Dispatch Eligibility Check", icon: "circle" },
  preview_tax_calculation: { label: "Tax Preview", icon: "circle" },
  get_container_status: { label: "Container Status", icon: "circle" },
  get_terminal_info: { label: "Terminal Info", icon: "circle" },
  search_loadboard: { label: "Search Load Board", icon: "search" },
  search_loads: { label: "Search Loads", icon: "search" },
  get_load: { label: "Load Details", icon: "box" },
  search_customers: { label: "Customer Lookup", icon: "users" },
  get_invoices: { label: "Invoices", icon: "file-text" },
  get_invoice: { label: "Invoice Details", icon: "file-text" },
  search_expenses: { label: "Expenses", icon: "receipt" },
  get_expense_stats: { label: "Expense Stats", icon: "chart-column" },
  get_upcoming_maintenance: { label: "Upcoming Maintenance", icon: "wrench" },
  get_rate_floor: { label: "Rate Floor", icon: "chart-column" },
  get_negotiation_thread: { label: "Negotiation Thread", icon: "mail" },

  assign_load_to_truck: { label: "Assign Load", icon: "link" },
  create_trip: { label: "Create Trip", icon: "circle-plus" },
  dispatch_trip: { label: "Dispatch Trip", icon: "send" },
  book_loadboard_load: { label: "Book Load", icon: "shopping-cart" },
  create_load_invoice: { label: "Create Invoice", icon: "file-text" },
  send_invoice: { label: "Send Invoice", icon: "mail" },
  create_payment_link: { label: "Create Payment Link", icon: "credit-card" },
  propose_counter_offer: { label: "Send Counter-Offer", icon: "mail" },
};

export function getToolLabel(toolName: string | null | undefined): string {
  return (toolName ? TOOL_META[toolName]?.label : null) ?? toolName ?? "Unknown";
}

export function getToolIcon(toolName: string | null | undefined): IconName {
  return (toolName ? TOOL_META[toolName]?.icon : null) ?? "circle";
}

/** A decision's resolved references, as rendered by the cards and the confirm dialog. */
interface DecisionRefs {
  load?: string;
  truck?: string;
  reasoning?: string;
}

interface DecisionRefSource {
  loadId?: string | null;
  truckId?: string | null;
  loadName?: string | null;
  truckNumber?: string | null;
  toolInput?: string | null;
}

/**
 * Resolves a decision's load/truck labels, preferring the server-resolved name over the raw id
 * from the tool input. Single owner of that precedence rule - the dispatch card, the copilot card
 * and the confirm dialog all read it here so they cannot label the same decision differently.
 */
export function getDecisionRefs(decision: DecisionRefSource): DecisionRefs {
  const parsed = parseToolInput(decision.toolInput);
  return {
    load:
      decision.loadId || decision.loadName || parsed.loadId
        ? (decision.loadName ?? parsed.loadId)
        : undefined,
    truck:
      decision.truckId || decision.truckNumber || parsed.truckId
        ? (decision.truckNumber ?? parsed.truckId)
        : undefined,
    reasoning: parsed.reasoning,
  };
}

/** Builds a human-readable detail string for confirmation dialogs */
export function buildDecisionDetail(
  decision: DecisionRefSource & { toolName?: string | null },
): string {
  const refs = getDecisionRefs(decision);
  const lines: string[] = [`Action: ${getToolLabel(decision.toolName)}`];

  if (refs.load) lines.push(`Load: ${refs.load}`);
  if (refs.truck) lines.push(`Truck: ${refs.truck}`);
  if (refs.reasoning) lines.push(`AI Reasoning: ${refs.reasoning}`);

  return lines.join("\n");
}

/**
 * Mirrors the backend's own derivation (`AgentToolDefinition.IsWrite` is `DecisionType != Query`),
 * so a newly added write tool cannot silently render as a read.
 */
export function isWriteDecision(decision: { type?: AgentDecisionType }): boolean {
  return !!decision.type && decision.type !== "query";
}

export function getToolMarkerClass(decision: { type?: AgentDecisionType }): string {
  return isWriteDecision(decision)
    ? "bg-primary text-primary-foreground"
    : "bg-muted text-muted-foreground";
}
