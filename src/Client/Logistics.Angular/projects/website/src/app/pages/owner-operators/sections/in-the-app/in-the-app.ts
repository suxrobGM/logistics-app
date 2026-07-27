import { Component } from "@angular/core";
import { Icon } from "@logistics/shared/ui";
import { BrowserFrame, SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

@Component({
  selector: "web-in-the-app",
  templateUrl: "./in-the-app.html",
  imports: [BrowserFrame, Icon, ScrollAnimateDirective, SectionContainer, SectionHeader],
})
export class InTheApp {
  protected readonly points: string[] = [
    "One dashboard for the load you're on, what you've billed, and what's still unpaid",
    "The dispatch agent's reasoning written out, so you can argue with it",
    "The driver app on your phone, signed in as the same person",
    "Quarterly IFTA built from your ELD miles and fuel card gallons",
  ];
}
