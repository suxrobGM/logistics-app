# Connecting Samsara

What a tenant does to get Samsara hours of service, violations and truck GPS into LogisticsX.
It takes about ten minutes and needs a Samsara Full Admin (or a role that can manage API tokens
and webhooks).

## What you get

| Data             | How it arrives                                  | How often      |
| ---------------- | ----------------------------------------------- | -------------- |
| HOS clocks       | `GET /fleet/hos/clocks`                         | every 5 min    |
| Truck GPS        | `GET /fleet/vehicles/stats` (GPS + odometer)    | every 5 min    |
| HOS logs         | `GET /fleet/hos/logs`, last 8 days re-read      | every 30 min   |
| HOS violations   | `GET /fleet/hos/violations`, last 8 days        | every 30 min   |
| Violation alerts | Webhook (`AlertIncident` with an HOS violation) | as they happen |

Truck positions feed the live map and the IFTA mileage per state, so map your trucks as well as
your drivers. Only mapped drivers and trucks are synced.

## 1. Find your region

Samsara hosts each organization in one data region (US, EU or Canada) and a token only works
against its own region's API (`api.samsara.com`, `api.eu.samsara.com`, `api.ca.samsara.com`). You
pick the region when you connect. Most North American fleets are US; a wrong region simply fails
the credential check, so you can try again with the other one.

## 2. Create an API token

In Samsara go to **Settings → API Tokens → Add API Token**, give it a name such as "LogisticsX",
and grant these read scopes (nothing needs write access):

| Scope                                          | Used for                        |
| ---------------------------------------------- | ------------------------------- |
| Read Drivers (Drivers)                         | driver list for mapping         |
| Read Vehicles (Vehicles)                       | vehicle list for mapping        |
| Read Vehicle Statistics (Vehicles)             | GPS position and odometer       |
| Read ELD Compliance Settings (US) (Compliance) | HOS clocks, logs and violations |

Copy the token; Samsara shows it once.

## 3. Connect it in LogisticsX

**ELD → Providers → Add Provider**, choose **Samsara**, pick the region and paste the token as the
API key. The dialog shows your webhook URL:

```
https://<your API host>/webhooks/eld/samsara/<your tenant id>
```

Leave the dialog open and do step 4 first if you want webhooks, so you can paste the secret in
the same go. LogisticsX checks the token against Samsara before saving.

## 4. Add the webhook (optional, for instant violation alerts)

Polling already picks violations up within 30 minutes. For alerts as they happen:

1. In Samsara go to **Settings → Webhooks → Add Webhook**, name it, and paste the webhook URL from
   step 3. Samsara requires HTTPS.
2. Copy the webhook's **Secret Key** into the **Webhook Secret** field in LogisticsX.
3. Create an alert for HOS violations and pick this webhook as its notification. Samsara then posts
   an `AlertIncident` event for each violation, and again when it resolves.
4. Optionally subscribe the webhook to `DriverCreated`, `DriverUpdated`, `VehicleCreated` and
   `VehicleUpdated`. They are accepted and acknowledged but do not change anything yet; new
   drivers and vehicles show up the next time you open the mappings page.

Use the webhook's **Test** button: the `Ping` it sends should come back as delivered.

Every delivery is checked against the secret. Samsara signs `v1:<X-Samsara-Timestamp>:<body>` with
the base64-decoded secret, and a delivery older than five minutes is rejected, so a captured
request cannot be replayed later. A webhook without a secret in LogisticsX is rejected outright;
the providers table marks it "No secret".

## 5. Map drivers and trucks

Open the provider's mappings (the people icon on the providers table). LogisticsX suggests
matches it is sure about:

- drivers by email, then license number, then full name
- trucks by VIN, then by truck number (a Samsara vehicle named "Truck #101" matches truck `101`)

A key that appears twice on either side (two "John Smith"s, one VIN on two trucks) is never
suggested. Review the list, click **Apply suggested matches**, then map whatever is left by hand.

## Troubleshooting

| Symptom                                  | Likely cause                                                             |
| ---------------------------------------- | ------------------------------------------------------------------------ |
| "Invalid API credentials" on connect     | Wrong region, or the token lacks Read Drivers                            |
| Drivers listed but no HOS clocks         | Token lacks Read ELD Compliance Settings (US), or the driver is unmapped |
| Trucks never move on the map             | Token lacks Read Vehicle Statistics, or the truck is unmapped            |
| Samsara shows webhook deliveries failing | Secret not entered or mistyped; delete and re-add the provider with it   |
| A driver shows no status for a while     | Samsara reports an empty status while the driver app is disconnected     |
