# DAT load board setup

How a carrier connects DAT One to LogisticsX, what works once it is connected, and which parts of
DAT's API we could verify. DAT's developer portal (developer.dat.com) needs a login, so the API
shapes here come from public client code. The gaps are listed at the end.

## What the carrier needs

1. **A DAT One plan that includes API access.** A standard DAT One subscription does not include
   it. Ask your DAT account rep to add API access (DAT calls it the "Loadboard" interface).
   - **DAT market rates** (RateView) are a separate DAT product. Third-party summaries say it comes
     with the Combo Pro and Combo Premium plans; confirm with DAT. Leave market rates off unless
     the plan includes RateView, because every lookup fails without it.
2. **A DAT service account.** This is an API-only login that DAT creates for your company. Email
   DAT and include:
   - your company name, and a main contact's name and phone number
   - your MC number or company address, so DAT can identify you
   - the interface you want: Loadboard, and Rating if you have RateView
   - **an email address for the service account that is not already a DAT One user**, for example
     `dat-api@yourcompany.com`

   TMS vendors send these requests to `techsupportteamleads@dat.com`. For developer-portal access
   and API questions, write to `developersupport@dat.com`. DAT then sends a password-reset email
   to the service account address. Set the password there.
3. **A DAT One user email.** DAT issues API tokens per DAT One seat: searches and truck posts run
   as that user, and brokers see that user's contact details. Use the seat of the dispatcher who
   takes broker calls.

## Connecting in LogisticsX

Go to **Load Board → Providers → Add Provider → DAT** and enter:

| Field                        | Value                                                      |
| ---------------------------- | ---------------------------------------------------------- |
| Service Account Email        | the service account from step 2                            |
| Service Account Password     | its password                                               |
| DAT One User Email           | the seat from step 3                                       |
| Show DAT market rates        | only if your plan includes RateView                        |
| DOT / MC Number (optional)   | your own numbers, for reference                            |

Saving signs in to DAT right away and refuses the connection if DAT rejects it, showing DAT's own
error text. Later you can **Test connection**, **Edit** (a blank password keeps the stored one),
or **Disable** the connection from the providers table.

The password and every token are encrypted at rest (`[EncryptedSecret]`). The service account
email and the DAT One user email are not secrets: the edit dialog shows them.

## What works

- **Search.** Search by origin city and state, deadhead radius, an optional destination, pickup
  dates and equipment. DAT has no weight, length or state-only destination filter, so we apply
  those to the matches after DAT returns them. Results show the broker, their DAT credit score and
  days-to-pay, the posted rate, and, when enabled, the **DAT market rate** for the lane.
- **Errors.** When DAT fails, search results show DAT's message for that provider instead of an
  empty list. An expired or revoked token gets one automatic re-authentication before the call
  fails. A `429` is retried with back-off (`RateLimitRetryHandler`).
- **Creating a load from a listing.** DAT has **no booking API**. **Create Load** adds the load to
  LogisticsX with the truck assigned and shows the broker's phone and email. Call or email the
  broker to confirm. The broker credit gate still applies.
- **Truck posts.** Post a truck with its origin, an optional destination, an availability window
  and equipment. You can update or remove a post, and removing it here takes it off DAT. Every 30
  minutes `LoadBoardSyncJob` checks the posts against DAT: a post DAT no longer lists is marked
  expired, and the post's expiry is copied from DAT.
- **Not supported:** load posting (that is a broker workflow) and DAT webhooks (DAT publishes
  none, so everything is pulled).

## Equipment

The UI offers one list (`LoadBoardEquipmentType`). `Providers/Dat/DatEquipment.cs` is the only map
to DAT's codes, taken from DAT's published list (one.support.dat.com, "Equipment Types"). Van `V`,
Reefer `R`, Flatbed `F`, Step Deck `SD`, Double Drop `DD`, Lowboy `LB`, RGN `RG`, Conestoga `CN`,
Hotshot `FH`, Power Only `PO`, Auto Carrier `AC`, Container `C`/`CI`/`CR`, Tanker `TS`/`TA`/`TN`,
Hopper `HB`, Dump `DT`, Box Truck `SB`, Flatbed or Step Deck `FD`. Variant codes on listings fold
onto the nearest type (`VZ` hazmat van → Van, `FT` flatbed with tarps → Flatbed). A code we do not
know shows as "-" rather than as a guess. RateView prices only `VAN`, `REEFER` and `FLATBED`.

## API reference and what is verified

| Call                     | Endpoint                                                              | Status                                                                 |
| ------------------------ | --------------------------------------------------------------------- | ---------------------------------------------------------------------- |
| Organization token       | `POST identity.api.dat.com/access/v1/token/organization` `{username, password}` → `{accessToken, expiresWhen}` | Verified (public client code) |
| User token               | `POST identity.api.dat.com/access/v1/token/user`, Bearer org token, `{username: <DAT One email>}` | Verified |
| `expiresWhen` format     | ISO-8601 UTC with milliseconds                                        | Verified                                                               |
| Search                   | `POST freight.api.prod.dat.com/search/v1/loads` `{criteria:{desiredSpecs, maxAgeInMinutes, availability, origin:{point}, destination}}` → `{id}`, then `GET .../search/v1/loads/{id}/matches?limit=` | Verified against a captured 2021 production response. DAT now documents a v3 search whose shape is not public |
| Match fields             | `matchId, postingId, origin/destination.point, tripLength, specs, availability, rate{rateBasedOn, baseRate}, poster{company, contact}, credit{score, daysToPay}` | Verified |
| Load posting (unused)    | `POST/GET/PATCH/DELETE freight.api.dat.com/posting/v2/loads` `{freight, lane, exposure}` | Verified, not used |
| Truck posting            | `/posting/v2/trucks` with the same `{freight, lane, exposure}` body; delete as `?id=`; list returns an array of `{id, exposure}` | **Assumed** to mirror loads |
| Post expiry              | `exposure.endWhen`                                                    | **Assumed** field name                                                 |
| Market rates             | `POST analytics.api.dat.com/linehaulrates/v1/lookups`, array of `{origin, destination, rateType, equipment, targetEscalation, rateTimePeriod}` → `rateResponses[i].response.rate.perMile/perTrip` | Verified. Index alignment of a batched request is assumed |
| Webhooks, booking        | -                                                                     | None published                                                         |

Hosts are in `appsettings.json` under `LoadBoard:Dat` (`IdentityBaseUrl`, `FreightBaseUrl`,
`SearchBaseUrl`, `AnalyticsBaseUrl`). For DAT's staging environment, insert `.staging`
(`identity.api.staging.dat.com`, `analytics.api.staging.dat.com`). Once you have portal access,
check the assumed rows first. They are isolated in `Providers/Dat/DatModels.cs` and
`DatLoadBoardService.cs`.

## Other load boards

Truckstop and 123Loadboard are **not available yet**. They cannot be picked when adding a
provider, and an old connection shows "not available yet" instead of searching.

- **Truckstop** publishes docs at developer.truckstop.com. It needs a signed Systems Integration
  Agreement (TSI@Truckstop.com), and its load search is a SOAP service.
- **123Loadboard** keeps its API docs behind a partner login
  (partner-integrations@123loadboard.com).
