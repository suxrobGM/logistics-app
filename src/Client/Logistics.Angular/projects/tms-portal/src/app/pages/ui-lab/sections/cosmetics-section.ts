import { Component, signal } from "@angular/core";
import {
  Avatar,
  Card,
  CountBadge,
  OverlayBadge,
  Progress,
  Skeleton,
  Spinner,
  Typography,
  UI_BADGE_INTENTS,
  type UiAvatarSize,
  type UiCountBadgeSize,
} from "@logistics/shared/ui";

/**
 * The cosmetics: ui-card, ui-spinner, ui-skeleton, ui-avatar, ui-progress, ui-count-badge,
 * ui-overlay-badge.
 *
 * The card matrix is the one that earns its keep. `ui-card` supports FOUR template slots
 * (`#header`, `#title`, `#subtitle`, `#footer`), and a card wired up with the wrong slot name
 * silently drops that content — nothing warns you. All four are rendered below, together and
 * separately.
 *
 * `ui-overlay-badge` is here for one row: the null-value case. The notification bell says "nothing
 * unread" by passing `null` and trusting the badge to disappear. Both states are on the page so the
 * difference is a screenshot, not an argument.
 */
@Component({
  selector: "app-ui-lab-cosmetics",
  templateUrl: "./cosmetics-section.html",
  imports: [Card, Spinner, Skeleton, Avatar, Progress, CountBadge, OverlayBadge, Typography],
})
export class UiLabCosmeticsSection {
  /** Driven off the exported vocabulary, so a tone added to the union appears here by itself. */
  protected readonly tones = UI_BADGE_INTENTS;

  protected readonly avatarSizes: readonly UiAvatarSize[] = ["normal", "large", "xlarge"];
  protected readonly countBadgeSizes: readonly UiCountBadgeSize[] = ["small", "normal"];

  /** p-progress-spinner's default is 100px; the four sized call sites want 24 / 40 / 50. */
  protected readonly spinnerSizes: readonly string[] = ["24px", "40px", "50px", "100px"];

  protected readonly progress = signal(42);

  /** Drives the overlay badge between its two states — the whole point of the component. */
  protected readonly unread = signal<number | null>(3);

  protected toggleUnread(): void {
    this.unread.update((value) => (value === null ? 3 : null));
  }
}
