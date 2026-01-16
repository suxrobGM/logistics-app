import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'cp-shipments-list',
  standalone: true,
  imports: [RouterLink, TableModule, ButtonModule],
  templateUrl: './shipments-list.html',
})
export class ShipmentsList {}
