# Car Hauler Load Boards (Central Dispatch, Super Dispatch)

- **Status**: Planned (blocked on vendor access)
- **Priority**: P1 - car-hauling pilots find most of their freight on Central Dispatch and Super Dispatch
- **Effort**: M once a vendor grants carrier-side API access
- **Category**: Integrations

## Why

DAT and Truckstop carry little vehicle freight. Car haulers book from Central Dispatch (Cox
Automotive) and the Super Dispatch Super Loadboard. The TMS has a load board provider pattern
(`Infrastructure.Integrations.LoadBoard`, `ILoadBoardProviderService`: search listings, post trucks, market
rates), so the plan was to add one or both as providers.

## What the public docs allow (checked 2026-10-14)

Neither vendor documents a **carrier-side** API for searching the board, booking or requesting a
listing, or posting capacity. Their public APIs cover the shipper side and work on loads that were
already dispatched or offered to the carrier. We did not write a provider: it would have meant
inventing endpoints.

### Central Dispatch

- Docs: <https://api-docs.centraldispatch.com/> (public, no login).
- Auth: OAuth2 client credentials, `POST https://id.centraldispatch.com/connect/token`. The scope
  names are not published.
- **Listings API v2** (`https://marketplace-api.centraldispatch.com`): `POST/GET /listings`,
  `GET/PUT/DELETE /listings/id/{id}`, `GET /listings/mine`. These are for the **shipper**. The only
  filter on `GET /listings` is `partnerReferenceId`, and there is no marketplace search.
- **Fulfillment API** (`https://fulfillment-api.centraldispatch.com`, Premium plan): carriers can
  `POST /api/fulfillments/search` their own dispatches, `POST /api/fulfillments/id/{dispatchId}/respond`
  to accept or decline a dispatch, and `PATCH .../drivers` to assign a driver.
- The test guide says the carrier calls or emails the shipper to agree a load, outside the API.
- No sandbox host is documented. Credentials come by email after a request form.

### Super Dispatch

- Carrier docs: <https://developer.superdispatch.com/carrier/docs/documentation-intro>, OpenAPI at
  <https://developer.superdispatch.com/specs/superdispatch-carrier-public-api.yaml>.
- Auth: OAuth2 client credentials, `POST /oauth/token/` on `https://carrier.superdispatch.com`
  (staging `https://staging.carrier.superdispatch.org`).
- The carrier API syncs the carrier's own TMS: `/v1/orders/` (assign driver, mark picked up or
  delivered), vehicles, BOL, photos, inspections, invoices, webhooks, and `GET /v1/offers/` with
  accept and decline for offers already sent to the carrier.
- The load board endpoints (`api.shipper.superdispatch.com/v1/public/loadboard/orders/...`) are
  shipper-only. Searching and booking are done in the Super Dispatch app.
- The API is a paid add-on, and support issues the credentials.

## What access we need

To build a real `ILoadBoardProviderService` for either vendor, we need written confirmation from the
vendor, and credentials, for:

1. A carrier-scoped search of open vehicle listings: origin and destination radius, dates, trailer
   type (open or enclosed), and inoperable units.
2. A listing's details: vehicles (VIN, year, make, model, operable), stops, price and payment terms,
   and the shipper's contact.
3. A way to request or book a listing, or the confirmed answer that booking stays off-API.
4. Optionally, posting truck capacity.

Ask Cox Automotive (DataSyndicationSupport@centraldispatch.com) and Super Dispatch support whether
a TMS partner program exposes these.

## Smaller integrations we can build now

These use documented carrier endpoints, but they are not load board searches:

- **Super Dispatch offers**: poll `GET /v1/offers/`, show pending offers in the TMS, and accept or
  decline them. An accepted offer becomes a vehicle load with its `vehicles[]` (`vin`, `year`,
  `make`, `model`, `color`, `lot_number`, `is_inoperable`, `price`).
- **Central Dispatch dispatches**: pull the carrier's `NOT_SIGNED` dispatches with
  `POST /api/fulfillments/search`, accept or decline them with `respond`, and create the load.

Both need the tenant's own API subscription with that vendor.

## Notes

- **2026-10-14** - Researched both public portals for the car hauler pilot. Neither has a carrier
  load board API, so we wrote no provider code. The multi-vehicle load, condition reports and eBOL
  shipped instead (see [car-hauler-guide.md](../car-hauler-guide.md)).
