import { getDecisionRefs, isWriteDecision, parseToolInput } from "./decision-utils";

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

describe("isWriteDecision", () => {
  it("is true for every non-query decision type", () => {
    expect(isWriteDecision({ type: "assign_load" })).toBe(true);
    expect(isWriteDecision({ type: "create_trip" })).toBe(true);
    expect(isWriteDecision({ type: "dispatch_trip" })).toBe(true);
    expect(isWriteDecision({ type: "book_load_board_load" })).toBe(true);
    expect(isWriteDecision({ type: "create_invoice" })).toBe(true);
    expect(isWriteDecision({ type: "send_invoice" })).toBe(true);
    expect(isWriteDecision({ type: "create_payment_link" })).toBe(true);
  });

  it("is false for a query decision", () => {
    expect(isWriteDecision({ type: "query" })).toBe(false);
  });

  it("is false when the server sent no type", () => {
    expect(isWriteDecision({})).toBe(false);
  });
});
