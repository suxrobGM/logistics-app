import { Component } from "@angular/core";
import { IconCircle, SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface Step {
  number: number;
  title: string;
  description: string;
  icon: string;
}

@Component({
  selector: "web-how-it-works",
  templateUrl: "./how-it-works.html",
  imports: [IconCircle, SectionContainer, SectionHeader, ScrollAnimateDirective],
})
export class HowItWorks {
  protected readonly steps: Step[] = [
    {
      number: 1,
      title: "Sign Up",
      description:
        "Create your company account in just a few minutes with a simple onboarding process.",
      icon: "pi-user-plus",
    },
    {
      number: 2,
      title: "Add Your Fleet",
      description: "Import your trucks, trailers, and driver information quickly and easily.",
      icon: "pi-truck",
    },
    {
      number: 3,
      title: "Start Dispatching",
      description: "Assign loads, track deliveries, and communicate with drivers in real-time.",
      icon: "pi-play",
    },
    {
      number: 4,
      title: "Get Paid",
      description: "Automated invoicing and payment collection to improve your cash flow.",
      icon: "pi-wallet",
    },
  ];
}
