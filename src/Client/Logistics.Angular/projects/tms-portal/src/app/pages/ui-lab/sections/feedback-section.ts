import { Component, signal } from "@angular/core";
import {
  Alert,
  Badge,
  Divider,
  EmptyState,
  ErrorState,
  LoadingSkeleton,
  StatusBadge,
  Typography,
  UI_BADGE_TONES,
  type CalloutIntent,
  type StatusKind,
  type TypographyVariant,
} from "@logistics/shared/ui";

/** One `ui-status-badge` per entity kind, using a status that exercises a non-default severity. */
interface LabStatus {
  readonly kind: StatusKind;
  readonly statuses: readonly string[];
}

@Component({
  selector: "app-ui-lab-feedback",
  templateUrl: "./feedback-section.html",
  imports: [
    Alert,
    Badge,
    StatusBadge,
    EmptyState,
    ErrorState,
    LoadingSkeleton,
    Divider,
    Typography,
  ],
})
export class UiLabFeedbackSection {
  protected readonly intents: readonly CalloutIntent[] = [
    "info",
    "success",
    "warning",
    "danger",
    "neutral",
  ];

  /**
   * Driven off the exported vocabulary rather than a hand-copied list, so a tone added to the union
   * shows up in the lab by itself. A hand-written array is the one that silently stops covering the
   * type it documents.
   *
   * `UI_BADGE_TONES`, not `UI_BADGE_INTENTS`: the tones are what `<ui-badge>` can PAINT (the six
   * producer intents plus `primary`, which is what a bare `<p-tag>` fell through to). Driving the
   * matrix off the producer vocabulary would leave `primary` — the one tone with no producer and so
   * the one nobody would notice breaking — as the only cell never rendered.
   */
  protected readonly severities = UI_BADGE_TONES;

  /**
   * The second axis is `rounded`, not the old `variant`. `variant="outlined"` never rendered an
   * outline — the old template mapped it straight onto p-tag's `[rounded]`, so it drew a rounded
   * SOLID tag. The lab was faithfully displaying a lie. This axis is the thing that was actually
   * happening, under its real name.
   */
  protected readonly badgeRounded: readonly boolean[] = [false, true];

  protected readonly statusKinds: readonly LabStatus[] = [
    { kind: "load", statuses: ["Draft", "InTransit", "Delivered", "Cancelled", "Exception"] },
    { kind: "truck", statuses: ["Available", "OnRoute", "Maintenance", "OutOfService"] },
    { kind: "container", statuses: ["AtPort", "InTransit", "Returned"] },
    { kind: "subscription", statuses: ["Active", "Trialing", "PastDue", "Canceled"] },
    { kind: "invoice", statuses: ["Draft", "Sent", "Paid", "Overdue"] },
    { kind: "employee", statuses: ["Active", "Suspended", "Terminated"] },
  ];

  protected readonly typographyVariants: readonly TypographyVariant[] = [
    "h1",
    "h2",
    "h3",
    "h4",
    "h5",
    "h6",
    "body",
    "body-sm",
    "caption",
    "overline",
    "label",
    "stat",
  ];

  /** Flipped by the dismiss button so the dismissible alert's output is actually driveable. */
  protected readonly alertDismissed = signal(false);

  protected readonly lastAction = signal("—");
}
