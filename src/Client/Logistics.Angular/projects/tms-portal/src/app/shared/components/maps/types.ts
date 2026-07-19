/**
 * Shared types for map components.
 */

/** Available map layer/style types */
export type MapLayerType = "streets" | "satellite" | "terrain";

/** Map control position options */
export type MapControlPosition = "top-left" | "top-right" | "bottom-left" | "bottom-right";

/** Route information with distance and duration */
export interface RouteInfo {
  /** Distance in meters */
  distance: number;
  /** Duration in seconds */
  duration: number;
  /** Distance in miles */
  distanceMiles: number;
  /** Human-readable duration string */
  durationFormatted: string;
}

/** Map bounds defined by southwest and northeast corners */
export interface MapBounds {
  sw: [number, number];
  ne: [number, number];
}

/** Segment information for route display */
export interface SegmentInfo {
  index: number;
  /** Distance in meters */
  distance: number;
  /** Duration in seconds */
  duration: number;
  fromLabel: string;
  toLabel: string;
}

/** Chrome (non-data) display options shared by the top-level map components. */
export interface MapChrome {
  /** Width of the map container. Default is 100%. */
  width?: string;
  /** Show map controls. Default is true. */
  showControls?: boolean;
  /** Show layer toggle in controls. Default is true. */
  showLayerToggle?: boolean;
}

/** Default values applied to any `MapChrome` fields left unset by the caller. */
export const DEFAULT_MAP_CHROME: Required<MapChrome> = {
  width: "100%",
  showControls: true,
  showLayerToggle: true,
};

/**
 * Formats a distance in meters to miles.
 */
export function formatDistanceMiles(meters: number): number {
  return Math.round((meters / 1609.344) * 10) / 10;
}

/**
 * Formats a duration in seconds to a human-readable string.
 */
export function formatDuration(seconds: number): string {
  const hours = Math.floor(seconds / 3600);
  const minutes = Math.round((seconds % 3600) / 60);

  if (hours > 0) {
    return `${hours}h ${minutes}m`;
  }
  return `${minutes} min`;
}
