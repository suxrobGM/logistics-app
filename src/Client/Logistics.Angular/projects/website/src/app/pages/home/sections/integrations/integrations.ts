import { Component } from "@angular/core";
import { SectionContainer } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives/scroll-animate.directive";

interface Integration {
  name: string;
  logo: string; // SVG path or icon class
}

@Component({
  selector: "web-integrations",
  templateUrl: "./integrations.html",
  imports: [SectionContainer, ScrollAnimateDirective],
})
export class Integrations {
  protected readonly integrations: Integration[] = [
    { name: "Samsara", logo: "samsara" },
    { name: "Motive", logo: "motive" },
    { name: "Stripe", logo: "stripe" },
    { name: "Mapbox", logo: "mapbox" },
    { name: "Firebase", logo: "firebase" },
  ];
}
