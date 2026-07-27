import { Component } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Icon } from "@logistics/shared/ui";
import { IconCard, SectionContainer, SectionHeader, type IconCardItem } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface Segment extends IconCardItem {
  variant: "accent" | "ink";
}

@Component({
  selector: "web-segments",
  templateUrl: "./segments.html",
  imports: [Icon, IconCard, RouterLink, ScrollAnimateDirective, SectionContainer, SectionHeader],
})
export class Segments {
  protected readonly segments: Segment[] = [
    {
      icon: "user",
      title: "Owner-operator, 1 truck",
      description:
        "You're the driver, the dispatcher, and the billing department. Solo mode drops the crew-shaped parts of the app, and the agent plans around your truck and your hours instead of asking which truck to use. $41 a month, all in.",
      variant: "accent",
    },
    {
      icon: "truck",
      title: "Small fleet, 2 to 10 trucks",
      description:
        "A few drivers and someone answering the phone. Loads, trips, invoices, and payroll live in one place instead of a spreadsheet and a group chat. Same $29 Starter plan.",
      variant: "ink",
    },
    {
      icon: "building-2",
      title: "Growing fleet, 10 and up",
      description:
        "Load boards, broker credit checks, IFTA, fuel card sync, QuickBooks, and API access. The per-truck rate drops at each plan, so the bill doesn't climb in a straight line.",
      variant: "ink",
    },
  ];
}
