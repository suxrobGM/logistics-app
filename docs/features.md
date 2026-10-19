# Features

Everything LogisticsX does today, grouped by app. Items that are planned but not built live in the [roadmap](roadmap/README.md).

## Equipment

The system is not tied to one kind of truck. These types are built in: flatbed, dry van, reefer, tanker, box truck, dump truck, tow truck, car hauler, car transporter, container truck, low loader, tautliner, swap body, and curtainsider. The same install can run a general freight company, a reefer fleet, a heavy haul outfit, a drayage operator, or a vehicle transport carrier.

## Regions

A company is set up as US or European. That choice sets the currency (USD or EUR), the units, the address validation, the country list, the map defaults, and the demo data. Europe here means the EU, the EEA and EFTA countries, the United Kingdom, the Western Balkans, Moldova, Ukraine, and the European microstates.

## TMS portal

The web app for owners, managers, and dispatchers.

### Dispatch and operations

| Feature | What it does |
| --- | --- |
| Loads | Create loads with pickup and delivery stops, cargo details, and instructions. Follow each load from booked to delivered. |
| PDF import | Upload a rate confirmation PDF and the system fills in the load for you. It reads the text first and falls back to an AI vision model for scanned pages. |
| Trips | Group loads into a trip with a planned multi-stop route. The system warns when a driver or truck is double-booked. Routes come from Mapbox. |
| Live tracking | See every active driver on a map. Positions update over a live connection as drivers move. |
| Public tracking | Share a tracking link with a customer or receiver. No login is needed. |
| Fleet | Trucks and trailers, registration and insurance dates, and who is assigned to what. Hook a trailer to a truck and each trip records the trailer it pulled. Enter a VIN and the truck details fill in from the NHTSA database. |
| Data import | Move off another TMS or a spreadsheet: upload CSV or XLSX for customers, drivers, trucks, trailers and loads, check the column mapping, see a dry run, then import. See [data import](data-import.md). |
| Maintenance | Record what was done on each truck and what it cost. |
| Customers | Customer profiles with several contacts each, billing addresses, and shipment history. |
| LTL freight | Record freight class, NMFC code, pallets, pieces, weight, dimensions and whether the freight stacks on the load. |
| Hazmat and ADR | Mark a load as hazmat and record ADR equipment on each truck. Dispatch refuses to send a hazmat load to a truck without the right equipment. |
| Driver licenses | Track each driver's license classes and expiry dates. Drivers get reminders before a license expires, and dispatch will not assign an unlicensed driver. |

### Intermodal

| Feature | What it does |
| --- | --- |
| Container tracking | Track a container across several loads by its ISO 6346 number, with seal number, booking reference, bill of lading, weight, and laden or empty status. |
| Container types | Common ISO size and type codes are built in: 20' and 40' general purpose, 40' and 45' high cube, reefers, open tops, flat racks, and 20' tanks. |
| Container lifecycle | A container moves through fixed stages: empty, loaded, at port, in transit, delivered, returned. Each change raises an event other parts of the system can react to. |
| Terminals | A directory of sea ports, rail terminals, inland depots, air cargo facilities, and border crossings, each keyed by its UN/LOCODE (for example `USLAX` or `DEHAM`). |
| Free time | Enter the last free day and the empty return deadline with daily demurrage and per-diem rates. Every container shows days left or days over and the charge so far, in grey, amber or red. |
| Deadline alerts | Each morning dispatchers get a notification for every container whose last free day or return deadline is today, tomorrow, or already past, with the accrued charge. |
| Holds and appointments | Mark customs, freight, line or other holds and the available-for-pickup time, and record pickup and return appointment numbers and windows. Entered by hand; there is no terminal system feed yet. |
| Chassis | Record the chassis number, its pool (own, DCLI, TRAC, Flexi-Van) and a daily rate. Chassis days are counted from outgate to return. |
| Street turns | Mark an empty import box as reused for an export booking instead of going back to the line. Its per-diem clock stops there. |
| Drayage billing | One button on the load writes demurrage, per-diem and chassis lines (rate times days) onto the load's invoice. Running it again replaces those lines. |
| Deadline-aware AI | The dispatch agent sees each load's last free day and hold flag, puts a box one day from demurrage ahead of loads with slack, and skips boxes on hold. |

