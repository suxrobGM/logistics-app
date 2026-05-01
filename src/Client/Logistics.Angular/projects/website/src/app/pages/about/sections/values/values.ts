import { Component } from "@angular/core";
import { IconCard, SectionContainer, SectionHeader, type IconCardItem } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

@Component({
  selector: "web-values",
  templateUrl: "./values.html",
  imports: [SectionContainer, SectionHeader, IconCard, ScrollAnimateDirective],
})
export class Values {
  protected readonly values: IconCardItem[] = [
    {
      icon: "pi-lightbulb",
      title: "Innovation",
      description:
        "We try new things and ship them. AI dispatch, the MCP server — we'd rather build it and see what works than wait until someone else does.",
    },
    {
      icon: "pi-shield",
      title: "Reliability",
      description:
        "We aim for 99.9% uptime because dispatch can't take the morning off when our database is having a moment.",
    },
    {
      icon: "pi-users",
      title: "Customer focus",
      description:
        "We build with the dispatcher and the driver in mind. The road test is whether they'd actually use it, not whether it demos well.",
    },
    {
      icon: "pi-check-circle",
      title: "Integrity",
      description:
        "We say what we ship, ship what we say, and tell you when we got something wrong.",
    },
  ];
}
