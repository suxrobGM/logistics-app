// Truck geolocation type for map visualization (custom type, not generated)
import type { Address, GeoPoint, PaginationMeta } from "../generated/models";

/**
 * Re-export all models from generated code for backwards compatibility.
 * Import from '@logistics/shared/api' or '@logistics/shared/api/models' - both work.
 */
export * from "../generated/models";
export * from "./enums";

// Backwards-compatible type aliases for renamed types
export type { Address as AddressDto } from "../generated/models/address";
export type { GeoPoint as GeoPointDto } from "../generated/models/geo-point";

// Re-export common query types used by consumers
export interface SearchableQuery {
  search?: string;
  orderBy?: string;
  page?: number;
  pageSize?: number;
}

export interface PagedIntervalQuery extends SearchableQuery {
  startDate?: Date | string;
  endDate?: Date | string;
}

/**
 * Generic PagedResponse type for paginated list endpoints.
 * The generated code uses specific types like LoadDtoPagedResponse, CustomerDtoPagedResponse.
 * Properties are optional to match the generated OpenAPI types.
 */
export interface PagedResponse<T> {
  items?: T[] | null;
  pagination?: PaginationMeta;
}

/**
 * Error response format from the API.
 */
export interface ErrorResponse {
  error: string;
  details?: Record<string, string[]>;
}

export interface TruckGeolocationDto {
  truckId?: string | null;
  truckNumber?: string | null;
  driversName?: string | null;
  currentLocation?: GeoPoint | null;
  currentAddress?: Address | null;
}
