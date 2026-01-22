import { Component } from "@angular/core";
import { Faq, Features, Hero, HowItWorks, Integrations, Pricing, Testimonials } from "./sections";

@Component({
  selector: "web-home",
  templateUrl: "./home.html",
  imports: [Hero, Features, Integrations, HowItWorks, Testimonials, Pricing, Faq],
})
export class Home {}