### Car haulers

| Feature | What it does |
| --- | --- |
| Vehicles on a load | A vehicle load lists each car with VIN, year, make, model, color, lot or stock number, operable flag, notes, and an optional per-vehicle rate. Decode VIN fills in the rest from NHTSA. |
| PDF import | Dispatch sheets with several vehicles import as one load with every vehicle on it. |
| Capacity warning | Each car hauler has a vehicle capacity. The load page, the eligibility check and the AI agent warn when a truck's open loads carry more vehicles. It never blocks. |
| Per-vehicle reports | The driver inspects each vehicle at pickup and at delivery. The delivery report shows the pickup damage for comparison. |
| Damage diagram | Damage is marked on a top-view car diagram by zone with the BOL codes S, D, C, CR, M and B, a severity, and an optional photo. The TMS and the eBOL show the same diagram. |
| Signatures | The shipper signs the BOL at pickup and the receiver signs the POD at delivery, on the driver's phone. If nobody can sign, the driver records why. |
| Electronic BOL | A PDF with every vehicle, its damage at pickup and delivery, both signatures, times and GPS. You can download it from the load in the TMS and from the shipment in the customer portal, and the customer is emailed a link at delivery. |

Setup: [car hauler guide](car-hauler-guide.md).

Central Dispatch and Super Dispatch are not connected. Neither offers a public carrier API for searching or booking loads (see [the roadmap note](roadmap/car-hauler-load-boards.md)).

### Load boards

| Feature | What it does |
| --- | --- |
| Search | Search DAT from one screen by origin, destination, dates and equipment, with an optional rate floor. A board that fails shows its error, so a failure never looks like "no freight". More boards are added as carriers need them. See [DAT setup](loadboard-dat-setup.md). |
| DAT market rates | With a DAT plan that includes RateView, each result shows the DAT spot rate for its lane. |
| Create load | Turn a listing into a load in the TMS with the truck assigned. Load boards have no booking API, so you confirm with the broker using the phone and email shown. |
| Broker credit check | Every listing shows the broker's credit score, days to pay, and FMCSA authority status. Loads below your minimum score are blocked unless a dispatcher overrides. |
| Post trucks | Advertise an open truck with its location, preferred destination, equipment, and availability window. Remove it here; posts that expired on DAT are marked expired. |

### AI

| Feature | What it does |
| --- | --- |
| Dispatch agent | A shared conversation where you ask the agent to plan the day. It proposes which truck takes which load, checks hours of service, and explains each choice. |
| Approvals | Every action that changes data becomes a decision waiting for a dispatcher. Approve it or reject it with a reason. Nothing changes until someone approves. |
| Timeline | Every tool call, reasoning step, and decision in a turn is written to a timeline with timestamps. |
| Follow-ups | Reject a suggestion or send a follow-up message, and the agent adjusts within the same conversation. |
| Learned preferences | A nightly job reads the decisions you approved and rejected and writes a short policy the agent follows next time. Dispatchers can edit it. |
| Rate negotiation | When a listing pays below your lane floor, the agent drafts a counter-offer email to the broker. You approve before it sends. Replies come back into the conversation. See [broker email negotiation](broker-email-negotiation.md). |
| Copilot | A chat panel on every page. Ask about loads, customers, or expenses, or have it invoice delivered loads and send payment links. It only uses the tools your role allows, and it asks before it writes. See [AI Copilot](ai-copilot.md). |
| Tools | Both agents draw on one shared tool catalogue covering dispatch, load boards, negotiation, finance, intermodal, and operations. |
| MCP server | Connect Claude Desktop, Cursor, Windsurf, and other MCP clients to your fleet. API keys are created per company, shown once, and rate limited. See [MCP Server](mcp-server.md). |

