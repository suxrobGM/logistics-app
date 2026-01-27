import { Component, computed, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { SkeletonModule } from "primeng/skeleton";

@Component({
  selector: "app-team-overview-widget",
  templateUrl: "./team-overview-widget.html",
  imports: [CardModule, DividerModule, SkeletonModule, RouterLink],
})
export class TeamOverviewWidgetComponent {
  public readonly employeesCount = input(0);
  public readonly driversCount = input(0);
  public readonly dispatchersCount = input(0);
  public readonly managersCount = input(0);
  public readonly trucksCount = input(0);
  public readonly idleTrucksCount = input(0);
  public readonly isLoading = input(false);

  protected readonly activeTrucks = computed(() => {
    return this.trucksCount() - this.idleTrucksCount();
  });

  protected readonly truckUtilization = computed(() => {
    const total = this.trucksCount();
    if (total === 0) return 0;
    return Math.round((this.activeTrucks() / total) * 100);
  });
}
