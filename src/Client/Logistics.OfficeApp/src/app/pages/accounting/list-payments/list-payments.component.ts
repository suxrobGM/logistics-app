import {Component} from '@angular/core';
import {CommonModule} from '@angular/common';
import {RouterModule} from '@angular/router';
import {CardModule} from 'primeng/card';
import {TableModule} from 'primeng/table';
import {TooltipModule} from 'primeng/tooltip';


@Component({
  selector: 'app-list-payments',
  standalone: true,
  templateUrl: './list-payments.component.html',
  styleUrls: ['./list-payments.component.scss'],
  imports: [
    CommonModule,
    CardModule,
    TableModule,
    TooltipModule,
    RouterModule,
  ],
})
export class ListPaymentsComponent {

}
