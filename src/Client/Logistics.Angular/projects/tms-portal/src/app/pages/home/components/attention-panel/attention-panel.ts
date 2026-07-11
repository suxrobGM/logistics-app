import { Component, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Card, CountBadge, Divider, Icon, Skeleton, Stack, Typography } from "@logistics/shared/ui";

@Component({
  selector: "app-attention-panel",
  templateUrl: "./attention-panel.html",
  imports: [Card, CountBadge, Divider, Icon, RouterLink, Skeleton, Stack, Typography],
})
export class AttentionPanelComponent {
  public readonly unassignedLoadsCount = input<number>(0);
  public readonly idleTrucksCount = input<number>(0);
  public readonly isLoading = input<boolean>(false);
}
