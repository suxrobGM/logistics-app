import { Component, OnInit } from '@angular/core';
import { Truck } from '@app/shared/models/truck';

@Component({
  selector: 'app-list-truck',
  templateUrl: './list-truck.component.html',
  styleUrls: ['./list-truck.component.scss']
})
export class ListTruckComponent implements OnInit {
  public trucks!: Truck[];

  constructor() { }

  ngOnInit(): void {
  }

}
