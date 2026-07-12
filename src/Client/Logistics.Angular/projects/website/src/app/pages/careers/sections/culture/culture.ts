import { Component } from "@angular/core";
import type { IconName } from "@logistics/shared/ui";
import { IconCircle, SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface CultureHighlight {
  icon: IconName;
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
      icon: "zap",
      title: "Ship",
      description:
        "We push code often, iterate, and take the kind of risks where the downside is fixable.",
    },
    {
      icon: "heart",
      title: "Care",
      description: "About the customers, the team, and the quality of what we ship.",
    },
    {
      icon: "messages-square",
      title: "Talk straight",
      description:
        "Transparency, honest feedback, and keeping each other in the loop. No politicking.",
    },
  ];
}
