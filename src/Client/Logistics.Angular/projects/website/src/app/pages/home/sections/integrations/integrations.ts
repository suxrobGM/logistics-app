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
    { name: "Anthropic", logo: "images/icons/anthropic.svg" },
    { name: "OpenAI", logo: "images/icons/openai.svg" },
    { name: "DeepSeek", logo: "images/icons/deepseek.png" },
    { name: "Samsara", logo: "images/icons/samsara.png" },
    { name: "Motive", logo: "images/icons/motive.jpg" },
    { name: "TT ELD", logo: "images/icons/tteld.png" },
    { name: "Stripe", logo: "images/icons/stripe.svg" },
    { name: "Mapbox", logo: "images/icons/mapbox.svg" },
    { name: "Firebase", logo: "images/icons/firebase.svg" },
    { name: "Cloudflare", logo: "images/icons/cloudflare.svg" },
  ];
}
