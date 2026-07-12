import { Component } from "@angular/core";

/** The panel area. Replaces `<p-tabpanels>`; it is purely a layout box. */
@Component({
  selector: "ui-tab-panels",
  templateUrl: "./tab-panels.html",
  host: { class: "flex-1" },
})
export class UiTabPanels {}
