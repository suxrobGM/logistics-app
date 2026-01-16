// Re-export everything from generated API layer
export * from "./generated";
export * from "./api.provider";

/**
 * Utility function to format sort field for API queries.
 * @param sortField Sort field name
 * @param sortOrder Sort order (1 for ascending, -1 for descending)
 * @returns The formatted sort field string (prefixed with - for descending)
 */
export function formatSortField(sortField?: string | null, sortOrder?: number | null): string {
  if (!sortField) return "";
  if (!sortOrder) sortOrder = 1;
  return sortOrder <= -1 ? `-${sortField}` : sortField;
}
