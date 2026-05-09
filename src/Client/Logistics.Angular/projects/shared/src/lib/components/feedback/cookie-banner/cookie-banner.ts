import { Component, computed, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ButtonModule } from "primeng/button";
import { CheckboxModule } from "primeng/checkbox";
import { ConsentService } from "../../../services/consent.service";
import { Stack } from "../../primitives/stack/stack";
import { Surface } from "../../primitives/surface/surface";
import { Typography } from "../../primitives/typography/typography";

/**
 * Bottom-of-screen consent banner shown on first visit. Mounts at the root
 * layout of the website and customer portal; auto-hides once the visitor
 * makes a decision. Strictly necessary cookies are always on and are not
 * shown as a toggle.
 */
@Component({
  selector: "ui-cookie-banner",
  templateUrl: "./cookie-banner.html",
  imports: [ButtonModule, CheckboxModule, FormsModule, Stack, Surface, Typography],
})
export class CookieBanner {
  private readonly consent = inject(ConsentService);

  protected readonly visible = computed(() => !this.consent.hasDecided());
  protected readonly customizing = signal(false);

  protected readonly functional = signal(false);
  protected readonly analytics = signal(false);
  protected readonly marketing = signal(false);

  protected acceptAll(): void {
    this.consent.acceptAll();
  }

  protected rejectAll(): void {
    this.consent.rejectAll();
  }

  protected openCustomize(): void {
    const current = this.consent.choices();
    this.functional.set(current.functional);
    this.analytics.set(current.analytics);
    this.marketing.set(current.marketing);
    this.customizing.set(true);
  }

  protected savePreferences(): void {
    this.consent.saveChoices({
      functional: this.functional(),
      analytics: this.analytics(),
      marketing: this.marketing(),
    });
    this.customizing.set(false);
  }
}
