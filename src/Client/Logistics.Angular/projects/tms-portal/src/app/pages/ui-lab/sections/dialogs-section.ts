import { Component, signal } from "@angular/core";
import {
  Typography,
  UiButton,
  UiCollapsible,
  UiDialog,
  UiSelectField,
  UiTextField,
} from "@logistics/shared/ui";

/**
 * `ui-dialog` and `ui-collapsible`.
 *
 * Every row here exists because a build and a full test run both stay GREEN through the failure it
 * demonstrates. None of these are type errors; all of them are obvious in a browser in five seconds:
 *
 *   1. WIDTH LANDS ON THE PORTALLED PANEL. CDK portals the panel out of <ui-dialog>, so a width on
 *      the host is a no-op and every dialog renders at Helm's default 28rem. The 450 / 650 / 80vw
 *      rows are here to be MEASURED, not admired — a 650px dialog that comes out 448px wide is the
 *      single most likely silent failure in this step.
 *
 *   2. THE BACKDROP MUST NOT CLOSE. No dialog in this app opts into closing on a mask click, but
 *      brain's default is the opposite. Click beside any dialog below: it must stay open, with
 *      whatever you typed still in it.
 *
 *   3. (closed) MUST NOT FIRE ON OPEN. 16 call sites reset their form in it. The event log row makes
 *      the ordering visible; the "prefilled" row makes the consequence visible — get it backwards and
 *      the edit dialog blanks itself as it appears.
 *
 *   4. A DIALOG MUST NOT STEAL A NESTED COMPONENT'S SLOTS. `#header` / `#content` inside a dialog in
 *      this repo belong to a nested accordion or table, never to the dialog (see dialog.ts). The
 *      "nested slots" row reproduces customer-edit-dialog's shape: the dialog's own title must stay
 *      "Edit Customer" and "Danger Zone" must stay inside the box.
 *
 *   5. ESCAPE BELONGS TO THE INNERMOST OVERLAY. A select opened inside a dialog is a SECOND CDK
 *      overlay stacked on the dialog's. Helm's select does not stop Escape from propagating, so one
 *      Escape could dismiss the dropdown AND discard the dialog behind it — with the half-filled
 *      form in it. The "select inside dialog" row reproduces the trip wizard's "Create a new load"
 *      shape. Type in the text box, open the select, press Escape ONCE: the dropdown closes, the
 *      dialog and your text stay.
 *
 * Drag any dialog by its header; grab the bottom-right grip to resize. Both are ON BY DEFAULT.
 */
@Component({
  selector: "app-ui-lab-dialogs",
  templateUrl: "./dialogs-section.html",
  imports: [Typography, UiButton, UiDialog, UiCollapsible, UiTextField, UiSelectField],
})
export class UiLabDialogsSection {
  protected readonly basic = signal(false);
  protected readonly wide = signal(false);
  protected readonly responsive = signal(false);
  protected readonly notClosable = signal(false);
  protected readonly noDragNoResize = signal(false);
  protected readonly nestedSlots = signal(false);
  protected readonly prefilled = signal(false);
  protected readonly breakpointed = signal(false);

  /**
   * The nested-overlay Escape row. A select opened INSIDE a dialog is a second CDK overlay stacked on
   * the dialog's, and Escape belongs to it alone. Helm's select does not stop the event from
   * propagating, so one Escape could dismiss the dropdown AND throw the dialog away with the form
   * still in it. `nestedText` is here so the data loss is VISIBLE, not just the dialog disappearing.
   */
  protected readonly nestedOverlay = signal(false);
  protected readonly nestedText = signal("");
  protected readonly nestedChoice = signal<string | null>(null);
  protected readonly nestedOptions = [
    { label: "Flatbed", value: "flatbed" },
    { label: "Reefer", value: "reefer" },
    { label: "Dry Van", value: "dry-van" },
  ];

  /** Proves (opened)/(closed) fire on the right transitions, in the right order. */
  protected readonly log = signal<readonly string[]>([]);

  /** The prefilled "edit" dialog: if (closed) fired on OPEN, this would be empty on screen. */
  protected readonly name = signal("Acme Logistics");

  protected readonly collapsedSeed = signal(true);

  protected note(event: string): void {
    this.log.update((entries) => [...entries, `${entries.length + 1}. ${event}`]);
  }

  /** What the 16 real `(onHide)` handlers do: reset. It must run on CLOSE and never on OPEN. */
  protected onPrefilledClosed(): void {
    this.note("closed -> reset()");
    this.name.set("Acme Logistics");
  }

  protected clearLog(): void {
    this.log.set([]);
  }
}
