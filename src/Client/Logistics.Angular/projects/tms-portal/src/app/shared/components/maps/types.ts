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
