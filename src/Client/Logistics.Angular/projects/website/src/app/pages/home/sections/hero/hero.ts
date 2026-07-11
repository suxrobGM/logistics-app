import { Component, inject, signal } from "@angular/core";
import { Icon, UiButton } from "@logistics/shared/ui";
import { TooltipModule } from "primeng/tooltip";
import { BrowserFrame, HeroBackground } from "@/shared/components";
import { DemoDialogService } from "@/shared/services";

interface StatItem {
  value: string;
  label: string;
}

@Component({
  selector: "web-hero",
  templateUrl: "./hero.html",
  imports: [BrowserFrame, HeroBackground, Icon, TooltipModule, UiButton],
})
export class Hero {
  private readonly demoDialogService = inject(DemoDialogService);

  protected readonly showVideo = signal(false);

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
