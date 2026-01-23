import { Component, inject } from "@angular/core";
import { HeroBackground } from "@/shared/components";
import { DemoDialogService } from "@/shared/services";
import { ButtonModule } from "primeng/button";

interface StatItem {
  value: string;
  label: string;
}

@Component({
  selector: "web-hero",
  templateUrl: "./hero.html",
  imports: [ButtonModule, HeroBackground],
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
