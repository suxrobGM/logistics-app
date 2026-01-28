import { Component, computed, inject, model } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ButtonModule } from "primeng/button";
import { CheckboxModule } from "primeng/checkbox";
import { DialogModule } from "primeng/dialog";
import { DividerModule } from "primeng/divider";
import { OrderListModule } from "primeng/orderlist";
import { SelectButtonModule } from "primeng/selectbutton";
import {
  type DashboardPanelConfig,
  DashboardSettingsService,
  type PanelSize,
  type PanelType,
} from "@/core/services";

interface SizeOption {
  label: string;
  value: PanelSize;
}

@Component({
  selector: "app-dashboard-settings-dialog",
  templateUrl: "./dashboard-settings-dialog.html",
  styleUrl: "./dashboard-settings-dialog.css",
  imports: [
    DialogModule,
    ButtonModule,
    CheckboxModule,
    OrderListModule,
    SelectButtonModule,
    DividerModule,
    FormsModule,
  ],
})
export class DashboardSettingsDialog {
  private readonly settingsService = inject(DashboardSettingsService);

  public readonly visible = model(false);

  protected readonly panels = computed(() => this.settingsService.sortedPanels());

  protected readonly sizeOptions: SizeOption[] = [
    { label: "S", value: "small" },
    { label: "M", value: "medium" },
    { label: "L", value: "large" },
    { label: "Full", value: "full" },
  ];

  protected toggleVisibility(panelId: PanelType): void {
    this.settingsService.togglePanelVisibility(panelId);
  }

  protected updateSize(panelId: PanelType, size: PanelSize): void {
    this.settingsService.updatePanel(panelId, { size });
  }

  protected onReorder(event: { value: DashboardPanelConfig[] }): void {
    this.settingsService.reorderPanels(event.value.map((p) => p.id));
  }

  protected resetDefaults(): void {
    this.settingsService.resetToDefaults();
  }

  protected close(): void {
    this.visible.set(false);
  }
}
