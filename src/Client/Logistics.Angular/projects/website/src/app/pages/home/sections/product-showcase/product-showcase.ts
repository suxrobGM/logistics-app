import { Component, signal } from "@angular/core";
import { Icon, type IconName } from "@logistics/shared/ui";
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
  icon: IconName;
}

@Component({
  selector: "web-product-showcase",
  templateUrl: "./product-showcase.html",
  imports: [BrowserFrame, Icon, ScrollAnimateDirective, SectionContainer, SectionHeader],
})
export class ProductShowcase {
  protected readonly activeTab = signal(0);
  protected readonly activeScreenshot = signal(0);

  protected readonly items: ShowcaseItem[] = [
    {
      title: "AI Dispatch",
      description:
        "Ask the agent to plan the day. It reads your unassigned loads, checks each driver's hours, and comes back with load-truck pairings - showing every tool it called and the reasoning behind each one. Nothing moves until you approve it.",
      screenshots: [
        {
          src: "images/screenshots/ai-dispatch-timeline.png",
          alt: "AI Dispatch chat showing the tools the agent called and two suggested assignments",
          label: "Agent Timeline",
        },
        {
          src: "images/screenshots/ai-dispatch-chat.png",
          alt: "AI Dispatch plan with an assignments table, open issues, and recommendations",
          label: "Dispatch Plan",
        },
        {
          src: "images/screenshots/ai-dispatch-decisions.png",
          alt: "Pending decision cards with approve and reject buttons beside the fleet map",
          label: "Pending Decisions",
        },
      ],
      features: [
        "Hours of service checked per driver",
        "Every tool call and reason on the record",
        "Approve or reject each suggestion",
        "AI Copilot in a drawer on every screen",
      ],
      icon: "sparkles",
    },
    {
      title: "Broker Negotiation",
      description:
        "The agent emails brokers about rates and works the thread for you. Set a floor per lane and it will not counter below it. You read the whole exchange, message by message, with the rate on each one.",
      screenshots: [
        {
          src: "images/screenshots/ai-dispatch-negotiation-thread.png",
          alt: "Broker rate thread showing four emails and the rate proposed in each",
          label: "Rate Thread",
        },
        {
          src: "images/screenshots/ai-dispatch-negotiations.png",
          alt: "Negotiation list with lane, broker, offers, rounds used, and status",
          label: "All Threads",
        },
        {
          src: "images/screenshots/ai-dispatch-rate-floors.png",
          alt: "Lane rate floors table showing dollars per mile per lane",
          label: "Rate Floors",
        },
      ],
      features: [
        "Rate emails sent and parsed for you",
        "A dollars-per-mile floor per lane",
        "Round limits and a reply deadline",
        "Every message kept on the thread",
      ],
      icon: "mail",
    },
    {
      title: "TMS Dashboard",
      description:
        "One screen for the whole operation - weekly gross, billed miles, rate per mile, what needs attention, and where the fleet is.",
      screenshots: [
        {
          src: "images/screenshots/tms-home.png",
          alt: "TMS dashboard with revenue metrics, active loads, and the fleet map",
          label: "Dashboard Overview",
        },
        {
          src: "images/screenshots/tms-home-lower.png",
          alt: "Dashboard panels for fleet overview and setup progress",
          label: "Panels",
        },
      ],
      features: [
        "Drag panels into the layout you want",
        "Live fleet map",
        "Unassigned loads and idle trucks up front",
      ],
      icon: "chart-column",
    },
    {
      title: "Load Management",
      description:
        "Create, assign, and track loads from pickup to delivery. Filter by status, customer, truck, or date range - you can see where each load is at any point.",
      screenshots: [
        {
          src: "images/screenshots/tms-loads.png",
          alt: "Load list with filters, status badges, revenue, and assigned trucks",
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
      icon: "box",
    },
    {
      title: "Trips & Route Optimization",
      description:
        "Plan trips with multi-stop routing, assign trucks and drivers, and watch the route and every stop on an interactive map.",
      screenshots: [
        {
          src: "images/screenshots/tms-trips.png",
          alt: "Trips list with status, routes, and truck assignments",
          label: "Trips List",
        },
        {
          src: "images/screenshots/tms-trip-details.png",
          alt: "Trip details with a numbered route on the map, stops table, and event timeline",
          label: "Trip Details",
        },
      ],
      features: [
        "Multi-stop route planning",
        "Numbered stops drawn on the map",
        "A timeline of dispatch, pickup, and delivery",
      ],
      icon: "map",
    },
    {
      title: "Fleet & Compliance",
      description:
        "Trucks, drivers, and the paperwork that keeps them legal. Hours of service, inspection reports, driver behaviour, and service records in one place.",
      screenshots: [
        {
          src: "images/screenshots/tms-fleet.png",
          alt: "Fleet view showing trucks, drivers, and availability",
          label: "Fleet List",
        },
        {
          src: "images/screenshots/tms-eld.png",
          alt: "ELD hours-of-service dashboard listing remaining hours per driver",
          label: "ELD / HOS",
        },
      ],
      features: [
        "Hours of service per driver",
        "DVIR inspections and defects",
        "Driver behaviour events",
        "Service records and upcoming maintenance",
      ],
      icon: "truck",
    },
    {
      title: "Accounting",
      description:
        "Load invoices, payroll, fuel cards, and IFTA in one place. Track what is outstanding, approve payroll, and sync it all to QuickBooks Online so your accountant's books stay current.",
      screenshots: [
        {
          src: "images/screenshots/tms-invoice-dashboard.png",
          alt: "Invoice dashboard with draft, overdue, paid, and outstanding totals",
          label: "Invoice Dashboard",
        },
        {
          src: "images/screenshots/tms-payroll.png",
          alt: "Payroll dashboard with recent payrolls and employee payments",
          label: "Payroll",
        },
        {
          src: "images/screenshots/tms-ifta-report.png",
          alt: "IFTA fuel tax report with a per-jurisdiction miles and tax breakdown",
          label: "IFTA Report",
        },
      ],
      features: [
        "Load invoice tracking",
        "Payroll processing and approval",
        "Fuel card transactions matched to trucks",
        "Quarterly IFTA, exportable as CSV or PDF",
        "QuickBooks Online sync",
      ],
      icon: "wallet",
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
      icon: "users",
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
