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
      title: "Set Up Your Fleet",
      description:
        "Add trucks, drivers, and fleet details. Connect your ELD provider for automatic HOS tracking.",
      icon: "pi-truck",
    },
    {
      number: 2,
      title: "Create Loads",
      description:
        "Enter shipments or import from load boards. The AI agent sees them instantly.",
      icon: "pi-box",
    },
    {
      number: 3,
      title: "Let AI Dispatch",
      description:
        "The agent analyzes availability, HOS, deadhead miles, and revenue to find optimal assignments.",
      icon: "pi-sparkles",
    },
    {
      number: 4,
      title: "Review & Go",
      description:
        "Approve suggestions or let the agent run autonomously. Trips are created and dispatched automatically.",
      icon: "pi-check-circle",
    },
  ];
}
