# Connecting QuickBooks Online

What it takes to get LogisticsX customers, invoices, payments, expenses and driver pay into a
carrier's QuickBooks Online company. The first part is a one-time job for whoever runs the
LogisticsX deployment (an Intuit developer app). The second part is what each tenant does, which
takes about five minutes and needs a QuickBooks admin.

## What gets pushed

The sync is one-way: LogisticsX is the source of truth and nothing is read back except the lists
you map against. It runs every 15 minutes, and **Sync now** on the settings card queues a run
straight away.

| LogisticsX                                 | QuickBooks Online                                                                                    | When                                      |
| ------------------------------------------ | ---------------------------------------------------------------------------------------------------- | ----------------------------------------- |
| Active customer                            | Customer (linked by display name if it exists)                                                       | created or edited                         |
| Customer made inactive or deleted          | Customer marked inactive                                                                             | once                                      |
| Load invoice (not draft)                   | Invoice, one line per invoice line, with item                                                        | created or edited                         |
| Load invoice cancelled after it was pushed | Invoice voided                                                                                       | once                                      |
| Paid payment on a pushed invoice           | Payment applied to that invoice                                                                      | created or edited                         |
| Approved or paid expense                   | Expense (Purchase), payee vendor by vendor name                                                      | created or edited                         |
| Approved or paid payroll invoice           | Bill owed to the driver, who becomes a vendor                                                        | created or edited, if an account is set   |
| Factored invoice bought by the factor      | Payment of the full invoice into the clearing account, and the factor fee as an Expense paid from it | funded, if the factoring accounts are set |

Only records changed since their last push are read, at most 500 of each kind per run, so a
large first sync spreads over a few runs. Invoices, payments, expenses and driver pay dated before
**Sync transactions from** are never pushed. It defaults to the day you first connect, so history
you already keyed into QuickBooks is not duplicated; move it back to backfill.

## Part 1: the Intuit app (once per deployment)

### Create the app

