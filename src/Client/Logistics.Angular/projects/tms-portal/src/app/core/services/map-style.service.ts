import { isPlatformBrowser } from "@angular/common";
import { Injectable, PLATFORM_ID, computed, inject, signal } from "@angular/core";
import { ThemeService } from "./theme.service";
import type { MapLayerType } from "@/shared/components/maps/types";

const MAP_LAYER_STORAGE_KEY = "tms-map-layer";

/** Mapbox style URLs for each layer type and theme */
const MAP_STYLES: Record<MapLayerType, { light: string; dark: string }> = {
  streets: {
    light: "mapbox://styles/mapbox/streets-v12",
    dark: "mapbox://styles/mapbox/dark-v11",
  },
  satellite: {
    light: "mapbox://styles/mapbox/satellite-streets-v12",
    dark: "mapbox://styles/mapbox/satellite-streets-v12",
  },
  terrain: {
    light: "mapbox://styles/mapbox/outdoors-v12",
    dark: "mapbox://styles/mapbox/outdoors-v12",
  },
};

/**
 * Service for managing map layer styles with theme awareness.
 * Provides consistent map styling across all map components.
 */
@Injectable({ providedIn: "root" })
export class MapStyleService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly themeService = inject(ThemeService);

  /** Currently active map layer type */
  public readonly activeLayer = signal<MapLayerType>("streets");

  /** Available layer options for UI */
  public readonly layerOptions: { value: MapLayerType; label: string; icon: string }[] = [
    { value: "streets", label: "Streets", icon: "pi-map" },
    { value: "satellite", label: "Satellite", icon: "pi-globe" },
    { value: "terrain", label: "Terrain", icon: "pi-compass" },
  ];

  /** Current Mapbox style URL based on active layer and theme */
  public readonly currentStyle = computed(() => {
    const layer = this.activeLayer();
    const theme = this.themeService.isDark() ? "dark" : "light";
    return MAP_STYLES[layer][theme];
  });

  constructor() {
    this.initializeLayer();
  }

  /** Set the active map layer */
  public setLayer(layer: MapLayerType): void {
    this.activeLayer.set(layer);
    this.persistLayer(layer);
  }

  private initializeLayer(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    const stored = localStorage.getItem(MAP_LAYER_STORAGE_KEY) as MapLayerType | null;
    if (stored && this.isValidLayer(stored)) {
      this.activeLayer.set(stored);
    }
  }

  private persistLayer(layer: MapLayerType): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(MAP_LAYER_STORAGE_KEY, layer);
    }
  }

  private isValidLayer(layer: string): layer is MapLayerType {
    return ["streets", "satellite", "terrain"].includes(layer);
  }
}
