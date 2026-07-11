import { Component } from "@angular/core";
import { UiButton } from "@logistics/shared/ui";
import { SectionContainer } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

@Component({
  selector: "web-careers-cta",
  templateUrl: "./careers-cta.html",
  imports: [ScrollAnimateDirective, SectionContainer, UiButton],
})
export class CareersCta {}
