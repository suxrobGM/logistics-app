using System.Text.Json.Nodes;
using Logistics.Application.Services;

namespace Logistics.Infrastructure.AI.Services;

internal sealed class DispatchToolRegistry : IDispatchToolRegistry
{
    private static readonly HashSet<string> LoadBoardTools =
        ["search_load_board", "book_load_board_load"];

    private static readonly List<DispatchToolDefinition> Tools =
    [
        // ── Read Tools ──

        new("get_unassigned_loads",
            "Get all Draft loads that are not assigned to any trip. Returns load ID, name, type, origin, destination, distance, delivery cost, and customer.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject(),
                ["required"] = new JsonArray()
            })),

        new("get_available_trucks",
            "Get all trucks with Available status along with their driver info and HOS (Hours of Service) status. Returns truck ID, number, type, current location, driver name, and remaining driving/on-duty hours.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject(),
                ["required"] = new JsonArray()
            })),

        new("get_fleet_overview",
            "Get a high-level summary of the fleet: total trucks, available trucks, unassigned loads, active trips, drivers in violation. Use this first to understand the current state before making decisions.",
            BuildSchema(new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject(),
                ["required"] = new JsonArray()
            })),

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
            })),

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
            })),

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
            })),

        new("search_load_board",
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
            })),

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
            })),

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
            })),

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
            })),

        new("book_load_board_load",
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
            }))
    ];

    public IReadOnlyList<DispatchToolDefinition> GetToolDefinitions(bool includeLoadBoardTools = false)
    {
        if (includeLoadBoardTools)
            return Tools;

        return Tools.Where(t => !LoadBoardTools.Contains(t.Name)).ToList();
    }

    private static JsonNode BuildSchema(JsonObject schema) => schema;

    private static JsonObject Prop(string type, string description) =>
        new() { ["type"] = type, ["description"] = description };
}
