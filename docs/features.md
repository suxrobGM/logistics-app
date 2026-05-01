# Features

Complete feature list for LogisticsX.

## Supported Equipment

LogisticsX is equipment-agnostic and ships with built-in support for the following truck and trailer types: **Flatbed**, **Freight Truck** (dry van), **Reefer**, **Tanker**, **Box Truck**, **Dump Truck**, **Tow Truck**, **Car Hauler**, **Car Transporter**, **Container Truck**, **Low Loader**, **Tautliner**, **Swap Body**, and **Curtainsider**. The same platform serves general freight, refrigerated, bulk/liquid, heavy haul, intermodal drayage, and vehicle transport operators.

## Supported Regions

The platform supports tenants operating in the **United States** (USD) and **Europe** (EUR), with region-aware address and country validation, locale-aware Mapbox defaults, and region-specific demo seeders. The European region covers EU-27, EEA/EFTA, the United Kingdom, the Western Balkans, selected Eastern Europe (Moldova, Ukraine), and the European microstates.

---

## TMS Portal

The dispatcher and manager web interface for managing all fleet operations.

### AI Dispatch

| Feature                    | Description                                                                                                                                                                                                    |
| -------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Autonomous Dispatch**    | AI agent analyzes unassigned loads, truck locations, HOS hours, truck type compatibility, and revenue per mile to find optimal load-to-truck assignments.                                                      |
| **Human-in-the-Loop Mode** | Agent suggests assignments for dispatcher approval. Approve, reject, or re-plan with rejection context for refined suggestions.                                                                                |
| **Fully Autonomous Mode**  | Agent executes decisions in real-time — assigns loads, creates trips, and dispatches without manual intervention.                                                                                              |
| **Multi-Provider LLM**     | Pluggable AI providers: Anthropic (Claude Sonnet, Haiku, Opus), OpenAI (GPT-5.4 series), and DeepSeek. Model selection per session.                                                                            |
| **Agent Tool Registry**    | 7+ tools including fleet search, HOS feasibility checks, assignment scoring, load board search, trip creation, and dispatch execution.                                                                         |
| **Session Audit Trail**    | Full transparency — every tool call, reasoning step, and decision is logged in a visual timeline with timestamps.                                                                                              |
| **Re-Planning**            | Reject suggestions and re-run the agent with rejection context so it can find alternative assignments.                                                                                                         |
| **Quota Management**       | Multiplier-based weekly quotas with tiered model access by subscription plan. Overage billing via Stripe.                                                                                                      |
| **MCP Server**             | Connect Claude Desktop, Cursor, Windsurf, and other AI tools directly to fleet data via the Model Context Protocol. Tenant-managed API keys with one-time display, SHA-256 hashing, and per-key rate limiting. |

### Operations

| Feature                  | Description                                                                                                                                                                               |
| ------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Load Management**      | Create, assign, and track shipments from origin to destination. Manage pickup/delivery stops, special instructions, cargo details, and status updates through the full lifecycle.         |
| **Trip Planning**        | Organize multiple loads into trips with optimized multi-stop routing. Assign drivers and trucks with automatic conflict detection. Heuristic and Mapbox-powered route optimization.       |
| **Fleet Management**     | Track trucks, trailers, and equipment. Monitor maintenance schedules, registration expiration, insurance, and vehicle assignments. VIN decoding via NHTSA API for automatic vehicle info. |
| **Maintenance Tracking** | Schedule and track vehicle maintenance tasks. Record service history, costs, and upcoming due dates. Link maintenance records to specific trucks.                                         |
| **GPS Tracking**         | Live driver location updates via SignalR WebSocket connections. View all active drivers on an interactive Mapbox map with route visualization.                                            |
| **Customer Management**  | Maintain customer profiles with contacts, billing addresses, and shipment history. Support for multiple contacts per customer.                                                            |

### Intermodal Operations

