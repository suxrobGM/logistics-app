import { Component, signal } from "@angular/core";
import { Footer, Navbar } from "@/layout";
import { DemoDialog } from "@/shared/components";
import { Faq, Features, Hero, HowItWorks, Integrations, Pricing, Testimonials } from "./sections";

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
