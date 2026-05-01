import { ChangeDetectionStrategy, Component, computed, input, output } from "@angular/core";
import { ButtonModule } from "primeng/button";
import { Stack } from "../primitives/stack/stack";
import { Typography } from "../primitives/typography/typography";

type Severity = "info" | "success" | "warning" | "danger" | null;

const SEVERITY_RING: Record<Exclude<Severity, null>, string> = {
  info: "bg-info/10",
  success: "bg-success/10",
  warning: "bg-warning/10",
  danger: "bg-danger/10",
};

const SEVERITY_COLOR: Record<Exclude<Severity, null>, string> = {
  info: "var(--info)",
  success: "var(--success)",
  warning: "var(--warning)",
  danger: "var(--danger)",
};

/**
 * Empty state component for when there's no data to display.
 */
@Component({
  selector: "ui-empty-state",
  templateUrl: "./empty-state.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ButtonModule, Stack, Typography],
})
export class EmptyState {
  /** Title displayed above the message */
  public readonly title = input("No data found");

  /** Description message */
  public readonly message = input("There are no items to display.");

  /** PrimeNG icon class (without 'pi pi-' prefix) */
  public readonly icon = input("inbox");

  /** Label for the optional action button */
  public readonly actionLabel = input<string | null>(null);

  /** Icon for the action button */
  public readonly actionIcon = input("pi pi-plus");

  /**
   * Optional severity tint for the icon. Default `null` renders a neutral muted
   * icon; passing a severity adds a soft tinted ring background + matching color.
   */
  public readonly severity = input<Severity>(null);

  /** Emitted when the action button is clicked */
  public readonly action = output<void>();

  protected readonly piIconClass = computed(() => `pi pi-${this.icon().replace(/^pi-?/, "")}`);

  protected readonly severityRingClass = computed(() => {
    const sev = this.severity();
    return sev ? SEVERITY_RING[sev] : "";
  });

  protected readonly severityIconStyle = computed(() => {
    const sev = this.severity();
    return sev ? { color: SEVERITY_COLOR[sev] } : null;
  });

  protected onAction(): void {
    this.action.emit();
  }
}
