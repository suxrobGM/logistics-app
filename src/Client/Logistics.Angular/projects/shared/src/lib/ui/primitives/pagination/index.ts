import { HlmPagination } from "./lib/hlm-pagination";
import { HlmPaginationContent } from "./lib/hlm-pagination-content";
import { HlmPaginationItem } from "./lib/hlm-pagination-item";

export * from "./lib/hlm-pagination";
export * from "./lib/hlm-pagination-content";
export * from "./lib/hlm-pagination-item";

export const HlmPaginationImports = [
  HlmPagination,
  HlmPaginationContent,
  HlmPaginationItem,
] as const;
