import { Component, signal } from "@angular/core";
import { BrowserFrame, SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface Screenshot {
  src: string;
  alt: string;
  label: string;
}

interface ShowcaseItem {
  title: string;
  description: string;
  screenshots: Screenshot[];
  features: string[];
  icon: string;
}

@Component({
  selector: "web-product-showcase",
  templateUrl: "./product-showcase.html",
  imports: [SectionContainer, SectionHeader, BrowserFrame, ScrollAnimateDirective],
})
export class ProductShowcase {
  protected readonly activeTab = signal(0);
  protected readonly activeScreenshot = signal(0);

  protected readonly items: ShowcaseItem[] = [
    {
      title: "AI Dispatch",
      description:
        "Watch the agent read your fleet, match loads to trucks, run compliance checks, and plan routes - live, with the reasoning behind every decision visible.",
      screenshots: [
        {
          src: "images/screenshots/ai-dispatch-sessions.png",
          alt: "AI Dispatch sessions list with fleet map and pending decision cards",
          label: "Sessions & Decisions",
        },
        {
          src: "images/screenshots/ai-dispatch-session-detail.png",
          alt: "AI Dispatch session detail with summary, assignments, and reasoning",
          label: "Session Summary",
        },
        {
          src: "images/screenshots/ai-dispatch-agent-timeline.png",
          alt: "AI Dispatch agent timeline with tool calls, HOS checks, and decisions",
          label: "Agent Timeline",
        },
      ],
      features: [
        "Autonomous or human-in-the-loop",
        "Agent reasoning timeline",
        "Approve or reject each suggestion",
      ],
      icon: "pi-sparkles",
    },
    {
      title: "TMS Dashboard",
      description:
        "One screen for the whole operation - live metrics, fleet map, active loads, and the financial state of the business.",
      screenshots: [
        {
          src: "images/screenshots/tms-dashboard.png",
          alt: "TMS Dashboard showing fleet overview, revenue metrics, and active loads",
          label: "Dashboard Overview",
        },
        {
          src: "images/screenshots/tms-drivers-report.png",
          alt: "Drivers report dashboard with performance stats and charts",
          label: "Drivers Report",
        },
      ],
      features: [
        "Live fleet map with GPS",
        "Revenue and performance metrics",
        "Driver stats and top performers",
      ],
      icon: "pi-chart-bar",
    },
    {
      title: "Load Management",
      description:
        "Create, assign, and track loads from pickup to delivery. Filter by status, customer, truck, or date range - you can see where each load is at any point.",
      screenshots: [
        {
          src: "images/screenshots/tms-loads.png",
          alt: "Load management list view with filters and status tracking",
          label: "Loads List",
        },
        {
          src: "images/screenshots/tms-load-details.png",
          alt: "Load details page with status stepper, info, and assignment",
          label: "Load Details",
        },
      ],
      features: [
        "Filtering and search",
        "Status tracking with delivery workflow",
        "Load detail view with assignment",
      ],
      icon: "pi-box",
    },
    {
      title: "Trips & Route Optimization",
      description:
        "Plan trips with multi-stop routing, assign trucks and drivers, and see the route on an interactive map.",
      screenshots: [
        {
          src: "images/screenshots/tms-trips.png",
          alt: "Trips list view with status, routes, and truck assignments",
          label: "Trips List",
        },
        {
          src: "images/screenshots/tms-trip-details.png",
          alt: "Trip details with interactive map, stops timeline, and assignment",
          label: "Trip Details",
        },
      ],
      features: [
        "Multi-stop route planning",
        "Interactive map with stop visualization",
        "Truck & driver assignment per trip",
      ],
      icon: "pi-map",
    },
    {
      title: "Fleet Overview",
      description:
        "Trucks, drivers, license plates, and availability in one place. See which trucks are free and who's driving them.",
      screenshots: [
        {
          src: "images/screenshots/tms-fleet.png",
          alt: "Fleet management view showing trucks, drivers, and availability",
          label: "Fleet List",
        },
        {
          src: "images/screenshots/tms-reports.png",
          alt: "Reports dashboard with charts, metrics, and performance analytics",
          label: "Reports & Analytics",
        },
      ],
      features: [
        "Vehicle and driver assignments",
        "Availability tracking",
        "Reports and analytics",
      ],
      icon: "pi-truck",
    },
    {
      title: "Accounting",
      description:
        "Load invoices, payroll, and payments in one place. Track what's outstanding, approve payroll, and keep the books in order.",
      screenshots: [
        {
          src: "images/screenshots/tms-invoice-dashboard.png",
          alt: "Load Invoice Dashboard with stats, quick actions, and recent invoices",
          label: "Invoice Dashboard",
        },
        {
          src: "images/screenshots/tms-payroll.png",
          alt: "Payroll dashboard with recent payrolls and employee payments",
          label: "Payroll",
        },
      ],
      features: [
        "Load invoice tracking",
        "Payroll processing and approval",
        "Payment recording and history",
      ],
      icon: "pi-wallet",
    },
    {
      title: "Customer Portal",
      description:
        "Customers can log in any time to track shipments, see invoices, and download delivery docs. No phone calls.",
      screenshots: [
        {
          src: "images/screenshots/customer-dashboard.png",
          alt: "Customer portal dashboard with shipment tracking and invoice access",
          label: "Customer Dashboard",
        },
        {
          src: "images/screenshots/customer-shipment.png",
          alt: "Customer shipment tracking view with delivery status",
          label: "Shipment Tracking",
        },
      ],
      features: ["Live shipment tracking", "Invoice and document access", "Self-service login"],
      icon: "pi-users",
    },
  ];

  protected setActiveTab(index: number): void {
    this.activeTab.set(index);
    this.activeScreenshot.set(0);
  }

  protected setActiveScreenshot(index: number): void {
    this.activeScreenshot.set(index);
  }
}
