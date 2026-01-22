import { Component } from "@angular/core";
import { SectionContainer } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives/scroll-animate.directive";

interface Integration {
  name: string;
  logo: string;
}

@Component({
  selector: "web-integrations",
  templateUrl: "./integrations.html",
  imports: [SectionContainer, ScrollAnimateDirective],
})
export class Integrations {
  protected readonly integrations: Integration[] = [
    { name: "Samsara", logo: "images/samsara-logo.png" },
    { name: "Motive", logo: "images/motive-logo.jpg" },
    { name: "Stripe", logo: "images/stripe-logo.svg" },
    { name: "Mapbox", logo: "images/mapbox-logo.svg" },
    { name: "Firebase", logo: "images/firebase-logo.svg" },
  ];
}