### Money

| Feature | What it does |
| --- | --- |
| Invoices | Every load gets a draft invoice whose lines follow the load: linehaul, fuel surcharge and one line per accessorial, rebuilt when those change. Three invoice kinds: load, payroll, and subscription. |
| Accessorials | A price list of extra charges (lumper, layover, liftgate, extra stop) billed flat, per hour, per mile or per stop. Add one to a load from the list or type one in. |
| Detention | Set free time and an hourly rate for the company or per customer. When the driver departs a stop past the free time, detention in quarter hours goes on the invoice; dispatchers are alerted when a truck overstays. |
| Tax | Tax is worked out by Stripe Tax or by rates you enter yourself. US sales tax and EU VAT are both handled, and invoices show the breakdown. |
| Payments | Card and bank payments through Stripe. Send a payment link that expires, and accept partial payments against a balance. |
| Stripe Connect | Payments go straight to the company's bank account through Stripe Connect. Onboarding uses Stripe Express. |
| Factoring | Factored customers' invoices carry the factor's remit-to. Email schedules with PODs and BOLs to the factor, then track advance, fee, reserve and chargebacks. See [invoices](invoices.md). |
| Payroll | Pay drivers by mile, by share of gross, per load, per stop, hourly, or a fixed weekly or monthly salary. Generate payroll invoices with PDF stubs and keep a payment history per employee. |
| Owner-operator settlements | Pay a share of linehaul with or without fuel surcharge and accessorials, and take truck lease, insurance, ELD fee, escrow (up to a target), cash advances and fuel card purchases off each settlement. The statement lists every deduction. |
| Timesheets | Track hours and overtime and feed them into payroll. |
| Expenses | Record fuel, tolls, repairs, insurance, and other costs. Break spending down by category, truck, or period. |
| Fuel cards | WEX and EFS transactions import every night as paid fuel expenses, matched to trucks by card or unit number. Unmatched ones wait in a review queue. |
| QuickBooks | Customers, invoices, payments, expenses, and approved driver pay sync to QuickBooks Online every 15 minutes, posting to the items and accounts you map. See [QuickBooks setup](accounting-quickbooks-setup.md). |

### Compliance and safety

| Feature | What it does |
| --- | --- |
| ELD and HOS | Hours of service from Samsara, Motive and Geotab, live GPS from Samsara and TT ELD. Clocks and GPS sync every 5 minutes, logs and violations every 30; Samsara HOS-violation alerts arrive by webhook. See [Samsara setup](eld-samsara-setup.md). |
| IFTA | Quarterly fuel tax per jurisdiction. Miles come from ELD GPS, gallons from fuel cards and expenses. Closed quarters are frozen for audits. Includes filing reminders and PDF and CSV export. |
| DVIR | Digital pre-trip and post-trip inspections with damage marked on a vehicle diagram, defects by severity, and photos. History per vehicle. |
| Accidents | Record accident reports with the people, vehicles, and documents involved, and follow them to close. |
| Documents | One place for proof of delivery, bills of lading, employee records, and compliance paperwork. Files are stored in Azure Blob Storage, Cloudflare R2, or on local disk. |
| Privacy (GDPR) | Customers and employees can ask for a copy of their data or for deletion. Requests are handled in the admin portal, and retention rules clean up old data on their own. |

### Reports

| Report | What it shows |
| --- | --- |
| Revenue | Gross revenue by day, week, and month, with trends. |
| Financial | Revenue against expenses and payroll. |
| Loads | Load volume, on-time rates, and status breakdowns. |
| Drivers | Miles, load counts, on-time delivery, and a detailed view per driver. |
| Team | Headcount and activity across the company. |
| Payroll | Pay totals, history, and cost trends. |
| Maintenance | Service costs per truck, type and vendor. |
| Safety | Inspections, defects, and accidents. |
| IFTA | The quarterly fuel tax report described above. |

