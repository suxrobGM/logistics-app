import { Component } from "@angular/core";
import { IconCard, SectionContainer, SectionHeader } from "@/shared/components";
import type { IconCardItem } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

@Component({
  selector: "web-benefits",
  templateUrl: "./benefits.html",
  imports: [SectionContainer, SectionHeader, IconCard, ScrollAnimateDirective],
})
export class Benefits {
  protected readonly benefits: IconCardItem[] = [
    {
      icon: "pi-heart",
      title: "Health Insurance",
      description: "Comprehensive medical, dental, and vision coverage for you and your family.",
    },
    {
      icon: "pi-home",
      title: "Remote Friendly",
      description: "Work from anywhere with flexible hours. We focus on results, not hours logged.",
    },
    {
      icon: "pi-calendar",
      title: "Unlimited PTO",
      description:
        "Take the time you need to recharge. We trust you to manage your own time off.",
    },
    {
      icon: "pi-wallet",
      title: "401(k) Match",
      description: "We match 100% of your contributions up to 4% to help you plan for the future.",
    },
    {
      icon: "pi-book",
      title: "Learning Budget",
      description:
        "$2,000 annual budget for courses, conferences, books, and professional development.",
    },
    {
      icon: "pi-users",
      title: "Team Events",
      description:
        "Regular team outings, annual retreats, and virtual events to stay connected.",
    },
  ];
}
