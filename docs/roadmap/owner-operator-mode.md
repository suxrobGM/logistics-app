# Owner-Operator (Solo) Mode

- **Status**: Planned
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
- **AI as the back office**: default the agent to a solo-tuned prompt - find load-board loads for MY truck near MY current location honoring MY HOS (`SearchLoadBoardTool` + `GetDriverHosTool` already exist). Pairs directly with [ai-rate-negotiation](ai-rate-negotiation.md) (negotiates while they drive) and [ai-voice-driver-assistant](ai-voice-driver-assistant.md) (hands-free operation - highest-value user for it).
- **Owner pay, not payroll**: settlements/draws to self via the existing employee Stripe Connect payout rails instead of `PayrollInvoice` cycles; [same-day-pay](same-day-pay.md) matters most to exactly this user.
- **Pricing/marketing**: "from $41/mo, AI dispatcher included" targeting owner-operators - cross-link [pricing-launch-polish](pricing-launch-polish.md). Consider a named "Owner-Operator" plan alias of Starter (1 truck) so the segment sees itself on the pricing page.
- Growth loop: drivers at fleet tenants who go independent already know the driver app - make "start your own authority" a first-class conversion path.

## Acceptance

A one-person carrier can sign up, get a load suggested by the AI from a load board, drive it using the driver app, invoice with one tap, and receive payment - without ever seeing an empty "Employees" or "Payroll" screen or touching a desktop.

## Notes

_(add dated implementation notes here)_
