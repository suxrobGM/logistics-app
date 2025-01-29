import {Component, input} from "@angular/core";
import {CommonModule} from "@angular/common";
import {InvoiceDto, TenantDto} from "@/core/models";
import {TenantService} from "@/core/services";
import {AddressPipe} from "@/core/pipes";

@Component({
  selector: "app-invoice-details",
  standalone: true,
  templateUrl: "./invoice-details.component.html",
  imports: [CommonModule, AddressPipe],
})
export class InvoiceDetailsComponent {
  public tenantData: TenantDto | null;
  public readonly invoice = input.required<InvoiceDto>();

  constructor(private readonly tenantService: TenantService) {
    this.tenantData = tenantService.getTenantData();
  }
}
