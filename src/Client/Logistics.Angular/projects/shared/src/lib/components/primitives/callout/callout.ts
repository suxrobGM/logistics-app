import { booleanAttribute, Component, computed, input, output } from "@angular/core";
import { Icon } from "../icon/icon";

export type CalloutIntent = "info" | "success" | "warning" | "danger" | "neutral";

const intentClasses: Record<CalloutIntent, string> = {
  info: "border-[var(--info)]/40 bg-subtle",
  success: "border-[var(--success)]/40 bg-subtle",
  warning: "border-[var(--warning)]/40 bg-subtle",
  danger: "border-[var(--danger)]/40 bg-subtle",
  neutral: "border-default bg-subtle",
};

const intentDefaultIcon: Record<CalloutIntent, string> = {
  info: "info-circle",
  success: "check-circle",
  warning: "exclamation-triangle",
  danger: "times-circle",
  neutral: "info-circle",
};

const intentIconColor: Record<CalloutIntent, "info" | "success" | "warning" | "danger" | "muted"> =
  {
    info: "info",
    success: "success",
    warning: "warning",
    danger: "danger",
    neutral: "muted",
  };

/**
 * Themed inline alert/info box. `intent` (info/success/warning/danger/neutral)
 * picks the icon and accent color; supply `title`, body content, and optional dismiss.
 */
@Component({
  selector: "ui-callout",
  templateUrl: "./callout.html",
  imports: [Icon],
})
export class Callout {
  public readonly intent = input<CalloutIntent>("info");
  public readonly title = input<string | null>(null);
  public readonly icon = input<string | null>(null);
  public readonly dismissible = input<boolean, unknown>(false, { transform: booleanAttribute });
  public readonly dismiss = output<void>();

  protected readonly wrapperClasses = computed(
    () => `flex gap-3 rounded-lg border p-4 ${intentClasses[this.intent()]}`,
  );

  protected readonly resolvedIcon = computed(() => this.icon() ?? intentDefaultIcon[this.intent()]);

  protected readonly iconColor = computed(() => intentIconColor[this.intent()]);

  protected handleDismiss(): void {
    this.dismiss.emit();
  }
}