1. Sign in at [developer.intuit.com](https://developer.intuit.com), verify the email and complete
   the developer profile (Intuit will not issue production keys until both are done).
2. **Dashboard → Create an app → QuickBooks Online and Payments**, and select only the
   **Accounting** scope (`com.intuit.quickbooks.accounting`). LogisticsX does not use Payments or
   OpenID.
3. Under **Keys & credentials → Development**, add the redirect URI
   `https://<your API host>/accounting/quickbooks/callback` (for local work,
   `http://localhost:7000/accounting/quickbooks/callback`; Intuit allows plain HTTP only for
   localhost in development).

### Configure the API

The `Accounting:QuickBooks` section of the API settings, or the matching environment variables:

| Setting                                   | Value                                                  |
| ----------------------------------------- | ------------------------------------------------------ |
| `Accounting__QuickBooks__ClientId`        | the app's client id                                    |
| `Accounting__QuickBooks__ClientSecret`    | the app's client secret (a secret; keep it out of git) |
| `Accounting__QuickBooks__Environment`     | `sandbox` or `production`                              |
| `Accounting__QuickBooks__RedirectUri`     | exactly the redirect URI registered with Intuit        |
| `Accounting__QuickBooks__PortalReturnUrl` | `https://<your TMS host>/settings/integrations`        |
| `Accounting__QuickBooks__Scopes`          | leave the default, `com.intuit.quickbooks.accounting`  |

`Environment` is install-wide: it picks the Data API host (`sandbox-quickbooks.api.intuit.com` or
`quickbooks.api.intuit.com`) for every tenant. Development keys only work against sandbox
companies and production keys only against real ones, so a deployment uses one or the other -
there is no per-tenant switch. Test end to end against a sandbox company (Intuit creates one for
every developer account) before switching a deployment to production keys.

### Production checklist

Intuit reviews an app before it issues production keys. Fill in **Keys & credentials →
Production** and the app's settings pages:

- [ ] **Redirect URI**: `https://<your API host>/accounting/quickbooks/callback`. HTTPS only, no IP
      address, no fragment, and it must match `RedirectUri` character for character.
- [ ] **Host domain**: your API host.
- [ ] **Launch URL**: `https://<your TMS host>/settings/integrations` (where a user lands from the
      QuickBooks app list; they sign in to LogisticsX and press Connect).
- [ ] **Disconnect URL**: `https://<your API host>/accounting/quickbooks/disconnected`. Intuit sends
      the browser here after a user disconnects LogisticsX from inside QuickBooks. The page sends
      them back to the TMS settings; the revoked token is detected on the next sync and the card
      asks the tenant to reconnect.
- [ ] **Connect/Reconnect URL** (if asked): the same TMS settings page.
- [ ] **End-user license agreement URL** and **privacy policy URL**: public pages on your website.
      Intuit will not issue production keys without both.
- [ ] **Scopes**: Accounting only.
- [ ] **App assessment questionnaire**: answer the security and data questions. What LogisticsX
      does, for reference: OAuth tokens are stored encrypted at rest (ASP.NET Core Data
      Protection), only the accounting scope is requested, tokens are revoked on disconnect, data
      flows one way from LogisticsX to QuickBooks, every request is logged with Intuit's
      `intuit_tid` on failure, and the API is served over HTTPS only.
- [ ] Copy the production client id and secret into the production configuration and set
      `Environment` to `production`.

## Part 2: connecting a tenant

The tenant's plan must include QuickBooks sync (Professional and up), and the user needs the
**Accounting: Manage** permission (Owner and Manager by default).

1. **Settings → Integrations → Connect to QuickBooks.** Intuit asks the user to sign in, pick the
   company and approve access. The link is valid for 10 minutes; if it expires, press Connect
   again. Back in LogisticsX a toast confirms the connection.
2. On connect, LogisticsX picks starting choices from the company: the first bank account to pay
   expenses from, the first expense account, and the "Services" item (or the first service item)
   for invoice lines.
3. Review the settings on the card and **Save settings**:
   - **Default item** and one item per invoice line type (base rate, fuel surcharge, detention,
     layover, lumper, accessorial, other). Most carriers create service items such as "Freight",
     "Fuel Surcharge" and "Detention" in QuickBooks first, each pointing at the right income account.
   - **Deposit payments to**: a bank account, or empty for Undeposited Funds (what QuickBooks
     itself defaults to, so payments can be grouped into bank deposits).
   - **Paid from** and **Default expense account**: both are needed for expenses to sync. A credit
     card account records expenses as card charges, a bank account as cash/debit payments.
   - **Truck and company expense categories**: map fuel, tolls, tires, maintenance, insurance and
     so on to their own expense accounts; unmapped categories use the default.
   - **Driver pay expense account**: set it to push approved payroll as bills owed to each driver
     (added as a QuickBooks vendor). Leave it empty if payroll runs through a payroll service.
   - **Factoring clearing account** and **Factoring fee expense account**: for carriers who sell
     invoices to a factor. Create a bank-type account such as "Factoring Clearing" first. When the
     factor funds an invoice, the invoice is paid in full into that account and the fee is posted as
     an expense out of it; record the advance and, later, the reserve as transfers from it to your
     operating bank when they land. Without the clearing account, sale payments of factored
     invoices show under Sync errors instead of landing in Undeposited Funds.
   - **Sync transactions from**: see above.
4. Press **Sync now**, or wait for the next 15-minute run.

Changing a setting applies to records pushed from then on; already-pushed records are not
re-posted until they are edited.

## When something fails

Records QuickBooks refuses appear under **Sync errors** with QuickBooks' own message, for example
a duplicate display name (QuickBooks display names are unique across customers, vendors and
employees) or an item that was deleted. Fix the cause, then press **Retry** on the row or
**Retry all**. A failed record that is not retried by hand is tried again an hour later, not on
every run.

- **An invoice or payment says its customer is not in QuickBooks yet**: fix the customer's error
  first; the invoice follows on the next run.
- **A record was edited in QuickBooks**: LogisticsX re-reads it and overwrites the fields it owns.
  Edit LogisticsX, not QuickBooks, for anything that syncs.
- **"Reconnect needed"**: QuickBooks stopped accepting the connection - someone disconnected
  LogisticsX inside QuickBooks, or the refresh token lapsed (it expires after 100 days without a
  sync). Sync is paused; press **Reconnect** and pick the same company to continue where it left
  off.
- **Connecting a different company** clears every item and account choice and forgets which
  records were pushed, so the next sync pushes into the new company from scratch. Check **Sync
  transactions from** before pressing Sync now.
- **The plan was downgraded**: sync stops; the card stays visible so the connection can still be
  disconnected.

For Intuit support, quote the `intuit_tid` shown at the end of the error; it identifies the exact
request on Intuit's side.

## Not covered yet

- QuickBooks Desktop (only QuickBooks Online is supported).
- Sales tax codes: invoice tax is sent as a total, so companies using QuickBooks automated sales
  tax may see QuickBooks recalculate it. Freight is usually not taxable, so most carriers send none.
- Factoring: the advance and reserve transfers are left to the bookkeeper, and a chargeback is not
  pushed - reverse the sale payment in QuickBooks by hand.
- Payments recorded in QuickBooks do not flow back to LogisticsX, and paid driver pay bills are
  not marked paid in QuickBooks.
