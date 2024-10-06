import {Component, Input} from '@angular/core';
import {CommonModule} from '@angular/common';
import {InvoiceDto, TenantDto} from '@/core/models';
import {TenantService} from '@/core/services';
import {AddressPipe} from '@/core/pipes';


@Component({
  selector: 'app-invoice-details',
  standalone: true,
  templateUrl: './invoice-details.component.html',
  imports: [
    CommonModule,
    AddressPipe,
  ],
})
export class InvoiceDetailsComponent {
  public tenantData: TenantDto | null;
  @Input({required: true}) invoice!: InvoiceDto;

  constructor(private readonly tenantService: TenantService) {
    this.tenantData = tenantService.getTenantData();
  }
}
