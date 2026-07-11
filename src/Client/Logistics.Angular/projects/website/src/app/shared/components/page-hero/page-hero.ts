import { Component, input, output } from "@angular/core";
import { Icon, type IconName } from "@logistics/shared/ui";
import { ButtonModule } from "primeng/button";
import { HeroBackground } from "../hero-background/hero-background";

interface StatItem {
  value: string;
  label: string;
}

@Component({
  selector: "web-page-hero",
  templateUrl: "./page-hero.html",
  imports: [ButtonModule, HeroBackground, Icon],
})
export class PageHero {
  public readonly badgeIcon = input.required<IconName>();
  public readonly badgeText = input.required<string>();
  public readonly headline = input.required<string>();
  public readonly accentLine = input<string>();
  public readonly description = input.required<string>();
  public readonly ctaLabel = input<string>();
  /** primeicons CLASS STRING — feeds `<p-button [icon]>`, which renders it on an internal
   *  `<span>`. Migrates with the button in S4, not with the icons. */
  public readonly ctaIcon = input<string>();
  public readonly stats = input<StatItem[]>();

  public readonly ctaClick = output<void>();
}
