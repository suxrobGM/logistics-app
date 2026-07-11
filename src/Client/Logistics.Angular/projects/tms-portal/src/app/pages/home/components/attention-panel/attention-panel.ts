import { Component, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Icon, Stack, Typography } from "@logistics/shared/ui";
import { BadgeModule } from "primeng/badge";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { SkeletonModule } from "primeng/skeleton";

@Component({
  selector: "app-attention-panel",
  templateUrl: "./attention-panel.html",
  imports: [
    CardModule,
    BadgeModule,
    DividerModule,
    RouterLink,
    SkeletonModule,
    Icon,
    Stack,
    Typography,
  ],
})
export class AttentionPanelComponent {
  public readonly unassignedLoadsCount = input<number>(0);
  public readonly idleTrucksCount = input<number>(0);
  public readonly isLoading = input<boolean>(false);
}
