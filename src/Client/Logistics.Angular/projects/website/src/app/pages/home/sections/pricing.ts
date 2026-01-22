import { Component, output } from "@angular/core";
import { ButtonModule } from "primeng/button";

@Component({
  selector: "web-pricing",
  templateUrl: "./pricing.html",
  imports: [ButtonModule],
})
export class Pricing {
  public readonly demoRequested = output<void>();

  protected readonly features = [
    "Real-time GPS tracking",
    "Automated dispatching",
    "Driver mobile app",
    "Invoicing & payments",
    "ELD/HOS compliance",
    "Real-time messaging",
    "Document management",
    "Reports & analytics",
    "24/7 customer support",
    "Unlimited loads",
  ];
}
