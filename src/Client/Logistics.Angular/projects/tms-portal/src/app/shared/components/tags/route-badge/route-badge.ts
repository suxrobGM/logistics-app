import { Component, computed, input } from "@angular/core";
import type { Address } from "@logistics/shared/api";
import { AddressPipe } from "@logistics/shared/pipes";
import { Icon, UiTooltip } from "@logistics/shared/ui";

@Component({
  selector: "app-route-badge",
  templateUrl: "./route-badge.html",
  imports: [Icon, UiTooltip],
})
export class RouteBadge {
  public readonly origin = input.required<Address>();
  public readonly destination = input.required<Address>();

  private readonly addressPipe = new AddressPipe();

  protected readonly originShort = computed(() =>
    this.addressPipe.transform(this.origin(), "short"),
  );
  protected readonly destinationShort = computed(() =>
    this.addressPipe.transform(this.destination(), "short"),
  );

  protected readonly originLines = computed(() => this.addressLines(this.origin()));
  protected readonly destinationLines = computed(() => this.addressLines(this.destination()));

  /**
   * The address as plain LINES for the tooltip template to render.
   *
   * Returns data, not markup, so the template can escape each line: these fields come from
   * tenant-supplied address data, and rendering them as raw HTML would let a crafted address field
   * inject markup into the page.
   */
  private addressLines(addr: Address): string[] {
    const lines: string[] = [];
    if (addr.line1) lines.push(addr.line1);
    if (addr.line2) lines.push(addr.line2);
    const cityLine = [addr.city, addr.state, addr.zipCode].filter(Boolean).join(", ");
    if (cityLine) lines.push(cityLine);
    if (addr.country && addr.country !== "US" && addr.country !== "USA") lines.push(addr.country);
    return lines;
  }
}
