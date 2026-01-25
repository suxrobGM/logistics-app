import { Component, input } from "@angular/core";

/**
 * Layout component for public-facing pages with tenant branding.
 */
@Component({
  selector: "cp-public-layout",
  templateUrl: "./public-layout.html",
})
export class PublicLayout {
  public readonly tenantName = input<string | null | undefined>(null);
  public readonly tenantLogoUrl = input<string | null | undefined>(null);
  public readonly subtitle = input("Track your shipment status");
}
