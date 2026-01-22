import { Component, output } from "@angular/core";
import { ButtonModule } from "primeng/button";

@Component({
  selector: "web-hero",
  templateUrl: "./hero.html",
  imports: [ButtonModule],
})
export class Hero {
  public readonly demoRequested = output<void>();
}
