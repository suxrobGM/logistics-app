# TMS UI simplification

Goal: the TMS portal felt overwhelming. Fewer places to go, fewer controls per screen.

## Phase 1: navigation

- [x] Sidebar groups become plain links to their first accessible child; children render as page tabs (`app-section-tabs`)
- [x] Remove the expandable group and collapsed-rail flyout code from `nav-menu`
- [x] DVIR moves under Safety; the orphaned load board dashboard becomes its Overview tab
- [x] Settings tab bar reuses `app-section-tabs` (fixes its active tab never highlighting)
- [x] Header buttons that only linked to a sibling tab removed

## Phase 2: home dashboard

- [x] Gridster removed: KPI row, then main and side columns sized to content (user chose dropping drag/resize)
- [x] Header: Refresh, one Customize menu (show/hide panels, reset) and Quick Actions
- [x] KPI empty state instead of `$0.00`; tabular numbers, not monospace
- [x] Needs Attention hidden when empty, zero rows dropped; onboarding lists remaining steps only
- [x] Active Loads: 6 rows max, no progress column; badges Title Case, picked up has its own colour

## Accent styles (user request)

- [x] No left-edge stripes: nav active item, unread notifications, load table rows
- [x] Coloured icon circles neutral (`bg-muted`) in all three portals; colour kept only for state
- [x] Stat card: no gradient ring or glow; `valueTone` only for values that carry state

## Phase 3: page template

- [x] Settings no longer pinned to the sidebar bottom (user request); `pinToBottom` removed
- [x] Load details: next step (Assign Truck or Dispatch) + Edit + overflow menu; money in a Charges tab; Timeline and duplicate rows removed; Customer Updates only when present
- [x] Header Back buttons become `backLink` arrows (loads, customers, employees, trucks, factoring schedules)
- [x] Headers audited: the rest show at most two buttons or state-exclusive pairs (Approve/Reject)
- [x] Detail pages: employee links in a header menu, customer "New Load" in the header, duplicate truck rows gone; trip already clean
- [x] List filters: `app-filter-bar` (one row + "More filters") on loads, trucks, trips, load invoices, payroll invoices
- [x] Inputs and select triggers have a solid `bg-card` fill (they read as disabled on the page background)

## Sidebar and home follow-ups (user requests)

- [x] Sidebar spacing: one 0.75rem gutter, brand inline, collapse toggle in the footer icon row
- [x] Sidebar gradient kept (user disliked it flat); borders, scrollbar, search and account raised for contrast
- [x] Needs Attention lists every waiting action (AI decisions, unassigned loads, overdue invoices, idle trucks) in a scrolling list
