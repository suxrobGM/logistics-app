import {Component, Input} from '@angular/core';
import {CommonModule} from '@angular/common';
import {Invoice, Tenant} from '@core/models';
import {TenantService} from '@core/services';


@Component({
  selector: 'app-invoice-details',
  standalone: true,
  templateUrl: './invoice-details.component.html',
  imports: [CommonModule],
})
export class InvoiceDetailsComponent {
  public tenantData: Tenant | null;
  @Input({required: true}) invoice!: Invoice;

  constructor(private readonly tenantService: TenantService) {
    this.tenantData = tenantService.getTenantData();
  }
}
