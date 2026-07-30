# Owner-Operator (Solo) Mode

- **Status**: Done
- **Priority**: P1 - the largest carrier segment by count (most US carriers run 1 truck) and explicitly ignored by our competitors
- **Effort**: M
- **Category**: Market expansion

## Why

DataTruck says it's "built for fleet owners with more than 10 trucks"; Truckbase starts at ~$290/mo.
The tools owner-operators actually use (TruckLogics, Rigbooks, spreadsheets) are cheap but have no AI,
no load boards, no payments. Our Starter plan at 1 truck is $41/mo - already the right price - but the
product UX assumes a back office (dispatcher at a desk + driver in a truck). A sole operator is owner,
dispatcher, accountant, and driver in one person, usually holding a phone.

For a solo operator, the AI dispatch agent isn't an efficiency feature - it **is** their dispatcher
and back office. That's the pitch: "run your authority alone; the AI does the desk work."

## What to build

- **Solo onboarding path**: signup flow that creates the tenant + one user holding Owner + Dispatcher + Driver roles at once. First step: verify the role model (`Entities/Role/TenantRole`, `UserTenantAccess`) supports one user with multiple tenant roles cleanly - the driver app and TMS portal must both accept the same identity.
- **Solo mode UX preset**: tenant flag that hides team-oriented surfaces (payroll runs, employees, timesheets approvals, messaging-to-self) and reshapes the dashboard around one truck: this week's revenue, next load, HOS remaining, invoices outstanding.
- **Mobile-first owner functions**: a solo operator does everything from the cab. Either extend the KMP driver app (`Logistics.DriverApp`) with owner actions (accept/create load, send invoice, snap expense receipts) or make the critical TMS pages properly phone-usable. Receipt-snap → expense via the existing LLM PDF/image extraction path is a natural fit.
- **AI as the back office**: default the agent to a solo-tuned prompt - find load-board loads for MY truck near MY current location honoring MY HOS (`SearchLoadBoardTool` + `GetDriverHosStatusTool` already exist). Pairs directly with [ai-rate-negotiation](ai-rate-negotiation.md) (negotiates while they drive) and [ai-voice-driver-assistant](ai-voice-driver-assistant.md) (hands-free operation - highest-value user for it).
- **Owner pay, not payroll**: settlements/draws to self via the existing employee Stripe Connect payout rails instead of `PayrollInvoice` cycles; [same-day-pay](same-day-pay.md) matters most to exactly this user.
- **Pricing/marketing**: "from $41/mo, AI dispatcher included" targeting owner-operators - cross-link [pricing-launch-polish](pricing-launch-polish.md). Consider a named "Owner-Operator" plan alias of Starter (1 truck) so the segment sees itself on the pricing page.
- Growth loop: drivers at fleet tenants who go independent already know the driver app - make "start your own authority" a first-class conversion path.

## Acceptance

A one-person carrier can sign up, get a load suggested by the AI from a load board, drive it using the driver app, invoice with one tap, and receive payment - without ever seeing an empty "Employees" or "Payroll" screen or touching a desktop.

## Notes

**2026-07-27 - shipped.** `OperatingMode` (`Fleet` | `SoloOperator`) sits on the `TenantSettings`
complex type - column `settings_operating_mode`, default `fleet`, migration
`20260727082723_AddTenantOperatingMode`. It is pickable when an admin creates a tenant and
switchable later in Settings → Company. Decisions worth remembering:

- **Multi-role turned out to be unnecessary.** The first bullet above asked to verify that one user
  can hold Owner + Dispatcher + Driver cleanly. Wrong question: every driver-facing query keys off
  `Truck.MainDriverId` / `SecondaryDriverId`, never off the role. An Owner-role employee assigned to
  a truck already flows through loads, HOS, payroll, stats and dispatch eligibility, so the role
  model needed no change at all.
- **Owner was missing `Permission.Driver.*`.** That 403'd all four `DriverController` endpoints, so
  the KMP driver app was dead for a solo login - the one thing this feature exists to enable.
  Deploy note: the permission list is C#, but `GetCurrentUserPermissionsHandler` reads
  `TenantRoleClaim` rows, so nothing changes in a running environment until `TenantRoleSeeder`
  re-syncs them on a DbMigrator run.
- **Two pre-existing bugs surfaced in the same pass,** neither solo-specific. Drivers had no
  `Permission.Dvir` at all, so DVIR submission 403'd for every real driver. And the truck driver
  autocomplete sent `Role: "Driver"` into a case-sensitive `Contains` on `"tenant.driver"`, so it
  matched nobody. The role filter is deleted rather than corrected - nothing in the domain requires
  the Driver role to be assigned to a truck, and an owner-operator has to be able to pick themselves.
- **Solo mode is expressed as absence, not as a second UI.** The nav drops `employees`, `payroll`
  and `messages` (timesheets goes with its payroll parent), the onboarding checklist drops
  `inviteTeam`, and default sidebar favourites are filtered to ids the tenant can actually reach.
  Everything else is the fleet product. The dashboard simplification that landed alongside
  (fewer default panels, real empty states) applies to both modes.
- **The solo prompt section is `## Fleet Profile`, not `## Operating Mode`.** That heading was
  already taken by the suggestions/autonomous switch, and two same-named sections in one system
  prompt is a good way to have the model obey the wrong one.
- **A third demo tenant beats a flag on `us`.** `solo` / Rodriguez Trucking LLC seeds one employee,
  one truck, two customers and 12 loads. Seed data had been keyed by region, so a second US tenant
  would have made `UserSeeder` skip and `EmployeeSeeder` re-home every existing `us` login onto it.
  `DemoTenantConfig.SeedDataKey` (falling back to the region name) plus a `DataScale` multiplier
  fixed both, with `us`/`eu` output byte-identical.
- Deferred: **owner pay**. A solo owner still has no draw or settlement path; the plan remains the
  existing employee Stripe Connect payout rails rather than `PayrollInvoice` cycles. Also deferred:
  **net-new business-side features in the mobile app** - it still has no create-load, invoice,
  expense or receipt-capture surface. This pass made the driver app usable by a solo owner, not
  their back office. And **self-serve signup** - `POST /tenants` is still behind
  `Permission.Tenant.Manage`, so the `/owner-operators` marketing page routes to Request a Demo and
  a human provisions the tenant.
