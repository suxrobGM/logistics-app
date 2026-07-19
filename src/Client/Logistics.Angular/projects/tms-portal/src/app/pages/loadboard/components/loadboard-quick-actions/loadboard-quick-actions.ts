import { Component, inject } from "@angular/core";
import { Router } from "@angular/router";
import { Card, Grid, Icon, Stack, Typography, type IconName } from "@logistics/shared/ui";

interface QuickAction {
  label: string;
  description: string;
  icon: IconName;
  route: string;
  iconColor: "info" | "success" | "warning";
}

@Component({
  selector: "app-loadboard-quick-actions",
  templateUrl: "./loadboard-quick-actions.html",
  imports: [Card, Grid, Icon, Stack, Typography],
})
export class LoadBoardQuickActions {
  protected readonly actions: readonly QuickAction[] = [
    {
      label: "Search Loads",
      description: "Find available freight from load boards",
      icon: "search",
      route: "/loadboard/search",
      iconColor: "info",
    },
    {
      label: "Post a Truck",
      description: "Advertise your available trucks",
      icon: "truck",
      route: "/loadboard/posted-trucks",
      iconColor: "success",
    },
    {
      label: "Configure Providers",
      description: "Connect to DAT, Truckstop, 123Loadboard",
      icon: "settings",
      route: "/loadboard/providers",
      iconColor: "warning",
    },
  ];

  private readonly router = inject(Router);

  protected navigate(route: string): void {
    this.router.navigateByUrl(route);
  }
}
