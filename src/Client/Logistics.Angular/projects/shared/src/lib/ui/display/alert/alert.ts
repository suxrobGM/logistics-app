import { booleanAttribute, Component, computed, input, output } from "@angular/core";
import { Icon } from "../../icons/icon/icon";
import type { IconName } from "../../icons/icons";

export type CalloutIntent = "info" | "success" | "warning" | "danger" | "neutral";

const intentClasses: Record<CalloutIntent, string> = {
  info: "border-[var(--info)]/40 bg-subtle",
  success: "border-[var(--success)]/40 bg-subtle",
  warning: "border-[var(--warning)]/40 bg-subtle",
  danger: "border-[var(--danger)]/40 bg-subtle",
  neutral: "border-default bg-subtle",
};

const intentDefaultIcon: Record<CalloutIntent, IconName> = {
  info: "info",
  success: "circle-check",
  warning: "triangle-alert",
  danger: "circle-x",
  neutral: "info",
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
 *
 * Absorbs `<p-message>` (10 sites, content-projected, `severity` its only input). The vocabularies
 * already line up — the sweep maps `warn`→`warning`, `error`→`danger`, `secondary`→`neutral` — so no
 * new intent was needed. (The brief expected `secondary` to be missing; `neutral` IS it, and it is
 * the grey p-message paints.)
 *
 * `class: "block"` on the host is the one thing that had to change, and it would have failed
 * quietly: an Angular component host is `display: inline` by default, and 3 of those 10 call sites
 * pass `class="mb-4 w-full"`. `w-full` does nothing on an inline box, so the alert would have shrunk
 * to fit its text instead of filling its column — on exactly the pages that use a full-width warning
 * banner to say something important.
 */
@Component({
  selector: "ui-alert",
  templateUrl: "./alert.html",
  imports: [Icon],
  host: { class: "block" },
})
export class Alert {
  public readonly intent = input<CalloutIntent>("info");
  public readonly title = input<string | null>(null);
  public readonly icon = input<IconName | null>(null);
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
