# Commercial Licensing

LogisticsX is published under the [PolyForm Noncommercial License 1.0.0](LICENSE). Personal projects, research, education, and evaluation are free. Any commercial use needs a commercial license from the author. This page summarizes the options. The signed agreement governs; this page is informational.

Commercial use includes running LogisticsX inside a for-profit company, hosting it for others, and selling a product built on it.

## License types

| Type              | Who it is for                                                                       | What you get                                                             | Not included                                                 |
| ----------------- | ----------------------------------------------------------------------------------- | ------------------------------------------------------------------------ | ------------------------------------------------------------ |
| Internal Use      | A carrier, broker, or logistics company that runs LogisticsX for its own operations | Run any number of instances for one legal entity                         | Offering the product to third parties                        |
| Hosted / Reseller | A company that hosts LogisticsX and sells access to other businesses                | Run it as a service for up to the number of tenants written into the key | Redistributing the source, sublicensing                      |
| Perpetual Source  | A company that forks LogisticsX and owns its copy                                   | A perpetual right to use and modify the source as of the purchase date   | Updates after the first year, unless the update plan is paid |

## Pricing

Prices live on [logisticsx.app/license](https://logisticsx.app/license), including the support and name-license add-ons. They are starting points: volume, term length, and support needs change the quote.

Hosted / Reseller keys carry the tenant cap. Growing past it means moving to the next band; the difference is charged for the rest of the year and a new key is issued. The daily heartbeat reports the tenant count, so no revenue reporting is needed.

## Common terms

1. No sublicensing or resale of the source code.
2. The LogisticsX name and logo are not included. A reseller must rebrand unless they buy the name license.
3. No warranty. Liability is capped at the fees paid in the previous 12 months.
4. Support is a separate add-on.
5. Hosted / Reseller: the tenant count reported by the heartbeat must stay within the cap in the key. The author may verify it once a year with 30 days notice.
6. The licensee agrees not to remove the license check, the noncommercial notice, or the instance heartbeat.
7. Non-payment ends the license after 30 days notice. Rights revert to the noncommercial license.

## How the license key works

A license key is a signed token issued by the author for one legal entity. A SuperAdmin installs it on the admin portal License page, or the operator sets the `License__Key` environment variable. Until a valid key is installed, every portal shows a one-line noncommercial notice. Nothing else changes: no feature is locked, and an expired key only brings the notice back.

Removing, disabling, or working around the key check, the notice, or the heartbeat grants no commercial rights. Commercial use without a valid key stays unlicensed, and bypassing the check on purpose is treated as willful infringement. The `LICENSE` file carries this as a Required Notice, so every copy of the source keeps it.

## What the heartbeat sends

Once a day each deployment posts a small report to `https://api.logisticsx.app/license/heartbeat`. It contains exactly these fields:

- a random instance id generated on first start
- the server hostname
- the product version
- the license key id and licensee name, when a valid key is installed
- the number of tenants

No tenant data, user data, or business data is sent. Operators can turn the heartbeat off with `License__HeartbeatEnabled=false`. Under a commercial license it stays on.

## How to buy

Email [suxrobgm@gmail.com](mailto:suxrobgm@gmail.com) with your company name, the license type, and your fleet or tenant count. You receive a quote, an agreement to sign, and an invoice. The key is issued when the first payment clears.
