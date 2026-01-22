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
        "To be the most trusted fleet management platform, enabling every trucking company to operate at peak efficiency.",
    },
    {
      icon: "pi-compass",
      title: "Mission",
      description:
        "Empower trucking companies with intuitive, powerful tools that streamline operations and drive profitability.",
    },
    {
      icon: "pi-heart",
      title: "Values",
      description:
        "We believe in transparency, reliability, customer success, and continuous innovation in everything we do.",
    },
  ];
}
