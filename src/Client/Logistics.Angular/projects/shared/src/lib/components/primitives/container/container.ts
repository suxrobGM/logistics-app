import { booleanAttribute, Component, computed, input } from "@angular/core";

export type ContainerMaxWidth = "xs" | "sm" | "md" | "lg" | "xl" | "full";

const maxWidthClasses: Record<ContainerMaxWidth, string> = {
  xs: "max-w-md",
  sm: "max-w-3xl",
  md: "max-w-5xl",
  lg: "max-w-7xl",
  xl: "max-w-screen-xl",
  full: "max-w-none",
};

/**
 * Page-width container. Centers content with `mx-auto` and caps width via `maxWidth`
 * (`xs` → `max-w-md`, `sm` → `max-w-3xl`, `md` → `max-w-5xl`, `lg` → `max-w-7xl`,
 * `xl` → `max-w-screen-xl`, `full` → unbounded). `gutters` controls horizontal padding.
 */
@Component({
  selector: "ui-container",
  templateUrl: "./container.html",
})
export class Container {
  public readonly maxWidth = input<ContainerMaxWidth>("lg");
  public readonly gutters = input<boolean, unknown>(true, { transform: booleanAttribute });

  protected readonly classes = computed(() => {
    const parts = ["mx-auto w-full", maxWidthClasses[this.maxWidth()]];
    if (this.gutters()) {
      parts.push("px-4 sm:px-6 lg:px-8");
    }
    return parts.join(" ");
  });
}
