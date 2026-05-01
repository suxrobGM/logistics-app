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
      title: "Set up the fleet",
      description:
        "Add trucks, drivers, and fleet details. Connect your ELD provider so HOS tracking happens automatically.",
      icon: "pi-truck",
    },
    {
      number: 2,
      title: "Create loads",
      description:
        "Enter shipments or pull them in from a load board. The agent sees them as soon as they're saved.",
      icon: "pi-box",
    },
    {
      number: 3,
      title: "Let the agent dispatch",
      description:
        "It looks at availability, HOS, deadhead miles, and revenue, then proposes assignments.",
      icon: "pi-sparkles",
    },
    {
      number: 4,
      title: "Review and go",
      description:
        "Approve the suggestions, or hand the agent the keys. Trips get created and dispatched either way.",
      icon: "pi-check-circle",
    },
  ];
}
