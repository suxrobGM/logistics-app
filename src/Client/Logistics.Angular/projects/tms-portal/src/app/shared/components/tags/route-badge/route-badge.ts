import { Component, computed, input } from "@angular/core";
import type { Address } from "@logistics/shared/api";
import { AddressPipe } from "@logistics/shared/pipes";
import { TooltipModule } from "primeng/tooltip";

@Component({
  selector: "app-route-badge",
  templateUrl: "./route-badge.html",
  imports: [TooltipModule],
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

  protected readonly tooltipHtml = computed(() => {
    const originHtml = this.formatTooltipAddress(this.origin());
    const destHtml = this.formatTooltipAddress(this.destination());
    return (
      `<div class="flex items-start gap-3">` +
      `<div><div class="mb-1 text-xs font-semibold opacity-70">ORIGIN</div>${originHtml}</div>` +
      `<div class="mt-3 opacity-40">&rarr;</div>` +
      `<div><div class="mb-1 text-xs font-semibold opacity-70">DESTINATION</div>${destHtml}</div>` +
      `</div>`
    );
  });

  private formatTooltipAddress(addr: Address): string {
    const parts: string[] = [];
    if (addr.line1) parts.push(addr.line1);
    if (addr.line2) parts.push(addr.line2);
    const cityLine = [addr.city, addr.state, addr.zipCode].filter(Boolean).join(", ");
    if (cityLine) parts.push(cityLine);
    if (addr.country && addr.country !== "US" && addr.country !== "USA") parts.push(addr.country);
    return parts.join("<br>");
  }
}
