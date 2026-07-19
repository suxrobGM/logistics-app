import { Component } from "@angular/core";
import { ThemeToggle, Typography } from "@logistics/shared/ui";
import { UiLabBehaviourSection } from "./sections/behaviour-section";
import { UiLabButtonsSection } from "./sections/buttons-section";
import { UiLabCosmeticsSection } from "./sections/cosmetics-section";
import { UiLabDatavizSection } from "./sections/dataviz-section";
import { UiLabDetailsSection } from "./sections/details-section";
import { UiLabDialogsSection } from "./sections/dialogs-section";
import { UiLabFeedbackSection } from "./sections/feedback-section";
import { UiLabFormSlotsSection } from "./sections/form-slots-section";
import { UiLabFormsSection } from "./sections/forms-section";
import { UiLabIconsSection } from "./sections/icons-section";
import { UiLabNotificationsSection } from "./sections/notifications-section";
import { UiLabOverlaysSection } from "./sections/overlays-section";
import { UiLabTableSection } from "./sections/table-section";
import { UiLabTooltipSection } from "./sections/tooltip-section";

interface LabSection {
  readonly id: string;
  readonly label: string;
}

/**
 * `/ui-lab` - the component gallery. Dev-only (`canMatch: isDevMode` in `app.routes.ts`), lazy, and
 * deliberately inert: no `HttpClient`, no auth, no store, no SignalR. Everything on the page comes
 * from local fixtures, so it renders signed-out in about a second and a screenshot of it means
 * something.
 *
 * It exists because every real bug in the spartan migration so far - four wrapper default-drifts, a
 * pristine-invalid bug, a dropped `<ng-template>`, a missing `*hlmSelectPortal` that left an overlay
 * stuck open - was green on `build:all` and green on the whole test suite. None of them were
 * type errors. All of them were visible in a browser in under five seconds.
 *
 * Each section is its own component so no template here grows past a screen. Add a section by
 * dropping a child component in and adding its heading id to {@link sections}.
 */
@Component({
  selector: "app-ui-lab",
  templateUrl: "./ui-lab.html",
  imports: [
    ThemeToggle,
    Typography,
    UiLabIconsSection,
    UiLabButtonsSection,
    UiLabDetailsSection,
    UiLabTableSection,
    UiLabFormSlotsSection,
    UiLabFormsSection,
    UiLabOverlaysSection,
    UiLabFeedbackSection,
    UiLabCosmeticsSection,
    UiLabDialogsSection,
    UiLabTooltipSection,
    UiLabNotificationsSection,
    UiLabBehaviourSection,
    UiLabDatavizSection,
  ],
})
export class UiLab {
  /** Anchors the probe navigates by. Each id is the `id` of that section's `<section>` element. */
  protected readonly sections: readonly LabSection[] = [
    { id: "icons", label: "Icons" },
    { id: "buttons", label: "Buttons" },
    { id: "details", label: "Details" },
    { id: "table", label: "Table" },
    { id: "forms", label: "Forms" },
    { id: "overlays", label: "Overlays" },
    { id: "feedback", label: "Feedback" },
    { id: "cosmetics", label: "Cosmetics" },
    { id: "dialogs", label: "Dialogs" },
    { id: "tooltip", label: "Tooltip" },
    { id: "notifications", label: "Notifications" },
    { id: "behaviour", label: "Behaviour" },
    { id: "dataviz", label: "Data viz" },
  ];
}
