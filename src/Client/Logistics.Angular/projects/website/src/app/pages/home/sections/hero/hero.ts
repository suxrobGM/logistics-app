import { Component, inject } from "@angular/core";
import { type StatItem, StatsGrid } from "@/shared/components";
import { DemoDialogService } from "@/shared/services";
import { ButtonModule } from "primeng/button";

@Component({
  selector: "web-hero",
  templateUrl: "./hero.html",
  imports: [ButtonModule, StatsGrid],
})
export class Hero {
  private readonly demoDialogService = inject(DemoDialogService);

  protected readonly stats: StatItem[] = [
    { value: "500+", label: "Companies" },
    { value: "50K+", label: "Trucks Tracked" },
    { value: "99.9%", label: "Uptime" },
    { value: "24/7", label: "Support" },
  ];

  protected openDemoDialog(): void {
    this.demoDialogService.open();
  }
}
