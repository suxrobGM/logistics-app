import { Component } from "@angular/core";
import { IconCircle, SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface CultureHighlight {
  icon: string;
  title: string;
  description: string;
}

@Component({
  selector: "web-culture",
  templateUrl: "./culture.html",
  imports: [SectionContainer, SectionHeader, IconCircle, ScrollAnimateDirective],
})
export class Culture {
  protected readonly highlights: CultureHighlight[] = [
    {
      icon: "pi-bolt",
      title: "Move Fast",
      description:
        "We ship quickly, iterate often, and aren't afraid to take calculated risks to deliver value.",
    },
    {
      icon: "pi-heart",
      title: "Care Deeply",
      description:
        "We care about our customers, our teammates, and the quality of everything we create.",
    },
    {
      icon: "pi-comments",
      title: "Communicate Openly",
      description:
        "We believe in transparency, honest feedback, and keeping everyone informed and aligned.",
    },
  ];
}
