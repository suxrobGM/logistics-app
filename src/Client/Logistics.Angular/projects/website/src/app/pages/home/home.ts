import { Component, signal } from "@angular/core";
import { Footer } from "@/layout/footer";
import { Navbar } from "@/layout/navbar";
import { DemoDialog } from "@/shared/demo-dialog";
import { Faq } from "./sections/faq";
import { Features } from "./sections/features";
import { Hero } from "./sections/hero";
import { HowItWorks } from "./sections/how-it-works";
import { Pricing } from "./sections/pricing";
import { Testimonials } from "./sections/testimonials";

@Component({
  selector: "app-home",
  imports: [Navbar, Footer, DemoDialog, Hero, Features, HowItWorks, Testimonials, Pricing, Faq],
  templateUrl: "./home.html",
})
export class Home {
  protected readonly showDemoDialog = signal(false);

  protected openDemoDialog(): void {
    this.showDemoDialog.set(true);
  }
}
