import { Component, input, signal } from "@angular/core";
import type { TruckGeolocationDto } from "@logistics/shared/api/models";
import {
  CountBadge,
  Icon,
  Stack,
  Surface,
  Typography,
  UiButton,
  UiDialog,
  UiTooltip,
} from "@logistics/shared/ui";
import { GeolocationMap } from "@/shared/components";

/** Fleet map card for the dispatch right panel: fills its flex-1 slot and expands to a dialog. */
@Component({
  selector: "app-fleet-map-card",
  templateUrl: "./fleet-map-card.html",
  host: { class: "flex min-h-0 flex-col" },
  imports: [
    CountBadge,
    GeolocationMap,
    Icon,
    Stack,
    Surface,
    Typography,
    UiButton,
    UiDialog,
    UiTooltip,
  ],
})
export class FleetMapCard {
  public readonly truckLocations = input.required<TruckGeolocationDto[]>();

  protected readonly expanded = signal(false);
}
