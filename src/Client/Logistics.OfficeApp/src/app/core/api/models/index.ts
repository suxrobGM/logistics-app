/**
 * Re-export all models from generated code for backwards compatibility.
 * Import from '@/core/api' or '@/core/api/models' - both work.
 *
 * Enum types are exported both as types and as const objects.
 * This allows using them both in type positions (e.g., status: SalaryType)
 * and as values (e.g., SalaryType.None).
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
 * Generic Result type for backwards compatibility.
 * The generated code uses specific types like LoadDtoResult, CustomerDtoResult.
 */
export interface Result<T = void> {
  data?: T | null;
  success?: boolean;
  error?: string | null;
}

/**
 * Generic PagedResult type for backwards compatibility.
 * The generated code uses specific types like LoadDtoPagedResult, CustomerDtoPagedResult.
 */
export interface PagedResult<T> {
  data?: T[] | null;
  totalItems?: number;
  totalPages?: number;
  success?: boolean;
  error?: string | null;
}

// Truck geolocation type for map visualization (custom type, not generated)
import type { Address } from "../generated/models/address";
import type { GeoPoint } from "../generated/models/geo-point";

export interface TruckGeolocationDto {
  truckId?: string | null;
  truckNumber?: string | null;
  driversName?: string | null;
  currentLocation?: GeoPoint | null;
  currentAddress?: Address | null;
}
