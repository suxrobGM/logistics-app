import { Component, input } from "@angular/core";
import { TagModule } from "primeng/tag";

export type BadgeSeverity = "info" | "success" | "warn" | "danger" | "secondary" | "contrast";
export type BadgeVariant = "solid" | "outlined";

/**
 * Generic badge/tag with severity and variant. For status-specific badges with
 * automatic severity resolution, use `<ui-status-badge>` instead.
 */
@Component({
  selector: "ui-badge",
  templateUrl: "./badge.html",
  imports: [TagModule],
  host: { class: "contents" },
})
export class Badge {
  public readonly value = input<string | number | null>(null);
  public readonly severity = input<BadgeSeverity>("info");
  public readonly variant = input<BadgeVariant>("solid");
  public readonly icon = input<string | null>(null);

  protected readonly iconClass = (): string | undefined => {
    const name = this.icon();
    return name ? `pi pi-${name.replace(/^pi-?/, "")}` : undefined;
  };
}