The dashboard also shows active loads, fleet use, revenue, and a live map.

### Communication

| Feature | What it does |
| --- | --- |
| Messaging | Chat between dispatchers and drivers, with direct conversations, per-load threads, and company-wide announcements. Read receipts and typing indicators included. |
| Notifications | In-app notifications, push notifications to the driver app through Firebase, and emails through Resend. |

### Settings

| Feature | What it does |
| --- | --- |
| Roles | Seven roles: super admin, admin, owner, manager, dispatcher, driver, and customer. Each has its own set of permissions. |
| Invitations | Invite team members by email. |
| Company settings | Company profile, billing address, tax rates, and integrations. |
| Subscription | Three plans (Starter, Professional, Enterprise). Some features are locked by plan. Billing runs through Stripe, priced per truck. |
| Setup checklist | A dashboard checklist for a new company: profile, first truck, team, first customer, first load, Stripe payouts, ELD. |
| Solo mode | An admin-set preset for one-person carriers. Turns off payroll and timesheets, hides employees and messaging, and points the AI at one truck. |

## Customer portal

The web app for a carrier's customers.

| Feature | What it does |
| --- | --- |
| Shipments | See every shipment, its status, expected delivery, and history. |
| Tracking | Follow a shipment on a live map. |
| Invoices | View and download invoices, and pay them online. A factored customer's invoices show the factor to pay instead of a card checkout. |
| Documents | Download proof of delivery, bills of lading, the electronic BOL of vehicle shipments, and other load paperwork. |
| Public links | Tracking and payment links work without an account. |

## Driver app

A Kotlin Multiplatform app for Android and iOS.

| Feature | What it does |
| --- | --- |
| Trips and loads | See assigned trips and loads with stops, cargo details, and instructions. |
| Status updates | Update a load as you go: en route, at pickup, loaded, at delivery, delivered. |
| Proof of delivery | Capture photos, a signature, the receiver's name, and GPS coordinates. |
| Inspections | Fill in pre-trip and post-trip DVIR forms, and condition reports for each vehicle on a car hauler load, with the car damage diagram and the camera. |
| Licenses | See your own driver licenses and their expiry dates. |
| Messaging | Chat with dispatch. |
| Stats | Personal performance, past loads, and earnings. |

## Admin portal

The web app for the platform operator.

| Feature | What it does |
| --- | --- |
| Tenants | Create and manage carrier companies, set their presets, see users and plan. |
| Plans | Manage subscription plans and see active subscriptions. |
| Users and admins | Manage users across all companies and the platform admin accounts. |
| Feature flags | Turn features on or off per company, beyond what the plan allows. |
| AI settings | Pick the model every company uses and set budgets. |
| IFTA rates | Enter the quarterly tax rates per jurisdiction. |
| License | Install the commercial license key. |
| Data requests | Handle GDPR export and deletion requests. |
| Website | Blog posts, contact form submissions, and demo requests from the marketing site. |

## Platform

| Feature | What it does |
| --- | --- |
| Multi-tenant | Each company gets its own PostgreSQL database. A master database holds the companies, subscriptions, and shared settings. |
| Live updates | GPS positions, messages, and notifications arrive over SignalR connections. |
| API | A REST API with OpenAPI docs. The Angular apps use TypeScript clients generated from the spec. |
| Sign-in | OAuth2 and OpenID Connect through Open.IdentityServer, with JWT access tokens and refresh token rotation. |
| Deployment | Docker Compose, with builds and deploys on GitHub Actions. |
| Error tracking | API, Identity Server and portal errors go to Sentry when a DSN is set, with personal data scrubbed and a tenant tag. |
| Health | /health reports unhealthy when the master database or the Hangfire job servers stop responding. |
| Backups | Nightly backups of the master and every tenant database, an optional off-site copy, and a restore script. See [observability and backups](deployment/observability-and-backups.md). |
