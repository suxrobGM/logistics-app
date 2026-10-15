# Invoice System

The platform's invoice system handles automatic invoice creation, line items, partial payments, and public payment links.

## Invoice Types

The system uses Table Per Hierarchy (TPH) pattern with three invoice types:

### LoadInvoice

- **Auto-created** by `LoadFactory` when a Load is created (status: Draft)
- Linked to a Customer and Load
- Its lines are built from the load by `LoadInvoice.ReplaceLoadCharges`: one `BaseRate` line for
  `Load.DeliveryCost` (the linehaul), one `FuelSurcharge` line for `Load.FuelSurcharge` when it is
  not 0, and one line per `LoadAccessorial` (rate x quantity, typed as the accessorial's line type).
  A fractional quantity (2.25 h of detention) becomes a single line carrying the total, with the
  arithmetic in the description, because line quantities are whole numbers.
- **Regenerated** whenever a load update changes the linehaul, the load type, the fuel surcharge or
  the accessorials, and whenever detention or drayage charges are written. Regeneration owns only the
  `BaseRate` and `FuelSurcharge` lines and lines whose `SourceId` points at an accessorial; hand-added
  lines are kept. A paid or cancelled invoice is never
  regenerated.

### Accessorials and detention

- **Catalog** (`AccessorialCharge`, Settings > Accessorials, `GET/POST/PUT/DELETE /accessorials`):
  name, line type, default amount and unit (`Flat`, `PerHour`, `PerDistance`, `PerStop`). Picking
  one on the load form copies its values into a `LoadAccessorial`, so a later price change never
  rewrites existing invoices. Ad hoc accessorials need no catalog entry.
- **Detention terms**: tenant default in `TenantSettings.DetentionFreeMinutes` (null = 120) and
  `DetentionRatePerHour` (null = tracked and alerted, not billed), `GET/PUT /accessorials/detention`;
  per customer in `Customer.DetentionFreeMinutes`/`DetentionRatePerHour`,
  `GET/PUT /customers/{id}/detention`. Customer values win field by field.
- **Calculation** (`DetentionCalculator`): dwell is `TripStop.ArrivedAt` to `DepartedAt`; detention is
  the dwell past the free time, billed in whole quarter hours rounded down.
- **Billing**: when the driver marks a stop departed, `LoadChargeService.ApplyDetentionAsync` writes
  (or updates, or removes) one `Detention` accessorial for that stop (`TripStopId`) and regenerates the
  invoice. A stop with no departure is never billed. `GET /loads/{id}/detention` returns dwell, free
  time, billable hours and charge per stop for the load details page.
- **Alert**: `DetentionAlertJob` (every 15 minutes) notifies dispatchers once per stop
  (`TripStop.DetentionAlertedAt`) when a truck is still at a stop past its free time.

### PayrollInvoice (driver settlement)

- Created for employee pay, by the weekly/monthly generation job or by hand. `PayrollService`
  builds its lines with `PayrollInvoice.ReplaceSettlementLines`: one `BasePay` line for the gross,
  then one negative line per deduction, so `Total` is the net pay.
- Gross pay by `SalaryType`: `Monthly`, `Weekly`, `Hourly` (time entries), `RatePerDistance`
  (delivered km), `PerLoad` (delivered loads x rate), `PerStop` (trip stops departed in the period x
  rate) and `ShareOfGross` - the percentage of each delivered load's linehaul plus fuel surcharge plus
  accessorials, minus what the employee's `PayExcludesFuelSurcharge` / `PayExcludesAccessorials`
  leave out. Weekly generation covers `Weekly`, `PerLoad` and `PerStop`; monthly covers `Monthly` and
  `ShareOfGross`.
- Deductions (owner-operator settlements):
  - `RecurringDeduction` rows (truck lease, insurance, ELD fee, other) become `Deduction` lines on
    every settlement; an `Escrow` one becomes an `Escrow` line, capped so the escrow balance stops at
    its `EscrowTarget`. The balance is the sum of escrow lines on the employee's settlements that
    were not rejected or cancelled (a positive escrow line is a release).
  - Approved `CashAdvance` rows not yet deducted become `CashAdvance` lines on the next settlement.
  - With `Employee.DeductFuelCardCharges`, matched fuel card transactions in the period on trucks the
    employee is the main driver of become `FuelCard` lines.
  - Advances and fuel transactions record the settlement that deducted them
    (`PayrollInvoiceId`); rejecting or deleting the settlement hands them back.
  - One-off deductions and reimbursements (lumper paid by the driver, tolls) are hand-added lines;
    the API stores any deduction-type line negative whatever sign was typed.
- Regenerating (period or employee change on a draft) rebuilds the `BasePay` line and every line
  with a `SourceId`, keeping hand-added lines.
- The pay stub PDF lists earnings, gross, each deduction and the net.
- Endpoints: `GET/PUT /employees/{id}/payprofile` (exclusions, fuel deduction, recurring deductions,
  escrow balance, advances owed); `/cashadvances` (list, issue, approve, reject, delete);
  `POST/GET /drivers/cashadvances` for the signed-in driver's requests.

### SubscriptionInvoice

- Platform subscription payments
- Created via Stripe webhooks

## Invoice Lifecycle

```text
Draft → Issued → PartiallyPaid → Paid
              ↘ Cancelled
```

1. **Draft**: Initial state, auto-created with Load
2. **Issued**: Sent to customer, awaiting payment
3. **PartiallyPaid**: Some payments received, balance due
4. **Paid**: Fully paid
5. **Cancelled**: Invoice cancelled, no further action

## Line Items

Each invoice can have multiple line items with types:

| Type          | Description             |
| ------------- | ----------------------- |
| BaseRate      | Primary freight charge  |
| FuelSurcharge | Fuel cost adjustment    |
| Detention     | Waiting time charges    |
| Layover       | Overnight delay charges |
| Lumper        | Loading/unloading labor |
| Accessorial   | Additional services     |
| Demurrage     | Container demurrage     |
| PerDiem       | Container per diem      |
| Chassis       | Chassis rental          |
| Storage       | Storage                 |
| Discount      | Price reductions        |
| Tax           | Tax charges             |
| Other         | Miscellaneous           |
| BasePay       | Settlement gross pay    |
| Bonus         | Settlement bonus        |
| Deduction     | Settlement deduction    |
| Reimbursement | Driver-paid expense     |
| Adjustment    | Settlement adjustment   |
| Escrow        | Escrow contribution     |
| CashAdvance   | Cash advance repaid     |
| FuelCard      | Fuel card purchase      |

Settlement deduction types (`Deduction`, `Escrow`, `CashAdvance`, `FuelCard`) are stored negative.

### Managing Line Items

```http
# Add line item
POST /invoices/{id}/line-items
{
  "description": "Detention fee - 2 hours",
  "type": "Detention",
  "amount": 50.00,
  "quantity": 2
}

# Remove line item
DELETE /invoices/{id}/line-items/{itemId}
```

## Payment Links

Shareable links allow customers to pay invoices without authentication.

### Creating Payment Links

```http
POST /invoices/{id}/payment-links
{
  "expiresInDays": 30  // optional, defaults to 30
}

Response:
{
  "id": "guid",
  "token": "secure-random-token",
  "url": "https://portal.example.com/pay/{tenantId}/{token}",
  "expiresAt": "2024-02-24T00:00:00Z",
  "isActive": true
}
```

### Public Payment Flow

1. Customer receives link via email or tracking page
2. Link validates tenant, expiration, and invoice status
3. Customer enters payment details (Stripe Elements)
4. Payment processed via Stripe Connect (destination charges)
5. Invoice status updated automatically

## Sending Invoices

```http
POST /invoices/{id}/send
{
  "email": "customer@example.com",
  "personalMessage": "Thank you for your business!"
}
```

This will:

1. Create a payment link if none exists
2. Render email template with invoice details
3. Send via configured email provider
4. Update `SentAt` and `SentToEmail` on invoice

## Manual Payments

Record cash or check payments:

```http
POST /invoices/{id}/payments/manual
{
  "amount": 500.00,
  "type": "Check",  // or "Cash"
  "referenceNumber": "1234",  // check number
  "notes": "Received via mail",
  "receivedDate": "2024-01-20T00:00:00Z"  // optional
}
```

## Factoring

Carriers who sell invoices to a factoring company set the factor up under Settings > Factoring
(`/factoring/companies`) and pick it on each factored customer (`Customer.FactoringCompanyId`).

- **Notice of assignment**: a factored customer's invoice PDF prints the factor's remit-to block and
  leaves out the carrier's payment state; the invoice email swaps the payment link for the notice;
  the customer portal shows "Pay to {factor}" and card checkout is refused.
- **Submit** (`POST /factoring/batches`, Load Invoices > Submit to factor): the invoices go on a
  schedule of accounts (`FactoringBatch`, one `FactoringBatchItem` per invoice) emailed to the
  factor's submission address with the schedule PDF, each invoice PDF and the loads' POD, BOL and
  rate confirmation. Each invoice must be issued, sent or overdue with no payments, billed to a
  customer factored with that factor, not already submitted, and have a POD or BOL on its load; the
  error names every invoice that fails.
- **Funding** (`POST /factoring/batches/{id}/funding`): per invoice the advance and fee, or a
  rejection. Advance + fee + reserve always equals the invoice total. Funding records a payment for
  the total, so the invoice reads Paid; the reserve stays owed by the factor until
  `POST .../items/{itemId}/reserverelease`.
- **Chargeback** (`POST .../items/{itemId}/chargeback`): the sale payment is removed, the invoice
  goes back to sent/overdue and `FactoringStatus` becomes `ChargedBack`. A sale payment already
  pushed to QuickBooks is voided there on the next sync.

`LoadInvoice.FactoringStatus` is `NotFactored`, `Submitted`, `Funded`, `Rejected` or `ChargedBack`
and filters `GET /invoices?FactoringStatus=`. The schedule status (`Submitted`, `PartiallyFunded`,
`Funded`, `Rejected`) is derived from its items. See
[factoring-integration](roadmap/factoring-integration.md) for what is not built.

## Dashboard Statistics

```http
GET /invoices/dashboard

Response:
{
  "draftCount": 5,
  "draftAmount": 2500.00,
  "pendingCount": 12,
  "pendingAmount": 15000.00,
  "overdueCount": 3,
  "overdueAmount": 4500.00,
  "partiallyPaidCount": 2,
  "partiallyPaidAmount": 1000.00,
  "paidCount": 50,
  "paidAmount": 75000.00,
  "totalOutstanding": 20500.00,
  "collectedThisMonth": 12000.00,
  "owedByCustomers": 18000.00,   // open balances customers owe the carrier
  "owedByFactor": 3200.00,       // submitted and awaiting funding, plus reserve held
  "recentInvoices": [...]
}
```

## Related Files

- Domain: `src/Core/Logistics.Domain/Entities/Invoice/`
- Commands: `src/Core/Logistics.Application/Commands/Invoice/`
- Queries: `src/Core/Logistics.Application/Queries/Invoice/`
- API: `src/Presentation/Logistics.API/Controllers/InvoiceController.cs`
- DTOs: `src/Shared/Logistics.Shared.Models/Invoice/`
