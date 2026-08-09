import { Component, inject, input, output } from "@angular/core";
import { Icon, UiTooltip } from "@logistics/shared/ui";
import { CommandPaletteService } from "@/core/services";

/** The "Search... Ctrl+K" button in the sidebar rail and the mobile drawer. */
@Component({
  selector: "app-search-trigger",
  templateUrl: "./search-trigger.html",
  styleUrl: "./search-trigger.css",
  host: { "[class.collapsed]": "collapsed()" },
  imports: [Icon, UiTooltip],
})
export class SearchTrigger {
  private readonly commandPaletteService = inject(CommandPaletteService);

  public readonly collapsed = input(false);
  /** The drawer closes itself before the palette takes over the screen. */
  public readonly triggered = output<void>();

  protected open(): void {
    this.triggered.emit();
    this.commandPaletteService.open();
  }
}
