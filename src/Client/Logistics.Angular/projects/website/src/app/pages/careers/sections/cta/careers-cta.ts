import { Component } from "@angular/core";
import { SectionContainer } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";
import { ButtonModule } from "primeng/button";

@Component({
  selector: "web-careers-cta",
  templateUrl: "./careers-cta.html",
  imports: [SectionContainer, ScrollAnimateDirective, ButtonModule],
})
export class CareersCta {}
