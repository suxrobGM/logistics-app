import {CommonModule} from "@angular/common";
import {Component, input, signal} from "@angular/core";
import {InvoiceDto, TenantDto} from "@/core/models";
import {AddressPipe} from "@/core/pipes";
import {TenantService} from "@/core/services";

@Component({
  selector: "app-invoice-details",
  standalone: true,
  templateUrl: "./invoice-details.component.html",
  imports: [CommonModule, AddressPipe],
})
export class InvoiceDetailsComponent {
  readonly tenantData = signal<TenantDto | null>(null);
  readonly invoice = input.required<InvoiceDto>();

  constructor(readonly tenantService: TenantService) {
    this.tenantData.set(this.tenantService.getTenantData());
  }
}
