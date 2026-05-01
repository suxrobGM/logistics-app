import { Component } from "@angular/core";
import { IconCircle, SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface PillarCard {
  icon: string;
  title: string;
  description: string;
}

@Component({
  selector: "web-mission",
  templateUrl: "./mission.html",
  imports: [SectionContainer, SectionHeader, IconCircle, ScrollAnimateDirective],
})
export class Mission {
  protected readonly pillars: PillarCard[] = [
    {
      icon: "pi-eye",
      title: "Vision",
      description:
        "Be the fleet platform that trucking companies actually want to use, not the one IT made them install.",
    },
    {
      icon: "pi-compass",
      title: "Mission",
      description:
        "Give trucking companies tools that are good at the boring parts so they can spend their time on the work that pays.",
    },
    {
      icon: "pi-heart",
      title: "Values",
      description:
        "Transparency, reliability, customer success, and shipping new things instead of talking about them.",
    },
  ];
}
