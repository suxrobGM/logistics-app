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
      loads: parsed.loads,
      trucks: parsed.trucks,
      batchResults: parsed.results,
    };
  } catch {
    return {};
  }
}

export function getToolLabel(toolName: string | null | undefined): string {
  switch (toolName) {
    case "get_unassigned_loads":
      return "Unassigned Loads";
    case "get_available_trucks":
      return "Available Trucks & Fleet Overview";
    case "get_driver_hos_status":
      return "Driver HOS Status";
    case "check_hos_feasibility":
      return "HOS Feasibility Check";
    case "batch_check_hos_feasibility":
      return "Batch HOS Feasibility Check";
    case "calculate_distance":
      return "Distance Calculation";
    case "assign_load_to_truck":
      return "Assign Load";
    case "create_trip":
      return "Create Trip";
    case "dispatch_trip":
      return "Dispatch Trip";
    case "search_load_board":
      return "Search Load Board";
    case "book_load_board_load":
      return "Book Load";
    default:
      return toolName ?? "Unknown";
  }
}

export function getToolIcon(toolName: string | null | undefined): string {
  switch (toolName) {
    case "get_unassigned_loads":
      return "pi pi-box";
    case "get_available_trucks":
      return "pi pi-truck";
    case "get_driver_hos_status":
      return "pi pi-clock";
    case "check_hos_feasibility":
    case "batch_check_hos_feasibility":
      return "pi pi-shield";
    case "calculate_distance":
      return "pi pi-map";
    case "assign_load_to_truck":
      return "pi pi-link";
    case "create_trip":
      return "pi pi-plus-circle";
    case "dispatch_trip":
      return "pi pi-send";
    case "search_load_board":
      return "pi pi-search";
    case "book_load_board_load":
      return "pi pi-shopping-cart";
    default:
      return "pi pi-circle";
  }
}

/** Builds a human-readable detail string for confirmation dialogs */
export function buildDecisionDetail(decision: {
  toolName?: string | null;
  loadName?: string | null;
  truckNumber?: string | null;
  reasoning?: string | null;
  toolInput?: string | null;
}): string {
  const parsed = parseToolInput(decision.toolInput);
  const lines: string[] = [];

  const action = getToolLabel(decision.toolName);
  lines.push(`Action: ${action}`);

  if (decision.loadName || parsed.loadId) {
    lines.push(`Load: ${decision.loadName ?? parsed.loadId}`);
  }
  if (decision.truckNumber || parsed.truckId) {
    lines.push(`Truck: ${decision.truckNumber ?? parsed.truckId}`);
  }
  if (parsed.reasoning) {
    lines.push(`AI Reasoning: ${parsed.reasoning}`);
  }

  return lines.join("\n");
}

export function isWriteTool(toolName: string | null | undefined): boolean {
  return ["assign_load_to_truck", "create_trip", "dispatch_trip", "book_load_board_load"].includes(
    toolName ?? "",
  );
}

export function getToolMarkerClass(toolName: string | null | undefined): string {
  if (isWriteTool(toolName)) {
    return "bg-blue-500 text-white";
  }
  return "bg-surface-400 text-white";
}
