import { getDecisionRefs, isWriteTool, parseToolInput, parseToolOutput } from "./decision-utils";

describe("parseToolInput", () => {
  it("maps every snake_case field to its camelCase counterpart", () => {
    const input = JSON.stringify({
      load_id: "load-1",
      truck_id: "truck-1",
      trip_id: "trip-1",
      reasoning: "closest truck",
      driver_id: "driver-1",
      distance_km: 12.5,
      load_ids: ["load-1", "load-2"],
      name: "Trip A",
    });

    expect(parseToolInput(input)).toEqual({
      loadId: "load-1",
      truckId: "truck-1",
      tripId: "trip-1",
      reasoning: "closest truck",
      driverId: "driver-1",
      distanceKm: 12.5,
      loadIds: ["load-1", "load-2"],
      tripName: "Trip A",
    });
  });

  it("returns an empty object for null/undefined/empty input", () => {
    expect(parseToolInput(null)).toEqual({});
    expect(parseToolInput(undefined)).toEqual({});
    expect(parseToolInput("")).toEqual({});
  });

  it("returns a safe empty object for malformed JSON, rather than throwing", () => {
    expect(() => parseToolInput("{not json")).not.toThrow();
    expect(parseToolInput("{not json")).toEqual({});
  });
});

describe("parseToolOutput", () => {
  it("maps every snake_case field to its camelCase counterpart", () => {
    const output = JSON.stringify({
      success: true,
      error: "boom",
      feasible: false,
      reason: "over hours",
      estimated_driving_minutes: 30,
      driving_minutes_remaining: 45,
      total_trucks: 10,
      available_trucks: 4,
      unassigned_loads: 2,
      active_trips: 1,
      drivers_in_violation: 3,
      loads: [{ id: "l1" }],
      trucks: [{ id: "t1" }],
      results: [{ driver_id: "d1" }],
    });

    expect(parseToolOutput(output)).toEqual({
      success: true,
      error: "boom",
      feasible: false,
      reason: "over hours",
      estimatedDrivingMinutes: 30,
      drivingMinutesRemaining: 45,
      totalTrucks: 10,
      availableTrucks: 4,
      unassignedLoads: 2,
      activeTrips: 1,
      driversInViolation: 3,
      loads: [{ id: "l1" }],
      trucks: [{ id: "t1" }],
      batchResults: [{ driver_id: "d1" }],
    });
  });

  it("unwraps fleet_summary, preferring it over top-level fields of the same meaning", () => {
    const output = JSON.stringify({
      total_trucks: 999,
      available_trucks: 999,
      fleet_summary: {
        total_trucks: 10,
        available_trucks: 4,
        unassigned_loads: 2,
        active_trips: 1,
        drivers_in_violation: 0,
      },
    });

    const parsed = parseToolOutput(output);

    expect(parsed.totalTrucks).toBe(10);
    expect(parsed.availableTrucks).toBe(4);
    expect(parsed.unassignedLoads).toBe(2);
    expect(parsed.activeTrips).toBe(1);
    expect(parsed.driversInViolation).toBe(0);
  });

  it("falls back to the top-level fleet fields when fleet_summary is absent", () => {
    const output = JSON.stringify({ total_trucks: 10, available_trucks: 4 });

    const parsed = parseToolOutput(output);

    expect(parsed.totalTrucks).toBe(10);
    expect(parsed.availableTrucks).toBe(4);
  });

  it("returns an empty object for null/undefined/empty input", () => {
    expect(parseToolOutput(null)).toEqual({});
    expect(parseToolOutput(undefined)).toEqual({});
    expect(parseToolOutput("")).toEqual({});
  });

  it("returns a safe empty object for malformed JSON, rather than throwing", () => {
    expect(() => parseToolOutput("not json at all")).not.toThrow();
    expect(parseToolOutput("not json at all")).toEqual({});
  });
});

describe("getDecisionRefs", () => {
  it("prefers the server-resolved load/truck name over the raw tool-input id", () => {
    const refs = getDecisionRefs({
      loadId: "load-1",
      loadName: "ACME Load #42",
      truckId: "truck-1",
      truckNumber: "TRK-7",
      toolInput: JSON.stringify({ load_id: "load-1", truck_id: "truck-1" }),
    });

    expect(refs.load).toBe("ACME Load #42");
    expect(refs.truck).toBe("TRK-7");
  });

  it("falls back to the raw tool-input id when no name was resolved", () => {
    const refs = getDecisionRefs({
      toolInput: JSON.stringify({ load_id: "load-raw", truck_id: "truck-raw" }),
    });

    expect(refs.load).toBe("load-raw");
    expect(refs.truck).toBe("truck-raw");
  });

  it("has nothing to show when only the bare id is present with no name and no tool input", () => {
    // Pins current behaviour: the guard treats `loadId`/`truckId` alone as "there is a ref", but the
    // returned value only ever reads `loadName ?? parsed.loadId` - never `decision.loadId` itself. A
    // raw id with no resolvable name therefore renders as `undefined`, not the id.
    const refs = getDecisionRefs({ loadId: "load-1", truckId: "truck-1" });

    expect(refs.load).toBeUndefined();
    expect(refs.truck).toBeUndefined();
  });

  it("carries reasoning straight from the parsed tool input", () => {
    const refs = getDecisionRefs({
      toolInput: JSON.stringify({ reasoning: "closest available truck" }),
    });

    expect(refs.reasoning).toBe("closest available truck");
  });

  it("returns all-undefined refs when nothing is present", () => {
    expect(getDecisionRefs({})).toEqual({
      load: undefined,
      truck: undefined,
      reasoning: undefined,
    });
  });
});

describe("isWriteTool", () => {
  it("is true for known write tools", () => {
    expect(isWriteTool("assign_load_to_truck")).toBe(true);
    expect(isWriteTool("create_trip")).toBe(true);
    expect(isWriteTool("dispatch_trip")).toBe(true);
    expect(isWriteTool("book_loadboard_load")).toBe(true);
    expect(isWriteTool("create_load_invoice")).toBe(true);
    expect(isWriteTool("send_invoice")).toBe(true);
    expect(isWriteTool("create_payment_link")).toBe(true);
  });

  it("is false for known read tools", () => {
    expect(isWriteTool("get_unassigned_loads")).toBe(false);
    expect(isWriteTool("get_available_trucks")).toBe(false);
  });

  it("is false for an unknown or missing tool name", () => {
    expect(isWriteTool("some_future_tool")).toBe(false);
    expect(isWriteTool(null)).toBe(false);
    expect(isWriteTool(undefined)).toBe(false);
  });
});
