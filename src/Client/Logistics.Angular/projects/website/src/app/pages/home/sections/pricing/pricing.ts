import { Component, inject } from "@angular/core";
import { SectionContainer, SectionHeader } from "@/shared/components";
import { DemoDialogService } from "@/shared/services";
import { ScrollAnimateDirective } from "@/shared/directives";
import { ButtonModule } from "primeng/button";

@Component({
  selector: "web-pricing",
  templateUrl: "./pricing.html",
  imports: [ButtonModule, SectionContainer, SectionHeader, ScrollAnimateDirective],
})
export class Pricing {
  private readonly demoDialogService = inject(DemoDialogService);

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

  protected openDemoDialog(): void {
    this.demoDialogService.open();
  }
}