| Feature                      | Description                                                                                                                                                                                                                                    |
| ---------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Container Tracking**       | Track intermodal containers across multiple loads over their full lifecycle. Stores ISO 6346 container number (4-letter owner/category code + 7 digits), seal number, booking reference, bill of lading, gross weight, and laden/empty status. |
| **ISO 6346 Container Types** | Built-in support for common ISO size/type codes: 20'/40' General Purpose, 40'/45' High Cube, 20'/40' Reefer, 20'/40' Open Top, 20'/40' Flat Rack, and 20' Tank.                                                                                |
| **Container Status Machine** | State machine-driven lifecycle with enforced transitions: `Empty → Loaded → At Port → In Transit → Delivered → Returned`. Status changes raise domain events for downstream automation.                                                        |
| **Load Linking**             | Link a container to one or more loads as it moves across legs of a journey (drayage, line haul, return). Decoupled from load lifecycle so a single container can serve multiple shipments.                                                     |
| **Terminal Directory**       | Manage intermodal facilities used as pickup / drop-off points: sea ports, rail terminals, inland depots, air cargo facilities, and border crossings.                                                                                           |
| **UN/LOCODE Support**        | Each terminal is keyed by its UN/LOCODE (e.g. `BEANR` Antwerp, `USLAX` Los Angeles, `DEHAM` Hamburg) for unambiguous global identification, with ISO 3166-1 country code and full address.                                                     |

### Load Board Integration

| Feature                   | Description                                                                                                                                                       |
| ------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Multi-Provider Search** | Search for available freight across DAT, Truckstop.com, and 123Loadboard from a single interface. Filter by origin, destination, dates, equipment type, and rate. |
| **Book Loads**            | Book loads directly from search results. Automatically creates a new load in the TMS with all details pre-filled from the listing.                                |
| **Post Trucks**           | Advertise available capacity to load boards. Specify truck location, destination preferences, equipment type, and availability window with auto-refresh.          |

### Financial

| Feature                | Description                                                                                                                                                                                     |
| ---------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Invoicing**          | Automated invoice generation for completed loads. Itemized charges with base rate, fuel surcharge, detention, accessorial fees, and more. Three invoice types: Load, Payroll, and Subscription. |
| **Payment Processing** | Stripe integration for credit card and ACH payments. Shareable payment links with expiration. Supports partial payments with balance tracking.                                                  |
| **Stripe Connect**     | Direct bank deposits for trucking companies via Stripe Connect destination charges. Express account onboarding for simplified setup.                                                            |
| **Payroll Management** | Calculate driver pay by miles, percentage, or flat rate. Generate payroll invoices with PDF stubs. Track payment history per employee.                                                          |
| **Timesheets**         | Track employee work hours and overtime. Link timesheets to payroll calculations for accurate compensation.                                                                                      |
| **Expense Tracking**   | Record and categorize fleet expenses — fuel, tolls, repairs, insurance, and more. Monitor spending with breakdowns by category, truck, or time period.                                          |

### Compliance & Safety

| Feature                        | Description                                                                                                                                                                                                 |
| ------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **ELD / HOS**                  | Integration with ELD providers (Samsara, Motive) for Hours of Service tracking. Real-time duty status, driving hours, and violation alerts. Webhook-based status sync.                                      |
| **Vehicle Inspections (DVIR)** | Digital pre-trip and post-trip Driver Vehicle Inspection Reports. Interactive damage marking on vehicle diagrams, defect categorization by severity, and photo attachments. Inspection history per vehicle. |
| **Safety & Incidents**         | Track safety incidents, violations, and corrective actions. Maintain compliance records for audits.                                                                                                         |
| **Document Management**        | Centralized storage for load paperwork (POD, POL, BOL), employee records, and compliance documents. Azure Blob Storage backend with secure access.                                                          |

### Reports & Analytics

| Feature             | Description                                                                                                             |
| ------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| **Dashboard**       | Real-time overview with active loads, fleet utilization, revenue metrics, and interactive map showing driver locations. |
| **Driver Report**   | On-time delivery rates, miles driven, load counts, and efficiency metrics per driver.                                   |
| **Truck Report**    | Vehicle utilization, mileage tracking, and maintenance cost analysis per truck.                                         |
| **Revenue Report**  | Daily, weekly, and monthly gross revenue with trend visualization and breakdowns.                                       |
| **Customer Report** | Shipment volume trends, revenue per customer, and customer activity metrics.                                            |
| **Payroll Report**  | Compensation summaries, payment history, and payroll cost trends.                                                       |
| **Expense Report**  | Spending breakdowns by category, truck, and time period with trend analysis.                                            |

