# Features

Everything LogisticsX does today, grouped by app. Items that are planned but not built live in the [roadmap](roadmap/README.md).

## Equipment

The system is not tied to one kind of truck. These types are built in: flatbed, dry van, reefer, tanker, box truck, dump truck, tow truck, car hauler, car transporter, container truck, low loader, tautliner, swap body, and curtainsider. The same install can run a general freight company, a reefer fleet, a heavy haul outfit, a drayage operator, or a vehicle transport carrier.

## Regions

A company is set up as US or European. That choice sets the currency (USD or EUR), the units, the address validation, the country list, the map defaults, and the demo data. Europe here means the EU, the EEA and EFTA countries, the United Kingdom, the Western Balkans, Moldova, Ukraine, and the European microstates.

## TMS portal

The web app for owners, managers, and dispatchers.

### Dispatch and operations

| Feature         | What it does                                                                                                                                             |
| --------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Loads           | Create loads with pickup and delivery stops, cargo details, and instructions. Follow each load from booked to delivered.                                 |
| PDF import      | Upload a rate confirmation PDF and the system fills in the load for you. It reads the text first and falls back to an AI vision model for scanned pages. |
| Trips           | Group loads into a trip with a planned multi-stop route. The system warns when a driver or truck is double-booked. Routes come from Mapbox.              |
| Live tracking   | See every active driver on a map. Positions update over a live connection as drivers move.                                                               |
| Public tracking | Share a tracking link with a customer or receiver. No login is needed.                                                                                   |
| Fleet           | Trucks, trailers, registration and insurance dates, and who is assigned to what. Enter a VIN and the truck details fill in from the NHTSA database.      |
| Maintenance     | Schedule service, record what was done and what it cost, and see what is coming due.                                                                     |
| Customers       | Customer profiles with several contacts each, billing addresses, and shipment history.                                                                   |
| Hazmat and ADR  | Mark a load as hazmat and record ADR equipment on each truck. Dispatch refuses to send a hazmat load to a truck without the right equipment.             |
| Driver licenses | Track each driver's license classes and expiry dates. Drivers get reminders before a license expires, and dispatch will not assign an unlicensed driver. |

### Intermodal

| Feature             | What it does                                                                                                                                                         |
| ------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Container tracking  | Track a container across several loads by its ISO 6346 number, with seal number, booking reference, bill of lading, weight, and laden or empty status.               |
| Container types     | Common ISO size and type codes are built in: 20' and 40' general purpose, 40' and 45' high cube, reefers, open tops, flat racks, and 20' tanks.                      |
| Container lifecycle | A container moves through fixed stages: empty, loaded, at port, in transit, delivered, returned. Each change raises an event other parts of the system can react to. |
| Terminals           | A directory of sea ports, rail terminals, inland depots, air cargo facilities, and border crossings, each keyed by its UN/LOCODE (for example `USLAX` or `DEHAM`).   |

### Load boards

| Feature             | What it does                                                                                                                                                         |
| ------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Search              | Search DAT, Truckstop, and 123Loadboard from one screen. Filter by origin, destination, dates, equipment, and rate.                                                  |
| Book                | Book a listing and the load is created in the TMS with the details filled in.                                                                                        |
| Broker credit check | Every listing shows the broker's credit score, days to pay, and FMCSA authority status. Bookings below your minimum score are blocked unless a dispatcher overrides. |
| Post trucks         | Advertise an open truck with its location, preferred destinations, equipment, and availability window. Posts refresh on their own.                                   |

### AI

