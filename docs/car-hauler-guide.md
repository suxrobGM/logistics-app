# Car Hauler Guide

How a car-hauling carrier sets up and runs vehicle loads, from booking to the signed electronic bill
of lading (eBOL). This is the onboarding checklist for pilot car haulers.

## Before the first load

1. **Turn on vehicle transport.** A platform admin enables `TenantFeature.VehicleTransport` for the
   tenant (the car hauler preset does this). Without it, vehicle loads and car hauler trucks are
   rejected. It also unlocks condition reports, VIN decode and the eBOL on any plan; the rest of
   the Safety area (accidents) still needs `TenantFeature.Safety`.
2. **Add your trucks as car haulers.** Set each truck's type to Car hauler and its **vehicle
   capacity**, the number of cars it carries (for example 7 or 9). Dispatch warns when a truck's
   open loads have more vehicles than this. It warns and never blocks, because what fits depends on
   the mix of sedans and pickups.
3. **Give each customer an email address.** When a load is delivered, the customer gets an email
   with a link to the shipment in the customer portal, where the eBOL can be downloaded.
4. **Invite customers to the customer portal** if they should download BOLs themselves.

## Booking a load

- Create a load of type **Vehicle** and add each vehicle in the **Vehicles** section: VIN, year,
  make, model, color, lot or stock number, operable or not, notes, and an optional per-vehicle rate.
  **Decode VIN** fills in the year, make and model from NHTSA. A vehicle needs at least a VIN or a
  make. A VIN must be 17 characters without I, O or Q.
- **Import from PDF** reads dispatch sheets with several vehicles and creates the load with all of
  them.
- Assign a car hauler truck. The load page and the eligibility check show a warning when the truck
  would be over capacity.

## At pickup (driver app)

1. Open the load and choose **Condition report**. The app lists the load's vehicles.
2. For each vehicle, start a **pickup** inspection and tap the damaged zones on the car diagram.
   For each zone, pick the damage type and a severity, and add a photo if you can. The codes match
   what car hauler BOLs use: **S** scratch, **D** dent, **C** chip, **CR** crack, **M** missing,
   **B** broken.
3. Capture the **BOL**. The shipper prints their name and signs on the phone. If nobody is there,
   choose **Unable to sign** and give the reason (unattended, refused, ...).

## At delivery

1. Inspect each vehicle again with a **delivery** report. The app shows the pickup damage next to
   the diagram, so new damage stands out.
2. Capture the **POD**: the receiver's name and signature, or the unable-to-sign reason.
3. Confirm delivery. The customer gets the delivery email.

## The eBOL

- In the TMS portal, open the load and choose **Download eBOL**. In the customer portal, open the
  shipment.
- The PDF lists every vehicle with its VIN, operable status, lot number and rate. For each vehicle
  it shows the damage diagram and codes at pickup and at delivery, the inspector, time and GPS. It
  ends with the shipper and receiver signatures, or the reason one is missing, with their times and
  GPS.
- The PDF is built when you download it, so it always shows the latest reports and signatures.

## Not included yet

- **Central Dispatch and Super Dispatch** load boards. Neither offers a public carrier API for
  searching or booking listings. See [roadmap/car-hauler-load-boards.md](roadmap/car-hauler-load-boards.md)
  for the access we need.
- The capacity check counts vehicles. It does not account for vehicle size or deck position.
