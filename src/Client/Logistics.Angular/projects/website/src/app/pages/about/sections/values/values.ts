import { Component } from "@angular/core";
import { IconCard, SectionContainer, SectionHeader } from "@/shared/components";
import type { IconCardItem } from "@/shared/components";
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
        "We continuously push boundaries to deliver cutting-edge solutions that keep our customers ahead of the curve.",
    },
    {
      icon: "pi-shield",
      title: "Reliability",
      description:
        "Our platform is built for 99.9% uptime because we know your business depends on it every minute of every day.",
    },
    {
      icon: "pi-users",
      title: "Customer Focus",
      description:
        "Every feature we build and every decision we make starts with understanding our customers' needs.",
    },
    {
      icon: "pi-check-circle",
      title: "Integrity",
      description:
        "We operate with transparency and honesty, building trust through our actions and commitments.",
    },
  ];
}
