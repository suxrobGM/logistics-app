import { Component, computed, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import {
  Card,
  CountBadge,
  Divider,
  EmptyState,
  Icon,
  Skeleton,
  Stack,
  Typography,
} from "@logistics/shared/ui";

@Component({
  selector: "app-attention-panel",
  templateUrl: "./attention-panel.html",
  imports: [Card, CountBadge, Divider, EmptyState, Icon, RouterLink, Skeleton, Stack, Typography],
})
export class AttentionPanelComponent {
  public readonly unassignedLoadsCount = input<number>(0);
  public readonly idleTrucksCount = input<number>(0);
  public readonly isLoading = input<boolean>(false);

  protected readonly isAllClear = computed(
    () => this.unassignedLoadsCount() === 0 && this.idleTrucksCount() === 0,
  );
}
