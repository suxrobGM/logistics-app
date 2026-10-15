# Invoice System

How invoices are created, what goes on them, how they get paid, and how factoring changes that.
Where the code lives: the Financial section of [the feature map](../.claude/feature-map.md).

## Invoice Types

There are three kinds of invoice, stored in one table.

### Load invoices

- Created as a draft with every load, billed to the load's customer.
- Its lines come from the load: one base-rate line for the linehaul, one fuel surcharge line when
  there is a surcharge, and one line per accessorial on the load (rate x quantity). A fractional
  quantity (2.25 h of detention) becomes one line carrying the total, with the arithmetic in the
  description, because line quantities are whole numbers.
- Rebuilt on every load update and whenever detention or drayage charges are written. The rebuild
  owns only the base-rate and fuel surcharge lines and the lines that came from an accessorial;
  hand-added lines are kept. A paid or cancelled invoice is never rebuilt.

### Accessorials and detention

- **Catalog** (Settings > Accessorials): name, line type, default amount and unit (flat, per hour,
  per distance, per stop, per day). Picking one on the load form copies its values onto the load, so
  a later price change never rewrites existing invoices. Ad hoc accessorials need no catalog entry.
- **Detention terms**: a tenant default on Settings > Company (free time defaults to 2 hours; with
  no hourly rate, detention is tracked and alerted but not billed) and optional per-customer terms
  on the customer. Customer values win field by field.
- **Calculation**: dwell runs from the driver's arrival at a stop to departure; detention is the
  dwell past the free time, billed in whole quarter hours rounded down.
- **Billing**: when the driver marks a stop departed, one detention charge for that stop is
  written (or updated, or removed) and the invoice is rebuilt. A stop with no departure is never
  billed. The load details page shows dwell, free time, billable hours and charge per stop.
- **Alert**: dispatchers are notified once per stop when a truck is still there past its free time
  (checked every 15 minutes).

### Payroll invoices (driver settlements)

- Created for employee pay, by the weekly/monthly generation job or by hand. A settlement has one
  gross-pay line followed by one negative line per deduction, so its total is the net pay.
- Gross pay by salary type: monthly, weekly, hourly (time entries), per distance (delivered km),
  per load (delivered loads x rate), per stop (stops departed in the period x rate) and share of
  gross - the percentage of each delivered load's linehaul plus fuel surcharge plus accessorials,
  minus whatever the employee's pay profile excludes. Weekly generation covers weekly, per-load
  and per-stop pay; monthly covers monthly and share of gross.
- Deductions (owner-operator settlements):
  - Recurring deductions (truck lease, insurance, ELD fee, other) appear on every settlement. An
    escrow deduction is capped so the escrow balance stops at its target; the balance is the sum
    of escrow lines on settlements that were not rejected or cancelled (a positive escrow line is a
    release).
  - Cash advances not yet repaid are deducted on the next settlement. Advances are issued by the
    office; there is no driver request flow.
  - With fuel card deduction on, matched fuel card purchases in the period on trucks the employee
    is the main driver of are deducted.
  - Advances and fuel purchases remember the settlement that deducted them; rejecting or deleting
    that settlement hands them back.
  - One-off deductions and reimbursements (lumper paid by the driver, tolls) are hand-added lines;
    a deduction-type line is always stored negative whatever sign was typed.
- Regenerating a draft (period or employee change) rebuilds the gross line and every generated
  deduction, keeping hand-added lines.
- The pay stub PDF lists earnings, gross, each deduction and the net.

### Subscription invoices

Platform subscription payments, created from Stripe webhooks.

## Invoice Lifecycle

```text
Draft → Issued → PartiallyPaid → Paid
              ↘ Cancelled
```

1. **Draft**: initial state, created with the load
2. **Issued**: sent to the customer, awaiting payment
3. **PartiallyPaid**: some payments received, balance due
4. **Paid**: fully paid
5. **Cancelled**: no further action

## Line Items

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

## Getting Paid

- **Payment links**: a shareable link (30 days by default) lets the customer pay without signing
  in. Opening it checks the tenant, the expiry and the invoice status, then hands off to
  Stripe-hosted checkout on the carrier's Stripe Connect account; the invoice status updates when
  the payment lands. See [customer payments](customer-payments.md).
- **Sending**: sending an invoice creates a payment link if there is none, emails the invoice with
  an optional personal message and records when and to whom it went.
- **Manual payments**: cash and check payments are recorded by hand, with a reference number
  (check number) and the date received.
- **Dashboard**: draft, pending, overdue, partially paid and paid counts and amounts, total
  outstanding, collected this month, and what is owed by customers versus by the factor
  (submitted and awaiting funding, plus reserve held).

## Factoring

Carriers who sell invoices to a factoring company set the factor up under Settings > Factoring and
pick it on each factored customer.

- **Notice of assignment**: a factored customer's invoice PDF prints the factor's remit-to block and
  leaves out the carrier's payment state; the invoice email swaps the payment link for the notice;
  the customer portal shows "Pay to {factor}" and card checkout is refused.
- **Submit** (Load Invoices > Submit to factor): the invoices go on a schedule of accounts emailed
  to the factor's submission address with the schedule PDF, each invoice PDF and the loads' POD,
  BOL and rate confirmation. Each invoice must be issued, sent or overdue with no payments, billed
  to a customer factored with that factor, not already submitted, and have a POD or BOL on its
  load; the error names every invoice that fails.
- **Funding**: per invoice the advance and fee, or a rejection. Advance + fee + reserve always
  equals the invoice total. Funding records a payment for the total, so the invoice reads Paid; the
  reserve stays owed by the factor until it is released.
- **Chargeback**: the sale payment is removed, the invoice goes back to sent/overdue and its
  factoring status becomes charged back. A sale payment already pushed to QuickBooks is voided
  there on the next sync.

An invoice's factoring status is not factored, submitted, funded, rejected or charged back, and the
invoice list filters on it. A schedule's status (submitted, partially funded, funded, rejected) is
derived from its invoices. See [factoring-integration](roadmap/factoring-integration.md) for what
is not built.
