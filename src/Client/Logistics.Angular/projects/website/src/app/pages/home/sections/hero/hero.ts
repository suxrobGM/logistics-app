import { Component, inject } from "@angular/core";
import { BrowserFrame, HeroBackground } from "@/shared/components";
import { DemoDialogService } from "@/shared/services";
import { ButtonModule } from "primeng/button";

interface StatItem {
  value: string;
  label: string;
}

@Component({
  selector: "web-hero",
  templateUrl: "./hero.html",
  imports: [ButtonModule, HeroBackground, BrowserFrame],
})
export class Hero {
  private readonly demoDialogService = inject(DemoDialogService);

  protected readonly stats: StatItem[] = [
    { value: "8+", label: "Integrations" },
    { value: "3", label: "Portals" },
    { value: "24/7", label: "Real-time Tracking" },
    { value: "100%", label: "Cloud-based" },
  ];

  protected openDemoDialog(): void {
    this.demoDialogService.open();
  }
}

