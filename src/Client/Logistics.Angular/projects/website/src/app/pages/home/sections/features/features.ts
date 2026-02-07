import { Component } from "@angular/core";
import { IconCircle, SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface Feature {
  icon: string;
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
      icon: "pi-box",
      title: "Load Management",
      description:
        "Create, assign, and track shipments from pickup to delivery. Manage stops, cargo details, and special instructions.",
    },
    {
      icon: "pi-map-marker",
      title: "Real-Time GPS Tracking",
      description:
        "Track your entire fleet on a live map with driver locations, route visualization, and geofencing alerts via SignalR.",
    },
    {
      icon: "pi-credit-card",
      title: "Invoicing & Payments",
      description:
        "Automated invoicing with Stripe and Stripe Connect for direct bank deposits. Shareable payment links and partial payments.",
    },
    {
      icon: "pi-mobile",
      title: "Driver Mobile App",
      description:
        "Native Kotlin Multiplatform app for assignments, POD capture, navigation, inspections, and real-time communication.",
    },
  ];

  protected readonly moreFeatures: Feature[] = [
    {
      icon: "pi-directions",
      title: "Trip Planning & Route Optimization",
      description:
        "Organize loads into optimized trips with multi-stop routing, driver assignment, and automatic conflict detection.",
    },
    {
      icon: "pi-truck",
      title: "Fleet & Maintenance",
      description:
        "Manage trucks, trailers, and equipment. Track maintenance schedules, registration expiry, and vehicle assignments.",
    },
    {
      icon: "pi-search",
      title: "Load Board Integration",
      description:
        "Search freight across DAT, Truckstop, and 123Loadboard from one interface. Book loads and post available trucks.",
    },
    {
      icon: "pi-wallet",
      title: "Payroll & Timesheets",
      description:
        "Calculate driver pay by miles, percentage, or flat rate. Track timesheets and generate payroll reports.",
    },
    {
      icon: "pi-receipt",
      title: "Expense Tracking",
      description:
        "Record and categorize fleet expenses — fuel, tolls, repairs, and more. Monitor spending with detailed breakdowns.",
    },
    {
      icon: "pi-clock",
      title: "ELD / HOS Compliance",
      description:
        "Integrated with Samsara and Motive for automatic hours of service tracking and FMCSA compliance.",
    },
    {
      icon: "pi-shield",
      title: "Safety & Compliance",
      description:
        "Digital DVIR inspections with photo documentation, incident reporting, and safety tracking for compliance audits.",
    },
    {
      icon: "pi-chart-bar",
      title: "Reports & Analytics",
      description:
        "Driver performance, revenue, fleet utilization, and operational dashboards. Export data for informed decision-making.",
    },
    {
      icon: "pi-user",
      title: "Customer Self-Service Portal",
      description:
        "24/7 access for customers to track shipments, view invoices, download documents, and make payments online.",
    },
    {
      icon: "pi-comments",
      title: "Real-Time Messaging",
      description:
        "Built-in chat between dispatchers and drivers with read receipts, typing indicators, and push notifications.",
    },
  ];
}
