import { Component, inject, input, output } from "@angular/core";
import { FormsModule } from "@angular/forms";
import {
  ControlComponent,
  FullscreenControlDirective,
  NavigationControlDirective,
  ScaleControlDirective,
} from "ngx-mapbox-gl";
import { ButtonModule } from "primeng/button";
import { SelectButtonModule } from "primeng/selectbutton";
import { TooltipModule } from "primeng/tooltip";
import { MapStyleService } from "@/core/services";
import type { MapControlPosition, MapLayerType } from "../types";

/**
 * Reusable map controls component providing navigation, fullscreen, scale,
 * fit-to-bounds, and layer toggle functionality.
 */
@Component({
  selector: "app-map-controls",
  templateUrl: "./map-controls.html",
  imports: [
    ControlComponent,
    NavigationControlDirective,
    FullscreenControlDirective,
    ScaleControlDirective,
    ButtonModule,
    SelectButtonModule,
    TooltipModule,
    FormsModule,
  ],
})
export class MapControls {
  protected readonly mapStyleService = inject(MapStyleService);

  /** Show zoom and compass controls */
  public readonly showNavigation = input(true);

  /** Show fullscreen toggle button */
  public readonly showFullscreen = input(true);

  /** Show scale bar */
  public readonly showScale = input(true);

  /** Show layer toggle */
  public readonly showLayerToggle = input(true);

  /** Position for navigation controls */
  public readonly navigationPosition = input<MapControlPosition>("top-right");

  /** Position for scale control */
  public readonly scalePosition = input<MapControlPosition>("bottom-left");

  /** Scale unit (metric or imperial) */
  public readonly scaleUnit = input<"metric" | "imperial">("imperial");

  /** Emitted when fit-to-bounds button is clicked */
  public readonly fitBoundsClick = output<void>();

  /** Emitted when layer type changes */
  public readonly layerChange = output<MapLayerType>();

  protected onFitBounds(): void {
    this.fitBoundsClick.emit();
  }

  protected onLayerChange(layer: MapLayerType): void {
    this.mapStyleService.setLayer(layer);
    this.layerChange.emit(layer);
  }
}
