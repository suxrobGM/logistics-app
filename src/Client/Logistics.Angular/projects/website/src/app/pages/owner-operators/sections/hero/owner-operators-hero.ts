import { Component, inject } from "@angular/core";
import { PageHero, type StatItem } from "@/shared/components";
import { DemoDialogService } from "@/shared/services";

@Component({
  selector: "web-owner-operators-hero",
  templateUrl: "./owner-operators-hero.html",
  imports: [PageHero],
})
export class OwnerOperatorsHero {
  private readonly demoDialogService = inject(DemoDialogService);

  protected readonly stats: StatItem[] = [
    { value: "$41", label: "Per month, one truck" },
    { value: "Solo", label: "Operating mode" },
    { value: "0", label: "Onboarding calls" },
    { value: "24/7", label: "AI dispatch" },
  ];

  protected openDemoDialog(): void {
    this.demoDialogService.open();
  }
}
