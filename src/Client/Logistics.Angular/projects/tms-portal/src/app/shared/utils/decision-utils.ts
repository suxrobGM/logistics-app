import type { AgentDecisionType } from "@logistics/shared/api";
import type { IconName } from "@logistics/shared/ui";

/** Parsed tool input for display in decision cards and timeline */
export interface ParsedToolInput {
  loadId?: string;
  truckId?: string;
  tripId?: string;
  reasoning?: string;
  driverId?: string;
  distanceKm?: number;
  loadIds?: string[];
  tripName?: string;
}

/** Parsed tool output for display */
export interface ParsedToolOutput {
  success?: boolean;
  error?: string;
  feasible?: boolean;
  reason?: string;
  estimatedDrivingMinutes?: number;
  drivingMinutesRemaining?: number;
  totalTrucks?: number;
  availableTrucks?: number;
  unassignedLoads?: number;
  activeTrips?: number;
  driversInViolation?: number;
  loads?: {
    id: string;
    name: string;
    type: string;
    origin: string;
    destination: string;
    distanceKm: number;
    deliveryCost: number;
    customer: string;
  }[];
  trucks?: {
    id: string;
    number: string;
    type: string;
    currentAddress: string;
    mainDriver?: {
      id: string;
      name: string;
      hos?: {
        drivingMinutesRemaining: number;
        onDutyMinutesRemaining: number;
        isInViolation: boolean;
        isAvailable: boolean;
      };
    };
  }[];
  batchResults?: {
    driver_id: string;
    distance_km: number;
    feasible: boolean;
    estimated_driving_minutes: number;
    driving_minutes_remaining: number | null;
    on_duty_minutes_remaining: number | null;
    reason: string;
  }[];
}

export function parseToolInput(json: string | null | undefined): ParsedToolInput {
  if (!json) return {};
  try {
    const parsed = JSON.parse(json);
    return {
      loadId: parsed.load_id,
      truckId: parsed.truck_id,
      tripId: parsed.trip_id,
      reasoning: parsed.reasoning,
      driverId: parsed.driver_id,
      distanceKm: parsed.distance_km,
      loadIds: parsed.load_ids,
      tripName: parsed.name,
    };
  } catch {
    return {};
  }
}

export function parseToolOutput(json: string | null | undefined): ParsedToolOutput {
  if (!json) return {};
  try {
    const parsed = JSON.parse(json);
    // get_available_trucks now includes fleet_summary
    const summary = parsed.fleet_summary;
    return {
      success: parsed.success,
      error: parsed.error,
      feasible: parsed.feasible,
      reason: parsed.reason,
      estimatedDrivingMinutes: parsed.estimated_driving_minutes,
      drivingMinutesRemaining: parsed.driving_minutes_remaining,
      totalTrucks: summary?.total_trucks ?? parsed.total_trucks,
      availableTrucks: summary?.available_trucks ?? parsed.available_trucks,
      unassignedLoads: summary?.unassigned_loads ?? parsed.unassigned_loads,
      activeTrips: summary?.active_trips ?? parsed.active_trips,
      driversInViolation: summary?.drivers_in_violation ?? parsed.drivers_in_violation,
      loads: parsed.loads?.map(parseLoad),
      trucks: parsed.trucks?.map(parseTruck),
      batchResults: parsed.results,
    };
  } catch {
    return {};
  }
}

/* eslint-disable @typescript-eslint/no-explicit-any -- raw tool JSON has no generated types */
function parseLoad(l: any): NonNullable<ParsedToolOutput["loads"]>[number] {
  return {
    id: l.id,
    name: l.name,
    type: l.type,
    origin: l.origin,
    destination: l.destination,
    distanceKm: l.distance_km,
    deliveryCost: l.delivery_cost,
    customer: l.customer,
  };
}

function parseTruck(t: any): NonNullable<ParsedToolOutput["trucks"]>[number] {
  return {
    id: t.id,
    number: t.number,
    type: t.type,
    currentAddress: t.current_address,
    mainDriver: t.main_driver
      ? {
          id: t.main_driver.id,
          name: t.main_driver.name,
          hos: t.main_driver.hos
            ? {
                drivingMinutesRemaining: t.main_driver.hos.driving_minutes_remaining,
                onDutyMinutesRemaining: t.main_driver.hos.on_duty_minutes_remaining,
                isInViolation: t.main_driver.hos.is_in_violation,
                isAvailable: t.main_driver.hos.is_available,
              }
            : undefined,
        }
      : undefined,
  };
}
/* eslint-enable @typescript-eslint/no-explicit-any */

interface ToolMeta {
  label: string;
  icon: IconName;
}

/**
 * The display metadata for every agent tool, keyed by the backend's snake_case tool name. One
 * table rather than two parallel lookups - the label and icon drifted apart when they were
 * separate. Whether a tool writes is NOT here: that is the decision's own `type`.
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

  assign_load_to_truck: { label: "Assign Load", icon: "link" },
  create_trip: { label: "Create Trip", icon: "circle-plus" },
  dispatch_trip: { label: "Dispatch Trip", icon: "send" },
  book_loadboard_load: { label: "Book Load", icon: "shopping-cart" },
  create_load_invoice: { label: "Create Invoice", icon: "file-text" },
  send_invoice: { label: "Send Invoice", icon: "mail" },
  create_payment_link: { label: "Create Payment Link", icon: "credit-card" },
};

export function getToolLabel(toolName: string | null | undefined): string {
  return (toolName ? TOOL_META[toolName]?.label : null) ?? toolName ?? "Unknown";
}

export function getToolIcon(toolName: string | null | undefined): IconName {
  return (toolName ? TOOL_META[toolName]?.icon : null) ?? "circle";
}

/** A decision's resolved references, as rendered by the cards and the confirm dialog. */
export interface DecisionRefs {
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
 * Read tools execute immediately; write tools become decisions awaiting approval. The backend
 * derives this the same way (`AgentToolDefinition.IsWrite` is `DecisionType != Query`), so reading
 * the decision's own type keeps a newly added tool from silently rendering as a read.
 */
export function isWriteDecision(decision: { type?: AgentDecisionType }): boolean {
  return !!decision.type && decision.type !== "query";
}

export function getToolMarkerClass(decision: { type?: AgentDecisionType }): string {
  return isWriteDecision(decision)
    ? "bg-primary text-primary-foreground"
    : "bg-muted text-muted-foreground";
}
