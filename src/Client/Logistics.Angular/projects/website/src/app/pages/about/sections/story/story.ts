import { Component } from "@angular/core";
import { IconCircle, SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface Milestone {
  year: string;
  title: string;
  description: string;
  icon: string;
}

@Component({
  selector: "web-story",
  templateUrl: "./story.html",
  imports: [SectionContainer, SectionHeader, IconCircle, ScrollAnimateDirective],
})
export class Story {
  protected readonly milestones: Milestone[] = [
    {
      year: "2018",
      title: "The Beginning",
      description:
        "Founded in Houston, TX by a team of logistics veterans who saw the need for modern fleet management solutions.",
      icon: "pi-flag",
    },
    {
      year: "2020",
      title: "Rapid Growth",
      description:
        "Reached 100 customers and launched our mobile driver app, enabling real-time communication and tracking.",
      icon: "pi-chart-line",
    },
    {
      year: "2022",
      title: "ELD Integration",
      description:
        "Launched integrations with major ELD providers, bringing Hours of Service compliance into a single platform.",
      icon: "pi-link",
    },
    {
      year: "2024",
      title: "500+ Companies",
      description:
        "Surpassed 500 trucking companies and 50,000 trucks managed on our platform across North America.",
      icon: "pi-globe",
    },
  ];
}
