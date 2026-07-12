import { Component, signal } from "@angular/core";
import {
  Typography,
  UiAccordionImports,
  UiButton,
  UiDrawer,
  UiMenu,
  UiPopover,
  UiStepperImports,
  UiTabsImports,
  UiTimeline,
  UiTimelineContent,
  UiTimelineMarker,
  type UiMenuItem,
} from "@logistics/shared/ui";

interface LabEvent {
  readonly title: string;
  readonly at: string;
}

/**
 * S9 — the behavioural tail: menu, tabs, accordion, popover, drawer, stepper, timeline.
 *
 * These are the seven components whose bugs are INVISIBLE to every other gate. A menu that opens and
 * never closes, a tab strip where no tab is selected, an overlay that never portals — all of them
 * compile, lint and test green. Each block below is here to be CLICKED.
 *
 * The menu row deliberately reproduces the exact call-site shape from the 19 real ones: a kebab that
 * sets the row first and then toggles, so a menu built from a stale row shows up here as the wrong
 * name in the output line.
 */
@Component({
  selector: "app-ui-lab-behaviour",
  templateUrl: "./behaviour-section.html",
  imports: [
    Typography,
    UiAccordionImports,
    UiButton,
    UiDrawer,
    UiMenu,
    UiPopover,
    UiStepperImports,
    UiTabsImports,
    UiTimeline,
    UiTimelineContent,
    UiTimelineMarker,
  ],
})
export class UiLabBehaviourSection {
  protected readonly rows = ["LOAD-1001", "LOAD-1002", "LOAD-1003"];
  protected readonly selectedRow = signal<string | null>(null);
  protected readonly lastCommand = signal("—");

  /** Mirrors the real row-kebab menus: separator, a hidden item, a disabled item, a destructive one. */
  protected readonly actionMenuItems = signal<UiMenuItem[]>([]);

  protected readonly activeTab = signal(0);
  protected readonly activeStep = signal(1);
  protected readonly drawerOpen = signal(false);

  protected readonly events: readonly LabEvent[] = [
    { title: "Picked up", at: "08:12" },
    { title: "In transit", at: "11:40" },
    { title: "Delivered", at: "17:05" },
  ];

  public constructor() {
    this.actionMenuItems.set([
      { label: "View details", icon: "eye", command: () => this.run("view") },
      { label: "Edit", icon: "square-pen", command: () => this.run("edit") },
      { separator: true },
      { label: "Never visible", icon: "box", visible: false, command: () => this.run("BUG") },
      { label: "Disabled", icon: "clock", disabled: true, command: () => this.run("BUG") },
      { separator: true },
      { label: "Delete", icon: "trash", variant: "destructive", command: () => this.run("delete") },
    ]);
  }

  /** `[linear]` must stop this from skipping ahead when driven from the step headers. */
  protected goToStep(step: number): void {
    this.activeStep.set(step);
  }

  protected nextStep(): void {
    this.activeStep.update((s) => Math.min(3, s + 1));
  }

  protected prevStep(): void {
    this.activeStep.update((s) => Math.max(1, s - 1));
  }

  private run(action: string): void {
    this.lastCommand.set(`${action} → ${this.selectedRow() ?? "(no row)"}`);
  }
}
