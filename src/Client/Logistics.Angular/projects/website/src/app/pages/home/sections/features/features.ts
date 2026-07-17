import { Component } from "@angular/core";
import type { IconName } from "@logistics/shared/ui";
import { IconCircle, SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface Feature {
  icon: IconName;
  title: string;
  description: string;
}

@Component({
  selector: "web-features",
  templateUrl: "./features.html",
  imports: [SectionContainer, SectionHeader, IconCircle, ScrollAnimateDirective],
})
export class Features {
  protected readonly highlightedFeatures: Feature[] = [
    {
      icon: "box",
      title: "Load Management",
      description:
        "Create, assign, and track shipments from pickup to delivery. Stops, cargo details, special instructions - all in one place.",
    },
    {
      icon: "map-pin",
      title: "Live GPS Tracking",
      description:
        "See the whole fleet on a live map: driver locations, route lines, geofencing alerts. Updates over SignalR.",
    },
    {
      icon: "credit-card",
      title: "Invoicing & Payments",
      description:
        "Auto-generated invoices, paid through Stripe and Stripe Connect so money goes straight to your bank. Shareable payment links and partial payments.",
    },
    {
      icon: "smartphone",
      title: "Driver Mobile App",
      description:
        "Native Kotlin Multiplatform app for assignments, POD capture, navigation, DVIR inspections, and chat with dispatch.",
    },
    {
      icon: "search",
      title: "Load Board Integration",
      description:
        "Search DAT, Truckstop, and 123Loadboard from one place. Book loads. Post trucks.",
    },
    {
      icon: "shield-check",
      title: "Broker Credit Check",
      description:
        "Credit score, days-to-pay, and FMCSA authority on every listing. Low-credit bookings are blocked before you haul for a broker who won't pay.",
    },
    {
      icon: "globe",
      title: "US and Europe",
      description:
        "Each tenant is provisioned for the US or Europe - address validation, currency (USD or EUR), and Mapbox defaults switch to match.",
    },
  ];

  protected readonly moreFeatures: Feature[] = [
    {
      icon: "navigation",
      title: "Trip Planning & Route Optimization",
      description:
        "Bundle loads into trips with multi-stop routing, driver assignment, and conflict detection.",
    },
    {
      icon: "truck",
      title: "Fleet & Maintenance",
      description:
        "Trucks, trailers, equipment. Track maintenance schedules, registration expiry, and which truck is assigned where.",
    },
    {
      icon: "wallet",
      title: "Payroll & Timesheets",
      description:
        "Pay drivers by miles, percentage, or flat rate. Track timesheets. Generate payroll reports.",
    },
    {
      icon: "receipt",
      title: "Expense Tracking",
      description:
        "Log fleet expenses by category - fuel, tolls, repairs, and so on - and see the breakdown of where the money goes.",
    },
    {
      icon: "refresh-cw",
      title: "QuickBooks Sync",
      description:
        "Push customers, invoices, payments, and expenses to QuickBooks Online so your accountant's books stay current without re-entry.",
    },
    {
      icon: "fuel",
      title: "Fuel Card Sync",
      description:
        "WEX and EFS transactions import nightly as paid fuel expenses - gallons, price, and purchase state included. No manual entry, and IFTA data builds itself.",
    },
    {
      icon: "map",
      title: "IFTA Reporting",
      description:
        "Per-jurisdiction miles from ELD GPS and gallons from fuel cards roll into a quarterly IFTA report with tax due - snapshot-frozen for audits, exportable as PDF or CSV.",
    },
    {
      icon: "clock",
      title: "ELD / HOS",
      description:
        "Hooks into Samsara and Motive for hours-of-service tracking and FMCSA compliance.",
    },
    {
      icon: "shield",
      title: "Safety & Compliance",
      description:
        "Digital DVIR inspections with photo documentation, incident reporting, and safety records you can pull for an audit.",
    },
    {
      icon: "chart-column",
      title: "Reports & Analytics",
      description:
        "Driver performance, revenue, utilization, ops dashboards. Export the data when you want to dig deeper.",
    },
    {
      icon: "user",
      title: "Customer Portal",
      description:
        "Customers can log in any time to track shipments, view invoices, download documents, and pay online.",
    },
    {
      icon: "messages-square",
      title: "Messaging",
      description:
        "In-app chat between dispatchers and drivers. Read receipts, typing indicators, push notifications.",
    },
    {
      icon: "inbox",
      title: "Intermodal Containers",
      description:
        "ISO 6346 tracking across multiple loads. State machine handles the lifecycle: Empty → Loaded → At Port → In Transit → Delivered → Returned.",
    },
    {
      icon: "building-2",
      title: "Terminals & Depots",
      description:
        "UN/LOCODE directory of sea ports, rail terminals, inland depots, air cargo facilities, and border crossings - usable as pickup or drop-off points.",
    },
    {
      icon: "car",
      title: "Any Equipment",
      description:
        "Flatbed, reefer, tanker, box truck, car hauler, container truck, low loader, tautliner, swap body, curtainsider. One install runs them all.",
    },
  ];
}
