import type { LoadType } from "../api/generated/models/load-type";

/**
 * True when the load type is one of the intermodal container variants.
 * Single source of truth — mirrors the server-side
 * `CargoInspectionPartCategoryExtensions.IsContainerLoad(LoadType)`.
 */
export function isContainerLoadType(loadType: LoadType | undefined | null): boolean {
  return (
    loadType === "intermodal_container" ||
    loadType === "tank_container" ||
    loadType === "reefer_container"
  );
}
