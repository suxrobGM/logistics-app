import { Component, input } from "@angular/core";

/**
 * Layout component for public-facing pages with tenant branding.
 */
@Component({
  selector: "cp-public-page-layout",
  templateUrl: "./public-page-layout.html",
})
export class PublicPageLayout {
  public readonly tenantName = input<string | null | undefined>(null);
  public readonly tenantLogoUrl = input<string | null | undefined>(null);
  public readonly subtitle = input("Track your shipment status");
}
