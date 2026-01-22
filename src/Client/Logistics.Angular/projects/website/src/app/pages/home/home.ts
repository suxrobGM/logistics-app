import { Component, signal } from "@angular/core";
import { Footer, Navbar } from "@/layout";
import { DemoDialog } from "@/shared/components";
import { Faq } from "./sections/faq";
import { Features } from "./sections/features";
import { Hero } from "./sections/hero";
import { HowItWorks } from "./sections/how-it-works";
import { Integrations } from "./sections/integrations/integrations";
import { Pricing } from "./sections/pricing";
import { Testimonials } from "./sections/testimonials";

@Component({
  selector: "web-home",
  templateUrl: "./home.html",
  imports: [
    Navbar,
    Footer,
    DemoDialog,
    Hero,
    Features,
    Integrations,
    HowItWorks,
    Testimonials,
    Pricing,
    Faq,
  ],
})
export class Home {
  protected readonly showDemoDialog = signal(false);

  protected openDemoDialog(): void {
    this.showDemoDialog.set(true);
  }
}
