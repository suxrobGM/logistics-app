import { Component, computed, input } from "@angular/core";

type BackgroundVariant = "white" | "light" | "dark";
type MaxWidthVariant = "3xl" | "7xl";

@Component({
  selector: "web-section-container",
  templateUrl: "./section-container.html",
  host: {
    "[id]": "sectionId()",
    "[class]": "hostClasses()",
  },
})
export class SectionContainer {
  public readonly sectionId = input<string>();
  public readonly background = input<BackgroundVariant>("white");
  public readonly maxWidth = input<MaxWidthVariant>("7xl");

  protected hostClasses(): string {
    const bg = this.background();
    const baseClasses = "block py-24";

    switch (bg) {
      case "dark":
        return `${baseClasses} bg-linear-to-br from-slate-900 via-purple-900 to-slate-900`;
      case "light":
        return `${baseClasses} bg-slate-50`;
      default:
        return `${baseClasses} bg-white`;
    }
  }

  protected readonly containerClasses = computed(() => {
    const maxW = this.maxWidth() === "3xl" ? "max-w-3xl" : "max-w-7xl";
    return `mx-auto ${maxW} px-4 sm:px-6 lg:px-8`;
  });
}