### Communication

| Feature                 | Description                                                                                                                                                                           |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Real-Time Messaging** | Built-in chat between dispatchers and drivers via SignalR. Supports direct conversations, load-specific threads, and company-wide announcements. Read receipts and typing indicators. |
| **Push Notifications**  | Instant alerts for load assignments, status changes, and important updates via Firebase Cloud Messaging.                                                                              |
| **Email Notifications** | Automated email notifications for invoices, payment confirmations, and account events using Fluid templates.                                                                          |

### Settings & Configuration

| Feature                       | Description                                                                                                                    |
| ----------------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| **Role-Based Access Control** | Six roles with granular permissions: Owner, Manager, Dispatcher, Driver, Customer, and Super Admin.                            |
| **Company Settings**          | Configure company profile, billing address, notification preferences, and feature toggles.                                     |
| **Subscription Management**   | Three tiers (Starter, Professional, Enterprise) with plan-based feature gating. Stripe-powered billing with per-truck pricing. |

---

## Customer Portal

Self-service web portal for trucking company customers.

| Feature                  | Description                                                                                       |
| ------------------------ | ------------------------------------------------------------------------------------------------- |
| **Shipment Tracking**    | View all shipments with real-time status updates, estimated delivery times, and shipment history. |
| **Invoice Access**       | View and download invoices. Make payments online via Stripe-powered payment links.                |
| **Document Downloads**   | Access proof of delivery (POD), bill of lading (BOL), and other load documents.                   |
| **Public Tracking**      | Shareable tracking links for shipment visibility without requiring login.                         |
| **Public Payment Links** | Direct payment links that customers can use without an account.                                   |

---

## Driver Mobile App

Native Kotlin Multiplatform app for Android and iOS.

| Feature                 | Description                                                                                          |
| ----------------------- | ---------------------------------------------------------------------------------------------------- |
| **Load Assignments**    | Receive and accept load assignments with full details — stops, cargo info, and special instructions. |
| **Navigation**          | Turn-by-turn navigation integrated with maps for each stop.                                          |
| **Proof of Delivery**   | Capture photos, digital signatures, recipient name, and GPS coordinates at delivery.                 |
| **Vehicle Inspections** | Complete pre-trip and post-trip DVIR reports with the device camera.                                 |
| **Status Updates**      | Update load status in real-time (en route, at pickup, loaded, at delivery, delivered).               |
| **Messaging**           | Chat with dispatchers directly from the app.                                                         |
| **Statistics**          | View personal performance stats, load history, and earnings.                                         |

---

## Admin Portal

Super admin management interface for platform operators.

| Feature                     | Description                                                                                                        |
| --------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| **Tenant Management**       | Create, configure, and manage trucking company tenants. View tenant details, user counts, and subscription status. |
| **Subscription Management** | Manage subscription plans and billing. Monitor active subscriptions across all tenants.                            |
| **User Management**         | View and manage users across all tenants. Role assignments and account status.                                     |
| **Blog Management**         | Create and publish blog posts for the marketing website.                                                           |
| **Demo Requests**           | View and manage demo request submissions from the website.                                                         |

---

## Platform & Architecture

| Feature                       | Description                                                                                                                                   |
| ----------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------- |
| **Multi-Tenant Architecture** | Complete data isolation with separate PostgreSQL databases per company. Master database for tenants, subscriptions, and shared configuration. |
| **Cloud-Native**              | .NET Aspire orchestration, Docker containerization, and GitHub Actions CI/CD.                                                                 |
| **Real-Time Updates**         | SignalR WebSocket connections for GPS tracking, messaging, and notifications across all clients.                                              |
| **API-First**                 | RESTful API with OpenAPI documentation. Auto-generated TypeScript clients for Angular apps.                                                   |
| **OAuth2/OIDC**               | Duende IdentityServer for secure authentication with JWT tokens and refresh token rotation.                                                   |