| Feature             | What it does                                                                                                                                                                                                                                         |
| ------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Dispatch agent      | A shared conversation where you ask the agent to plan the day. It proposes which truck takes which load, checks hours of service, and explains each choice.                                                                                          |
| Approvals           | Every action that changes data becomes a decision waiting for a dispatcher. Approve it or reject it with a reason. Nothing changes until someone approves.                                                                                           |
| Timeline            | Every tool call, reasoning step, and decision in a turn is written to a timeline with timestamps.                                                                                                                                                    |
| Follow-ups          | Reject a suggestion or send a follow-up message, and the agent adjusts within the same conversation.                                                                                                                                                 |
| Learned preferences | A nightly job reads the decisions you approved and rejected and writes a short policy the agent follows next time. Dispatchers can edit it.                                                                                                          |
| Rate negotiation    | When a listing pays below your lane floor, the agent drafts a counter-offer email to the broker. You approve before it sends. Replies come back into the conversation. See [broker email negotiation](broker-email-negotiation.md).                  |
| Copilot             | A chat panel on every page. Ask about loads, customers, expenses, or maintenance, or have it invoice delivered loads and send payment links. It only uses the tools your role allows, and it asks before it writes. See [AI Copilot](ai-copilot.md). |
| Tools               | Both agents share one set of 31 tools covering dispatch, load boards, negotiation, finance, intermodal, and operations.                                                                                                                              |
| MCP server          | Connect Claude Desktop, Cursor, Windsurf, and other MCP clients to your fleet. API keys are created per company, shown once, and rate limited. See [MCP Server](mcp-server.md).                                                                      |

### Money

| Feature        | What it does                                                                                                                                                           |
| -------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Invoices       | Invoice a delivered load with base rate, fuel surcharge, detention, and other line items. Three invoice kinds: load, payroll, and subscription.                        |
| Tax            | Tax is worked out by Stripe Tax or by rates you enter yourself. US sales tax and EU VAT are both handled, and invoices show the breakdown.                             |
| Payments       | Card and bank payments through Stripe. Send a payment link that expires, and accept partial payments against a balance.                                                |
| Stripe Connect | Payments go straight to the company's bank account through Stripe Connect. Onboarding uses Stripe Express.                                                             |
| Payroll        | Pay drivers by mile, by share of gross, hourly, or a fixed weekly or monthly salary. Generate payroll invoices with PDF stubs and keep a payment history per employee. |
| Timesheets     | Track hours and overtime and feed them into payroll.                                                                                                                   |
| Expenses       | Record fuel, tolls, repairs, insurance, and other costs. Break spending down by category, truck, or period.                                                            |
| Fuel cards     | WEX and EFS transactions import every night as paid fuel expenses, matched to trucks by card or unit number. Unmatched ones wait in a review queue.                    |
| QuickBooks     | Customers, invoices, payments, and expenses sync to QuickBooks Online every night after a one-time OAuth connection.                                                   |

### Compliance and safety

| Feature         | What it does                                                                                                                                                                                 |
| --------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| ELD and HOS     | Hours of service from Samsara, Motive, Geotab, and TT ELD. Duty status, driving hours, and violation alerts update through webhooks.                                                         |
| IFTA            | Quarterly fuel tax per jurisdiction. Miles come from ELD GPS, gallons from fuel cards and expenses. Closed quarters are frozen for audits. Includes filing reminders and PDF and CSV export. |
| DVIR            | Digital pre-trip and post-trip inspections with damage marked on a vehicle diagram, defects by severity, and photos. History per vehicle.                                                    |
| Accidents       | Record accident reports with the people, vehicles, and documents involved, and follow them to close.                                                                                         |
| Driver behavior | Log speeding, harsh braking, and similar events per driver.                                                                                                                                  |
| Documents       | One place for proof of delivery, bills of lading, employee records, and compliance paperwork. Files are stored in Azure Blob Storage, Cloudflare R2, or on local disk.                       |
| Privacy (GDPR)  | Customers and employees can ask for a copy of their data or for deletion. Requests are handled in the admin portal, and retention rules clean up old data on their own.                      |

### Reports

| Report      | What it shows                                                         |
| ----------- | --------------------------------------------------------------------- |
| Revenue     | Gross revenue by day, week, and month, with trends.                   |
| Financial   | Revenue against expenses and payroll.                                 |
| Loads       | Load volume, on-time rates, and status breakdowns.                    |
| Drivers     | Miles, load counts, on-time delivery, and a detailed view per driver. |
| Team        | Headcount and activity across the company.                            |
| Payroll     | Pay totals, history, and cost trends.                                 |
| Maintenance | Service costs and upcoming work per truck.                            |
| Safety      | Inspections, defects, accidents, and behavior events.                 |
| IFTA        | The quarterly fuel tax report described above.                        |

