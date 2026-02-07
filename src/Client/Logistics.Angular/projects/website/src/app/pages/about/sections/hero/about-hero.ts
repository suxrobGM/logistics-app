import { Component } from "@angular/core";
import { PageHero, type StatItem } from "@/shared/components";

@Component({
  selector: "web-about-hero",
  templateUrl: "./about-hero.html",
  imports: [PageHero],
})
export class AboutHero {
  protected readonly stats: StatItem[] = [
    { value: "2024", label: "Founded" },
    { value: "3", label: "Product Portals" },
    { value: "8+", label: "Integrations" },
    { value: "24/7", label: "Cloud Platform" },
  ];
}
