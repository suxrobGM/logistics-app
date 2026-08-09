import { booleanAttribute, Component, computed, inject, input } from "@angular/core";
import { TenantService } from "@/core/services";

/** Tenant logo plus company name, at the top of the sidebar rail and the mobile drawer. */
@Component({
  selector: "app-brand-header",
  templateUrl: "./brand-header.html",
  styleUrl: "./brand-header.css",
  host: {
    "[class.collapsed]": "collapsed()",
    "[class.inline]": "inline()",
  },
})
export class BrandHeader {
  private readonly tenantService = inject(TenantService);

  /** The drawer sets logo and name side by side; the rail stacks them. */
  public readonly inline = input(false, { transform: booleanAttribute });
  public readonly collapsed = input(false, { transform: booleanAttribute });

  protected readonly companyName = computed(
    () => this.tenantService.tenantData()?.companyName ?? null,
  );
  protected readonly companyLogoUrl = computed(
    () => this.tenantService.tenantData()?.logoUrl ?? null,
  );
}