The dashboard also shows active loads, fleet use, revenue, and a live map.

### Communication

| Feature       | What it does                                                                                                                                                     |
| ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Messaging     | Chat between dispatchers and drivers, with direct conversations, per-load threads, and company-wide announcements. Read receipts and typing indicators included. |
| Notifications | In-app notifications, push notifications to the driver app through Firebase, and emails through Resend.                                                          |

### Settings

| Feature          | What it does                                                                                                                                      |
| ---------------- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| Roles            | Seven roles: super admin, admin, owner, manager, dispatcher, driver, and customer. Each has its own set of permissions.                           |
| Invitations      | Invite team members by email.                                                                                                                     |
| Company settings | Company profile, billing address, tax rates, integrations, and feature toggles.                                                                   |
| Subscription     | Three plans (Starter, Professional, Enterprise). Some features are locked by plan. Billing runs through Stripe, priced per truck.                 |
| Setup checklist  | A dashboard checklist for a new company: profile, first truck, team, first customer, first load, Stripe payouts, ELD.                             |
| Solo mode        | For one-person carriers. Hides employees, payroll, timesheets, and messaging, shortens the checklist, and points the dispatch agent at one truck. |

## Customer portal

The web app for a carrier's customers.

| Feature      | What it does                                                           |
| ------------ | ---------------------------------------------------------------------- |
| Shipments    | See every shipment, its status, expected delivery, and history.        |
| Tracking     | Follow a shipment on a live map.                                       |
| Invoices     | View and download invoices, and pay them online.                       |
| Documents    | Download proof of delivery, bills of lading, and other load paperwork. |
| Public links | Tracking and payment links work without an account.                    |

## Driver app

A Kotlin Multiplatform app for Android and iOS.

| Feature           | What it does                                                                             |
| ----------------- | ---------------------------------------------------------------------------------------- |
| Trips and loads   | See assigned trips and loads with stops, cargo details, and instructions.                |
| Status updates    | Update a load as you go: en route, at pickup, loaded, at delivery, delivered.            |
| Proof of delivery | Capture photos, a signature, the receiver's name, and GPS coordinates.                   |
| Inspections       | Fill in pre-trip and post-trip DVIR forms and vehicle condition reports with the camera. |
| Licenses          | See your own driver licenses and their expiry dates.                                     |
| Messaging         | Chat with dispatch.                                                                      |
| Stats             | Personal performance, past loads, and earnings.                                          |

## Admin portal

The web app for the platform operator.

| Feature          | What it does                                                                     |
| ---------------- | -------------------------------------------------------------------------------- |
| Tenants          | Create and manage carrier companies. See users, plan, and subscription status.   |
| Plans            | Manage subscription plans and see active subscriptions.                          |
| Users and admins | Manage users across all companies and the platform admin accounts.               |
| Feature flags    | Turn features on or off per company, beyond what the plan allows.                |
| AI settings      | Pick the model every company uses and set budgets.                               |
| IFTA rates       | Enter the quarterly tax rates per jurisdiction.                                  |
| License          | Install the commercial license key.                                              |
| Data requests    | Handle GDPR export and deletion requests.                                        |
| Website          | Blog posts, contact form submissions, and demo requests from the marketing site. |

## Platform

| Feature      | What it does                                                                                                              |
| ------------ | ------------------------------------------------------------------------------------------------------------------------- |
| Multi-tenant | Each company gets its own PostgreSQL database. A master database holds the companies, subscriptions, and shared settings. |
| Live updates | GPS positions, messages, and notifications arrive over SignalR connections.                                               |
| API          | A REST API with OpenAPI docs. The Angular apps use TypeScript clients generated from the spec.                            |
| Sign-in      | OAuth2 and OpenID Connect through Duende IdentityServer, with JWT access tokens and refresh token rotation.               |
| Deployment   | Docker Compose, with builds and deploys on GitHub Actions.                                                                |
