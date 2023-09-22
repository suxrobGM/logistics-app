import {Truck} from '@core/models';
import {ApiService} from '@core/services';
import {Observable, map} from 'rxjs';


export class TruckHelper {
  static findTruckDrivers(apiService: ApiService, searchQuery: string): Observable<TruckData[]> {
    return apiService.getTruckDrivers(searchQuery).pipe(
        map((result) => {
          if (!result.success || !result.items) {
            return [];
          }

          return result.items.map((truckDriver) => ({
            truckId: truckDriver.truck.id,
            driversName: TruckHelper.formatDriversName(truckDriver.truck),
          }));
        }),
    );
  }

  static formatDriversName(truck: Truck): string {
    const driversName = truck.drivers.map((driver) => driver.fullName);
    return `${truck.truckNumber} - ${driversName}`;
  }
}

export interface TruckData {
  driversName: string,
  truckId: string;
}
