# TMS UI simplification

Goal: the TMS portal felt overwhelming. Fewer places to go, fewer controls per screen.

## Phase 1: navigation

- [x] Sidebar groups become plain links to their first accessible child; children render as page tabs (`app-section-tabs`)
- [x] Remove the expandable group and collapsed-rail flyout code from `nav-menu`
- [x] DVIR moves under Safety; the orphaned load board dashboard becomes its Overview tab
- [x] Settings tab bar reuses `app-section-tabs` (fixes its active tab never highlighting)
- [x] Header buttons that only linked to a sibling tab removed

## Phase 2: home dashboard

- [ ] Panels size to content: no nested scrollbars, KPI cards never scroll
- [ ] Header: one "Customize" menu (add/remove panels, reset layout) plus Quick Actions
- [ ] KPI empty state instead of `$0.00`; body font, not monospace
- [ ] Needs Attention hidden when empty; onboarding shows remaining steps only
- [ ] Active Loads: distinct status colours, Title Case, no progress column

## Phase 3: page template

- [ ] One primary action per page header, the rest in an overflow menu
- [ ] Drop Back buttons that duplicate breadcrumbs
- [ ] Load details: hide empty sections, money in its own tab
- [ ] Other detail pages, one per commit
