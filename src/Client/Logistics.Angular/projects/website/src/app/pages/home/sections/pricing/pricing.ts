import { Component, inject } from "@angular/core";
import { ButtonModule } from "primeng/button";
import { SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";
import { DemoDialogService } from "@/shared/services";

interface PricingTier {
  name: string;
  basePrice: number;
  perTruckPrice: number;
  description: string;
  features: string[];
  highlighted: boolean;
  badge?: string;
}

@Component({
  selector: "web-pricing",
  templateUrl: "./pricing.html",
  imports: [ButtonModule, SectionContainer, SectionHeader, ScrollAnimateDirective],
})
export class Pricing {
  private readonly demoDialogService = inject(DemoDialogService);

  protected readonly tiers: PricingTier[] = [
    {
      name: "Starter",
      basePrice: 29,
      perTruckPrice: 12,
      description: "The basics for small fleets just getting started.",
      features: [
        "Load management & dispatching",
        "Trip & route optimization",
        "Fleet & driver management",
        "ELD / HOS compliance",
        "Invoicing & payments",
        "Expenses tracking",
        "Reports & analytics",
        "Real-time messaging",
        "AI Dispatch - base models",
        "Up to 10 trucks",
      ],
      highlighted: false,
    },
    {
      name: "Professional",
      basePrice: 79,
      perTruckPrice: 9,
      description: "More features as fleets grow.",
      features: [
        "Everything in Starter",
        "Load board integrations",
        "Payroll management",
        "Timesheets tracking",
        "Safety & compliance",
        "Maintenance tracking",
        "Telegram bot integration",
        "MCP Server",
        "AI Dispatch - premium models, 4× usage",
        "Up to 30 trucks",
      ],
      highlighted: true,
      badge: "Most Popular",
    },
    {
      name: "Enterprise",
      basePrice: 169,
      perTruckPrice: 6,
      description: "The whole platform, for larger operations.",
      features: [
        "Everything in Professional",
        "AI Dispatch - all models incl. Opus, 8× usage",
        "Unlimited trucks",
        "Priority support",
        "API access & documentation",
        "Custom integrations",
      ],
      highlighted: false,
    },
  ];

  protected openDemoDialog(): void {
    this.demoDialogService.open();
  }
}
