import {
  LucideArrowLeft,
  LucideChevronDown,
  LucideChevronLeft,
  LucideChevronRight,
  LucideChevronUp,
  LucideCirclePlus,
  LucideEllipsisVertical,
  LucideExternalLink,
  LucideFileText,
  LucideLogOut,
  LucidePlus,
  LucideRefreshCw,
  LucideSearch,
  LucideTrash2,
  LucideTriangleAlert,
  LucideX,
} from "@lucide/angular";

/**
 * Foundational Lucide icons that the shared lib relies on (toast, error/empty
 * states, page-header, action-menu, pdf-viewer, etc.) plus a small set of UI
 * primitives that virtually every portal needs (chevrons, x, plus, search).
 *
 * Every portal that consumes `@logistics/shared` MUST register these via
 * `provideLucideIcons(...BASE_LUCIDE_ICONS, ...PORTAL_LUCIDE_ICONS)` in its
 * `app.config.ts` — otherwise shared components render blank icons.
 */
export const BASE_LUCIDE_ICONS = [
  LucideArrowLeft,
  LucideChevronDown,
  LucideChevronLeft,
  LucideChevronRight,
  LucideChevronUp,
  LucideCirclePlus,
  LucideEllipsisVertical,
  LucideExternalLink,
  LucideFileText,
  LucideLogOut,
  LucidePlus,
  LucideRefreshCw,
  LucideSearch,
  LucideTrash2,
  LucideTriangleAlert,
  LucideX,
];
