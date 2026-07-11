import { Component, input, output } from "@angular/core";
import { Icon, UiButton, type IconName } from "@logistics/shared/ui";
import { HeroBackground } from "../hero-background/hero-background";

interface StatItem {
  value: string;
  label: string;
}

@Component({
  selector: "web-page-hero",
  templateUrl: "./page-hero.html",
  imports: [HeroBackground, Icon, UiButton],
})
export class PageHero {
  public readonly badgeIcon = input.required<IconName>();
  public readonly badgeText = input.required<string>();
  public readonly headline = input.required<string>();
  public readonly accentLine = input<string>();
  public readonly description = input.required<string>();
  public readonly ctaLabel = input<string>();
  /** Trailing glyph on the CTA. Feeds `<ui-button [iconEnd]>`, so it is typed — the old
   *  `'pi ' + ctaIcon()` class-string concatenation is gone with the p-button. */
  public readonly ctaIcon = input<IconName>();
  public readonly stats = input<StatItem[]>();

  public readonly ctaClick = output<void>();
}
