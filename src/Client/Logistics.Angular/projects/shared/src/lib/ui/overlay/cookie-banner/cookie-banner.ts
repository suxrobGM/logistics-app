import { Component, computed, inject, signal } from "@angular/core";
import { ConsentService } from "../../../services/consent.service";
import { UiButton } from "../../action/button/button";
import { Typography } from "../../display/typography/typography";
import { UiCheckboxField } from "../../form/checkbox-field/checkbox-field";
import { Stack } from "../../layout/stack/stack";
import { Surface } from "../../layout/surface/surface";

/**
 * Bottom-of-screen consent banner shown on first visit. Mounts at the root
 * layout of the website and customer portal; auto-hides once the visitor
 * makes a decision. Strictly necessary cookies are always on and are not
 * shown as a toggle.
 */
@Component({
  selector: "ui-cookie-banner",
  templateUrl: "./cookie-banner.html",
  imports: [UiCheckboxField, Stack, Surface, Typography, UiButton],
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
