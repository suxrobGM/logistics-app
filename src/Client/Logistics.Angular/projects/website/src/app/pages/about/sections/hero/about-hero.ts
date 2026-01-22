import { Component } from "@angular/core";
import { PageHero, type StatItem } from "@/shared/components";

@Component({
  selector: "web-about-hero",
  templateUrl: "./about-hero.html",
  imports: [PageHero],
})
export class AboutHero {
  protected readonly stats: StatItem[] = [
    { value: "2018", label: "Founded" },
    { value: "500+", label: "Companies" },
    { value: "150+", label: "Employees" },
    { value: "50K+", label: "Trucks Managed" },
  ];
}
