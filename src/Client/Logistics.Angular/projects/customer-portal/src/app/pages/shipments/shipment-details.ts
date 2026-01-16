import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TimelineModule } from 'primeng/timeline';

@Component({
  selector: 'cp-shipment-details',
  standalone: true,
  imports: [RouterLink, CardModule, ButtonModule, TimelineModule],
  templateUrl: './shipment-details.html',
})
export class ShipmentDetails {
  id = input.required<string>();
}
