import { Component, inject } from "@angular/core";
import { ButtonModule } from "primeng/button";
import { SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";
import { DemoDialogService } from "@/shared/services";

interface FeatureRow {
  name: string;
  us: string;
  dataTruck: string;
  alvys: string;
  roseRocket: string;
}

interface FeatureCategory {
  category: string;
  rows: FeatureRow[];
}

interface PricingRow {
  trucks: number;
  us: string;
  usDetail: string;
  dataTruck: string;
  alvys: string;
}

@Component({
  selector: "web-compare",
  templateUrl: "./compare.html",
  imports: [ButtonModule, SectionContainer, SectionHeader, ScrollAnimateDirective],
})
export class Compare {
  private readonly demoDialogService = inject(DemoDialogService);

  protected readonly features: FeatureCategory[] = [
    {
      category: "AI Dispatch",
      rows: [
        {
          name: "AI dispatch (agentic, multi-LLM)",
          us: "✓",
          dataTruck: "✓ (TruckGPT)",
          alvys: "Basic",
          roseRocket: "✓ (Rosie)",
        },
        { name: "Human-in-the-loop AI", us: "✓", dataTruck: "-", alvys: "-", roseRocket: "-" },
        {
          name: "AI model choice (tenant selects)",
          us: "✓",
          dataTruck: "-",
          alvys: "-",
          roseRocket: "-",
        },
      ],
    },
    {
      category: "Load Boards",
      rows: [
        { name: "DAT load board", us: "✓", dataTruck: "✓", alvys: "✓", roseRocket: "-" },
        { name: "Truckstop", us: "✓", dataTruck: "✓", alvys: "-", roseRocket: "-" },
        { name: "123Loadboard", us: "✓", dataTruck: "✓", alvys: "-", roseRocket: "-" },
      ],
    },
    {
      category: "ELD / HOS",
      rows: [
        {
          name: "ELD/HOS (Samsara, Motive+)",
          us: "✓",
          dataTruck: "✓",
          alvys: "✓",
          roseRocket: "Limited",
        },
        { name: "DVIR inspections", us: "✓", dataTruck: "-", alvys: "✓", roseRocket: "-" },
      ],
    },
    {
      category: "Safety & Compliance",
      rows: [{ name: "Accident reporting", us: "✓", dataTruck: "-", alvys: "-", roseRocket: "-" }],
    },
    {
      category: "Financial",
      rows: [
        { name: "Stripe Connect payouts", us: "✓", dataTruck: "-", alvys: "-", roseRocket: "-" },
        { name: "Payment links (public)", us: "✓", dataTruck: "-", alvys: "-", roseRocket: "-" },
        { name: "Payroll management", us: "✓", dataTruck: "✓", alvys: "✓", roseRocket: "-" },
      ],
    },
    {
      category: "Fleet & Drivers",
      rows: [
        { name: "Driver mobile app", us: "✓", dataTruck: "✓", alvys: "✓", roseRocket: "✓" },
        { name: "Real-time GPS tracking", us: "✓", dataTruck: "✓", alvys: "✓", roseRocket: "✓" },
      ],
    },
    {
      category: "Customer Portal",
      rows: [
        { name: "Customer portal", us: "✓", dataTruck: "✓", alvys: "✓", roseRocket: "✓" },
        { name: "Public tracking links", us: "✓", dataTruck: "✓", alvys: "✓", roseRocket: "✓" },
        { name: "Multi-tenant architecture", us: "✓", dataTruck: "-", alvys: "-", roseRocket: "✓" },
      ],
    },
    {
      category: "Intermodal & Regions",
      rows: [
        {
          name: "ISO 6346 container tracking",
          us: "✓",
          dataTruck: "-",
          alvys: "-",
          roseRocket: "-",
        },
        {
          name: "Terminals & UN/LOCODE directory",
          us: "✓",
          dataTruck: "-",
          alvys: "-",
          roseRocket: "-",
        },
        {
          name: "US + Europe operations",
          us: "✓",
          dataTruck: "US only",
          alvys: "US only",
          roseRocket: "US/CA",
        },
        {
          name: "Multi-currency (USD / EUR)",
          us: "✓",
          dataTruck: "-",
          alvys: "-",
          roseRocket: "-",
        },
      ],
    },
  ];

  protected readonly pricing: PricingRow[] = [
    { trucks: 5, us: "$89", usDetail: "Starter: $29 + $60", dataTruck: "$99–299", alvys: "~$514+" },
    {
      trucks: 15,
      us: "$214",
      usDetail: "Professional: $79 + $135",
      dataTruck: "$299–499",
      alvys: "Custom",
    },
    {
      trucks: 30,
      us: "$349",
      usDetail: "Professional: $79 + $270",
      dataTruck: "$499+",
      alvys: "Custom",
    },
  ];

  protected openDemoDialog(): void {
    this.demoDialogService.open();
  }
}
