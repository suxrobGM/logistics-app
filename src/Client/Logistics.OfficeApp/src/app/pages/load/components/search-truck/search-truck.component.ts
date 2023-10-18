import {Component, EventEmitter, Input, Output} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {AutoCompleteModule} from 'primeng/autocomplete';
import {ApiService} from '@core/services';
import {TruckHelper} from '@pages/load/shared';


@Component({
  selector: 'app-search-truck',
  standalone: true,
  templateUrl: './search-truck.component.html',
  styleUrls: [],
  imports: [
    CommonModule,
    AutoCompleteModule,
    FormsModule,
  ],
})
export class SearchTruckComponent {
  public suggestedTrucks: TruckData[] = [];
  @Input() selectedTruck: TruckData | null = null;
  @Output() selectedTruckChange = new EventEmitter<TruckData>();

  constructor(private readonly apiService: ApiService) {
  }

  searchTruck(event: {query: string}) {
    this.apiService.getTruckDrivers({search: event.query}).subscribe((result) => {
      if (!result.data) {
        return;
      }

      this.suggestedTrucks = result.data.map((truckDriver) => ({
        truckId: truckDriver.truck.id,
        driversName: TruckHelper.formatDriversName(truckDriver.truck.truckNumber, truckDriver.drivers.map((i) => i.fullName)),
      }));
    });
  }

  changeSelectedTruck(event: TruckData) {
    this.selectedTruckChange.emit(event);
  }
}

export interface TruckData {
  driversName: string,
  truckId: string;
}
