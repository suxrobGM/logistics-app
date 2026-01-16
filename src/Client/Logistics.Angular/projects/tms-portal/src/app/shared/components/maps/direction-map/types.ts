import type { Feature, FeatureCollection, LineString, Point } from "geojson";
import type { GeoPointDto } from "@/core/api/models";

/**
 * Fired when a route segment is clicked.
 */
export interface RouteSegmentClickEvent {
  fromWaypoint: Waypoint;
  toWaypoint: Waypoint;
}

/**
 * Represents a waypoint click event.
 */
export interface WaypointClickEvent {
  waypoint: Waypoint;
}

/**
 * Represents a change in the route.
 */
export interface RouteChangeEvent {
  origin: GeoPointDto;
  destination: GeoPointDto;
  distance: number;
}

/**
 * Represents a waypoint on the map.
 */
export interface Waypoint {
  id: string;
  location: GeoPointDto;
}

interface WaypointProps {
  id: string;
  label: string;
}

export type SegmentFeature = Feature<LineString, { distance: number }>;
export type WaypointFeature = FeatureCollection<Point, WaypointProps>;

/**
 * Represents a segment of the route in the directions map.
 */
export interface SegmentLayer {
  layerId: string; // seg-{from}-{to}
  data: SegmentFeature; // GeoJSON line
  fromWaypoint: Waypoint;
  toWaypoint: Waypoint;
}
