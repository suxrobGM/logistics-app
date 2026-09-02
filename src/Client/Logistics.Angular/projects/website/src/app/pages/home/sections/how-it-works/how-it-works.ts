import { Component } from "@angular/core";
import type { IconName } from "@logistics/shared/ui";
import { IconCircle, SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface Step {
  number: number;
  title: string;
  description: string;
  icon: IconName;
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
      title: "Create the account",
      description:
        "Sign up and fill in the company details. A setup checklist lands on the dashboard with whatever is still missing.",
      icon: "building-2",
    },
    {
      number: 2,
      title: "Work the checklist",
      description:
        "Add a truck, your team if you have one, a customer, your ELD, and Stripe payouts. Each item links to the page that finishes it.",
      icon: "check-square",
    },
    {
      number: 3,
      title: "Add your first load",
      description:
        "Type it in or pull it off DAT, Truckstop, or 123Loadboard. The agent sees it as soon as it's saved.",
      icon: "box",
    },
    {
      number: 4,
      title: "Let the agent dispatch",
      description:
        "It weighs HOS, deadhead miles, and rate per mile, then proposes the assignment. Approve it, or send it back with a note and it plans again.",
      icon: "sparkles",
    },
  ];
}
