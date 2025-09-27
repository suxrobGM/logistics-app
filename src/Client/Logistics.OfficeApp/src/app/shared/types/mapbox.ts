/**
 * Mapbox Geocoding API response
 */
export interface MapboxGeocodingResponse {
  attribution: string;
  type: string;
  features: MapboxGeocodingFeature[];
}

export interface MapboxGeocodingFeature {
  id: string;
  type: string;
  geometry: { type: string; coordinates: GeoPoint };
  properties: GeocodingFeatureProperties;
}

interface GeocodingFeatureProperties {
  feature_type: string;
  full_address: string;
  name: string;
  context: GeocodingFeatureContext;
  coordinates: { longitude: number; latitude: number; accuracy: string };
}

interface GeocodingFeatureContext {
  address: { name: string; street_name: string; address_number: string };
  country: { name: string; country_code: string; country_code_alpha_3: string };
  district: { name: string };
  place: { name: string; alternate: { name: string }[] };
  postcode: { name: string };
  region: { name: string; region_code: string };
  street: { name: string };
}

/**
 * Mapbox Directions API response
 */
export interface MapboxDirectionsResponse {
  code: string;
  routes: MapboxDirectionsRoute[];
}

/**
 * Represents a route returned by the Mapbox Directions API.
 * Contains information about the route's duration, distance, and geometry.
 */
export interface MapboxDirectionsRoute {
  duration: number;
  distance: number;
  weight_name: string;
  weight: number;
  duration_typical: number;
  weight_typical: number;
  geometry: GeoJSON.Geometry;
}

/**
 * Represents a geographical point with longitude and latitude.
 * The order is [longitude, latitude] as per GeoJSON standards.
 */
export type GeoPoint = [number, number];
