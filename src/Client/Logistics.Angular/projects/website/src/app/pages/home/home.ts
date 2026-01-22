import { Component, signal } from "@angular/core";
import { Footer, Navbar } from "@/layout";
import { DemoDialog } from "@/shared/components";
import { Faq } from "./sections/faq";
import { Features } from "./sections/features";
import { Hero } from "./sections/hero";
import { HowItWorks } from "./sections/how-it-works";
import { Pricing } from "./sections/pricing";
import { Testimonials } from "./sections/testimonials";

@Component({
  selector: "app-home",
  templateUrl: "./home.html",
  imports: [Navbar, Footer, DemoDialog, Hero, Features, HowItWorks, Testimonials, Pricing, Faq],
})
export class Home {
  protected readonly showDemoDialog = signal(false);

  protected openDemoDialog(): void {
    this.showDemoDialog.set(true);
  }
}
