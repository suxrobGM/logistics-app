import {
  LucideBot,
  LucideBuilding,
  LucideCreditCard,
  LucideFilePenLine,
  LucideHouse,
  LucideInbox,
  LucideLayoutGrid,
  LucideMail,
  LucideShieldCheck,
  LucideUser,
  LucideUsers,
} from "@lucide/angular";

/**
 * Admin-portal-specific Lucide icons. Foundational icons (chevrons, plus, x,
 * arrows, log-out, etc.) come from `BASE_LUCIDE_ICONS` in `@logistics/shared`.
 *
 * Every icon referenced by `sidebar-items.ts` must be registered here, or the
 * sidebar renders a blank space instead of the glyph.
 */
export const ADMIN_LUCIDE_ICONS = [
  LucideBot,
  LucideBuilding,
  LucideCreditCard,
  LucideFilePenLine,
  LucideHouse,
  LucideInbox,
  LucideLayoutGrid,
  LucideMail,
  LucideShieldCheck,
  LucideUser,
  LucideUsers,
];
