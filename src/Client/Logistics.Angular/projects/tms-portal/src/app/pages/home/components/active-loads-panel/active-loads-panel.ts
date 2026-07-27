import { Component, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import type { LoadDto } from "@logistics/shared/api";
import {
  Card,
  Divider,
  Icon,
  Stack,
  StatusBadge,
  Typography,
  UiButton,
  UiDataTable,
  UiTooltip,
} from "@logistics/shared/ui";
import { LoadProgressBarComponent } from "../load-progress-bar/load-progress-bar";

@Component({
  selector: "app-active-loads-panel",
  templateUrl: "./active-loads-panel.html",
  imports: [
    Card,
    Divider,
    Icon,
    LoadProgressBarComponent,
    RouterLink,
    Stack,
    StatusBadge,
    Typography,
    UiButton,
    UiDataTable,
    UiTooltip,
  ],
})
export class ActiveLoadsPanel {
  public readonly loads = input<LoadDto[]>([]);
  public readonly isLoading = input(false);
  /** Solo operators see their own book of work, not a dispatcher's queue. */
  public readonly title = input("Active Loads");
}
