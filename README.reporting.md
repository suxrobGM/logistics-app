Reporting Feature - Loads, Drivers, Financials

Overview
This change adds a complete reporting capability across backend and frontend:
- Backend: New CQRS queries/handlers and an API controller under route `reports`.
- Shared: New DTOs and queries for loads, drivers, and financials reports.
- Frontend (Admin): New Blazor pages under Reports menu to visualize and filter reports.

Backend
- Added DTOs and queries:
  - `LoadsReportDto`, `LoadsReportItemDto`, `LoadsReportQuery`
  - `DriversReportDto`, `DriversReportItemDto`, `DriversReportQuery`
  - `FinancialsReportDto`, `FinancialsReportItemDto`, `FinancialsReportQuery`
- Implemented handlers:
  - `LoadsReportHandler`: aggregates and pages loads with totals.
  - `DriversReportHandler`: aggregates per-driver stats (loads delivered, distance, gross).
  - `FinancialsReportHandler`: aggregates invoices with totals paid/due.
- Added `ReportController` with endpoints:
  - GET `reports/loads`
  - GET `reports/drivers`
  - GET `reports/financials`
  All protected by `Permissions.Stats.View`.
  - Export endpoints:
    - GET `reports/loads/export?format=csv|xlsx|pdf|docx`
    - GET `reports/drivers/export?format=csv|xlsx|pdf|docx`
    - GET `reports/financials/export?format=csv|xlsx|pdf|docx`
  - Export service: `IReportExportService` with implementation `ReportExportService` (currently CSV; swap in QuestPDF, ClosedXML, OpenXML easily).

Frontend (Admin)
- Navigation: Added Reports group to `MainLayout.razor` with three items.
- Pages:
  - `/reports/loads`: filters by date range and search; grid of loads with totals.
  - `/reports/drivers`: filters by date range and search; per-driver metrics with totals.
  - `/reports/financials`: filters by date range and search; invoice metrics with totals.

HTTP Client
- Added `IReportsApi` and implemented methods in `ApiClient`:
  - `GetLoadsReportAsync`, `GetDriversReportAsync`, `GetFinancialsReportAsync`
- Extended `IApiClient` to include `IReportsApi`.

Frontend (Angular)
- Location: `src/Client/Logistics.ReportingPortal`
- Routing to three pages under `/reports/*` with filters and export buttons.
- Uses Angular standalone components with HttpClient.
- Commands:
  - Install deps: `npm install`
  - Run: `npm start` (defaults to port 4200)
  - Configure API base (default assumes same-origin; adjust proxy if needed).
 
OfficeApp Integration (Angular)
- Added in `src/Client/Logistics.OfficeApp` under `src/app/pages/reports`:
  - Routes: `reports.routes.ts` mounted at `/reports` in `app.routes.ts`
  - Layout: `reports.layout.ts` with tabs (Loads, Drivers, Financials)
  - Views: `views/loads.ts`, `views/drivers.ts`, `views/financials.ts` with filters and export buttons
- Tenant header
  - OfficeApp already injects `X-Tenant` via `tenant.interceptor.ts` using `TenantService`
  - On login, `AuthService.checkAuth()` sets tenant id from token claim so reports work without manual input

Export Formats (Real Implementations)
- Excel (xlsx): ClosedXML
- PDF: QuestPDF
- Word (docx): OpenXML SDK
- Implemented in `ReportExportService` with `ExportXlsx`, `ExportPdf`, and `ExportDocx`.
- NuGet packages added in `Logistics.Infrastructure.csproj`:
  - `ClosedXML`
  - `QuestPDF` (resolved to latest compatible minor)
  - `DocumentFormat.OpenXml`

How to Run
1) Ensure .NET SDK is installed and available as `dotnet`. If it's missing in your environment, install .NET 8 SDK.
2) From repository root:
   - Build: `dotnet build Logistics.sln`
   - Run API: `dotnet run --project src/Presentation/Logistics.API/Logistics.API.csproj`
   - Run Admin (WASM): Serve via `dotnet run` in identity/API and use existing hosting or static server.

Endpoints
- GET `/reports/loads?from=...&to=...&search=...&status=...&page=1&pageSize=10`
- GET `/reports/drivers?from=...&to=...&search=...&page=1&pageSize=10`
- GET `/reports/financials?from=...&to=...&status=...&search=...&page=1&pageSize=10`

Notes
- Aggregations use existing entities via `ITenantUnitOfWork` and repository `Query()` access.
- Permissions align with existing `Stats.View` policy.

