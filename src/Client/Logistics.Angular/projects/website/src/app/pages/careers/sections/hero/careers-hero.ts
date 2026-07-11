import { DOCUMENT } from "@angular/common";
import { Component, inject } from "@angular/core";
import { PageHero } from "@/shared/components";

@Component({
  selector: "web-careers-hero",
  templateUrl: "./careers-hero.html",
  imports: [PageHero],
})
export class CareersHero {
  private readonly document = inject(DOCUMENT);

  protected scrollToPositions(): void {
    this.document.getElementById("positions")?.scrollIntoView({ behavior: "smooth" });
  }
}
