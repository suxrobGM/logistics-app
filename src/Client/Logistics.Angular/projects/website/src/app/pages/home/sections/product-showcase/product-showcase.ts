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
      title: "TMS Dashboard",
      description:
        "Get a bird's-eye view of your entire operation with real-time metrics, fleet maps, active loads, and financial health — all on one screen.",
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
      features: ["Live fleet map with GPS tracking", "Revenue & performance metrics", "Driver stats & top performers"],
      icon: "pi-chart-bar",
    },
    {
      title: "Load Management",
      description:
        "Create, assign, and track loads from pickup to delivery. Filter by status, customer, truck, or date range with full visibility at every step.",
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
      features: ["Advanced filtering & search", "Status tracking with delivery workflow", "Detailed load view with assignment"],
      icon: "pi-box",
    },
    {
      title: "Trips & Route Optimization",
      description:
        "Plan efficient trips with multi-stop routing, assign trucks and drivers, and visualize routes on an interactive map — maximizing your fleet's productivity.",
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
      features: ["Multi-stop route planning", "Interactive map with stop visualization", "Truck & driver assignment per trip"],
      icon: "pi-map",
    },
    {
      title: "Fleet Overview",
      description:
        "Monitor your trucks, drivers, license plates, and availability in one place. Quickly see which trucks are available and who's assigned to them.",
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
      features: ["Vehicle & driver assignments", "Availability tracking", "Comprehensive reports & analytics"],
      icon: "pi-truck",
    },
    {
      title: "Accounting",
      description:
        "Manage load invoices, payroll, and payments in one place. Track outstanding balances, approve payroll, and keep your finances organized.",
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
      features: ["Load invoice management & tracking", "Payroll processing & approval", "Payment recording & history"],
      icon: "pi-wallet",
    },
    {
      title: "Customer Portal",
      description:
        "Give your customers 24/7 self-service access to track shipments, view invoices, and download delivery documents — no phone calls needed.",
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
      features: ["Real-time shipment tracking", "Invoice & document access", "Clean self-service experience"],
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
