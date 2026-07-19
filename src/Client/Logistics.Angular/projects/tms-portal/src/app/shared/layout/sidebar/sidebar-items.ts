import type { NavSection } from "@/shared/layout/nav-menu";
import { dispatchNav } from "./nav/dispatch.nav";
import { financeNav } from "./nav/finance.nav";
import { fleetNav } from "./nav/fleet.nav";
import { mainNav } from "./nav/main.nav";
import { peopleNav } from "./nav/people.nav";
import { systemNav } from "./nav/system.nav";

// Array order = rendered order (top to bottom in the sidebar); `pinToBottom` sections
// (e.g. system) are pulled out separately by `NavMenu`, but the source order still matters
// for the command palette + favorites index.
export const sidebarSections: NavSection[] = [
  mainNav,
  dispatchNav,
  fleetNav,
  financeNav,
  peopleNav,
  systemNav,
];
