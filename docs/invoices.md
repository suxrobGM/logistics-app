# Invoice System

The platform's invoice system handles automatic invoice creation, line items, partial payments, and public payment links.

## Invoice Types

The system uses Table Per Hierarchy (TPH) pattern with three invoice types:

### LoadInvoice

- **Auto-created** when a Load is created (status: Draft)
- Linked to a Customer and Load
- Contains freight charges based on `Load.DeliveryCost`
- Default line item: "Freight charges for Load #{number}"

### PayrollInvoice

- Created for employee salary payments
- Supports multiple salary calculation types:
  - Per Mile
  - Per Mile (Team)
  - Percentage
  - Salary
  - Salary + Per Mile
  - Hourly

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
| Discount      | Price reductions        |
| Tax           | Tax charges             |
| Other         | Miscellaneous           |

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
  goes back to sent/overdue and `FactoringStatus` becomes `ChargedBack`.

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
