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
    return {
      success: parsed.success,
      error: parsed.error,
      feasible: parsed.feasible,
      reason: parsed.reason,
      estimatedDrivingMinutes: parsed.estimated_driving_minutes,
      drivingMinutesRemaining: parsed.driving_minutes_remaining,
      totalTrucks: parsed.total_trucks,
      availableTrucks: parsed.available_trucks,
      unassignedLoads: parsed.unassigned_loads,
      activeTrips: parsed.active_trips,
      driversInViolation: parsed.drivers_in_violation,
      loads: parsed.loads,
      trucks: parsed.trucks,
    };
  } catch {
    return {};
  }
}

export function getToolLabel(toolName: string | null | undefined): string {
  switch (toolName) {
    case "get_fleet_overview":
      return "Fleet Overview";
    case "get_unassigned_loads":
      return "Unassigned Loads";
    case "get_available_trucks":
      return "Available Trucks";
    case "get_driver_hos_status":
      return "Driver HOS Status";
    case "check_hos_feasibility":
      return "HOS Feasibility Check";
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
    case "get_fleet_overview":
      return "pi pi-chart-bar";
    case "get_unassigned_loads":
      return "pi pi-box";
    case "get_available_trucks":
      return "pi pi-truck";
    case "get_driver_hos_status":
      return "pi pi-clock";
    case "check_hos_feasibility":
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
