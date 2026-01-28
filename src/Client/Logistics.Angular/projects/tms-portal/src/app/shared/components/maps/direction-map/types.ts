import type { GeoPoint } from "@logistics/shared/api";
import type { Feature, FeatureCollection, LineString, Point } from "geojson";

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
  origin: GeoPoint;
  destination: GeoPoint;
  /** Total distance in meters */
  distance: number;
  /** Total duration in seconds */
  duration: number;
  /** Per-segment breakdown */
  segments: RouteSegmentInfo[];
}

/**
 * Information about a single route segment.
 */
export interface RouteSegmentInfo {
  index: number;
  /** Distance in meters */
  distance: number;
  /** Duration in seconds */
  duration: number;
  fromWaypointId: string;
  toWaypointId: string;
}

/**
 * Represents a waypoint on the map.
 */
export interface Waypoint {
  id: string;
  location: GeoPoint;
}

interface WaypointProps {
  id: string;
  label: string;
}

export type SegmentFeature = Feature<LineString, { distance: number; duration: number }>;
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
