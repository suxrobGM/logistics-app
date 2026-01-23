import { Component } from "@angular/core";
import { SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface Feature {
  icon: string;
  title: string;
  description: string;
}

@Component({
  selector: "web-features",
  templateUrl: "./features.html",
  imports: [SectionContainer, SectionHeader, ScrollAnimateDirective],
})
export class Features {
  protected readonly features: Feature[] = [
    {
      icon: "pi-map-marker",
      title: "Real-Time GPS Tracking",
      description:
        "Track your entire fleet in real-time with live map updates, geofencing alerts, and detailed route history.",
    },
    {
      icon: "pi-send",
      title: "Automated Dispatching",
      description:
        "Intelligent load assignment and route optimization to maximize efficiency and reduce empty miles.",
    },
    {
      icon: "pi-mobile",
      title: "Driver Mobile App",
      description:
        "Empower drivers with a native app for POD capture, navigation, document scanning, and real-time communication.",
    },
    {
      icon: "pi-credit-card",
      title: "Invoicing & Payments",
      description:
        "Automated invoicing with Stripe integration for seamless payment processing and faster cash flow.",
    },
    {
      icon: "pi-clock",
      title: "ELD/HOS Compliance",
      description:
        "Integrated with Samsara and Motive for automatic hours of service tracking and FMCSA compliance.",
    },
    {
      icon: "pi-comments",
      title: "Real-Time Messaging",
      description:
        "Built-in messaging between dispatchers and drivers with read receipts and typing indicators.",
    },
    {
      icon: "pi-user",
      title: "Customer Self-Service Portal",
      description:
        "Give your customers 24/7 access to track shipments, view invoices, request quotes, and download delivery documents.",
    },
    {
      icon: "pi-clipboard",
      title: "Vehicle Inspections (DVIR)",
      description:
        "Digital pre-trip and post-trip inspections with photo documentation and automated maintenance alerts.",
    },
  ];
}
