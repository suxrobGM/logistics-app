import { Component, inject } from "@angular/core";
import { BrowserFrame, HeroBackground } from "@/shared/components";
import { DemoDialogService } from "@/shared/services";
import { ButtonModule } from "primeng/button";
import { TooltipModule } from "primeng/tooltip";

interface StatItem {
  value: string;
  label: string;
}

@Component({
  selector: "web-hero",
  templateUrl: "./hero.html",
  imports: [ButtonModule, TooltipModule, HeroBackground, BrowserFrame],
})
export class Hero {
  private readonly demoDialogService = inject(DemoDialogService);

  protected readonly stats: StatItem[] = [
    { value: "2 Modes", label: "Suggestions & Autonomous" },
    { value: "7+", label: "Agent Tools" },
    { value: "3", label: "AI Providers" },
    { value: "24/7", label: "Always-On Dispatch" },
  ];

  protected openDemoDialog(): void {
    this.demoDialogService.open();
  }
}

