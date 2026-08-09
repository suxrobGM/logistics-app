import type {
  AgentDecisionDto,
  AgentToolFleetSummaryDto,
  AgentToolLoadDto,
} from "@logistics/shared/api";
import { groupTurnEntries, readGroupSummary } from "./dispatch-turn-timeline.utils";

function decision(overrides: Partial<AgentDecisionDto> = {}): AgentDecisionDto {
  return { id: "d1", toolName: "get_unassigned_loads", type: "query", ...overrides };
}

function load(): AgentToolLoadDto {
  return {};
}

function fleet(availableTrucks: number, totalTrucks: number): AgentToolFleetSummaryDto {
  return { availableTrucks, totalTrucks, activeTrips: 0, driversInViolation: 0 };
}

describe("groupTurnEntries", () => {
  it("merges consecutive read tools into one reads group", () => {
    const decisions = [
      decision({ id: "d1", toolName: "get_unassigned_loads" }),
      decision({ id: "d2", toolName: "get_available_trucks" }),
      decision({ id: "d3", toolName: "get_driver_hos_status" }),
    ];

    const entries = groupTurnEntries(decisions);

    expect(entries).toHaveLength(1);
    expect(entries[0].kind).toBe("reads");
    expect(entries[0].kind === "reads" && entries[0].decisions).toEqual(decisions);
  });

  it("keeps write tools standalone, never merged with a read group", () => {
    const decisions = [
      decision({ id: "d1", toolName: "get_unassigned_loads" }),
      decision({ id: "d2", toolName: "assign_load_to_truck", type: "assign_load" }),
    ];

    const entries = groupTurnEntries(decisions);

    expect(entries).toHaveLength(2);
    expect(entries[0]).toEqual({
      kind: "reads",
      id: "d1",
      decisions: [decisions[0]],
      summary: readGroupSummary([decisions[0]]),
    });
    expect(entries[1]).toEqual({ kind: "write", id: "d2", decision: decisions[1] });
  });

  it("keeps consecutive write tools as separate standalone entries, not merged", () => {
    const decisions = [
      decision({ id: "d1", toolName: "assign_load_to_truck", type: "assign_load" }),
      decision({ id: "d2", toolName: "dispatch_trip", type: "dispatch_trip" }),
    ];

    const entries = groupTurnEntries(decisions);

    expect(entries).toEqual([
      { kind: "write", id: "d1", decision: decisions[0] },
      { kind: "write", id: "d2", decision: decisions[1] },
    ]);
  });

  it("alternating reads/writes/reads produces a fresh group after each write", () => {
    const decisions = [
      decision({ id: "d1", toolName: "get_unassigned_loads" }),
      decision({ id: "d2", toolName: "get_available_trucks" }),
      decision({ id: "d3", toolName: "assign_load_to_truck", type: "assign_load" }),
      decision({ id: "d4", toolName: "get_driver_hos_status" }),
      decision({ id: "d5", toolName: "dispatch_trip", type: "dispatch_trip" }),
    ];

    const entries = groupTurnEntries(decisions);

    expect(entries.map((e) => e.kind)).toEqual(["reads", "write", "reads", "write"]);
    expect(entries[0].kind === "reads" && entries[0].decisions).toEqual([
      decisions[0],
      decisions[1],
    ]);
    expect(entries[2].kind === "reads" && entries[2].decisions).toEqual([decisions[3]]);
  });

  it("returns an empty array for no decisions", () => {
    expect(groupTurnEntries([])).toEqual([]);
  });
});

describe("readGroupSummary", () => {
  it("dedupes repeated tool calls into one chip with an accumulated count", () => {
    const decisions = [
      decision({ id: "d1", toolName: "get_unassigned_loads", toolResult: {} }),
      decision({ id: "d2", toolName: "get_unassigned_loads", toolResult: {} }),
      decision({ id: "d3", toolName: "get_available_trucks", toolResult: {} }),
    ];

    const summary = readGroupSummary(decisions);

    expect(summary.chips).toHaveLength(2);
    const loadsChip = summary.chips.find((c) => c.label === "Unassigned Loads")!;
    expect(loadsChip.count).toBe(2);
    const trucksChip = summary.chips.find((c) => c.label === "Available Trucks & Fleet Overview")!;
    expect(trucksChip.count).toBe(1);
  });

  it("sets the chip's failed flag when any of its outputs carries an error, and only for that tool", () => {
    const decisions = [
      decision({
        id: "d1",
        toolName: "get_unassigned_loads",
        toolResult: { error: "boom" },
      }),
      decision({ id: "d2", toolName: "get_unassigned_loads", toolResult: {} }),
      decision({ id: "d3", toolName: "get_available_trucks", toolResult: {} }),
    ];

    const summary = readGroupSummary(decisions);

    const loadsChip = summary.chips.find((c) => c.label === "Unassigned Loads")!;
    const trucksChip = summary.chips.find((c) => c.label === "Available Trucks & Fleet Overview")!;
    expect(loadsChip.failed).toBe(true);
    expect(trucksChip.failed).toBe(false);
  });

  it("composes the key-figures string from the first load count and truck-availability figures seen", () => {
    const decisions = [
      decision({
        id: "d1",
        toolName: "get_unassigned_loads",
        toolResult: { loads: [load(), load(), load()] },
      }),
      decision({
        id: "d2",
        toolName: "get_available_trucks",
        toolResult: { fleetSummary: fleet(3, 5) },
      }),
    ];

    const summary = readGroupSummary(decisions);

    expect(summary.keyFigures).toBe("3 unassigned loads · 3/5 trucks available");
  });

  it("singularizes the load count for exactly one load", () => {
    const decisions = [
      decision({
        id: "d1",
        toolName: "get_unassigned_loads",
        toolResult: { loads: [load()] },
      }),
    ];

    const summary = readGroupSummary(decisions);

    expect(summary.keyFigures).toBe("1 unassigned load");
  });

  it("returns an empty key-figures string when no decision carries recognized figures", () => {
    const decisions = [decision({ id: "d1", toolName: "get_driver_hos_status", toolResult: {} })];

    const summary = readGroupSummary(decisions);

    expect(summary.keyFigures).toBe("");
  });

  it("returns empty chips and key figures for no decisions", () => {
    const summary = readGroupSummary([]);

    expect(summary.chips).toEqual([]);
    expect(summary.keyFigures).toBe("");
  });
});
