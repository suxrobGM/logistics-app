import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Truck } from '@shared/models/truck';
import { ApiClientService } from '@shared/services/api-client.service';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-edit-truck',
  templateUrl: './edit-truck.component.html',
  styleUrls: ['./edit-truck.component.scss']
})
export class EditTruckComponent implements OnInit {
  private truck?: Truck;
  public isBusy = false;
  public form: FormGroup;
  public id?: string; 
  
  constructor(
    private apiService: ApiClientService,
    private messageService: MessageService,
  ) 
  { 
    this.form = new FormGroup({
      'truckNumber': new FormControl(0, Validators.required),
      'driver': new FormControl(''),
    });
  }

  public ngOnInit(): void {
    this.id = history.state.id;
    
    if (!this.id) {
      this.messageService.add({key: 'notification', severity: 'error', summary: 'Error', detail: 'ID is an empty'});
      return;
    }

    this.isBusy = true;
    this.apiService.getTruck(this.id).subscribe(result => {
      if (result.success && result.value) {
        this.truck = result.value;
        
        this.form.patchValue({
          truckNumber: this.truck?.truckNumber,
          driver: this.truck?.driverName,
        });
      }

      this.isBusy = false;
    });
  }

  public onSubmit() {
    const truck: Truck = {
      id: this.truck?.id,
      truckNumber: this.form.value.truckNumber,
      driverId: this.form.value.driver,
      driverName: this.form.value.driver,
    }
    
    this.isBusy = true;
    this.apiService.updateTruck(truck).subscribe(result => {
      if (result.success) {
        this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: 'User has been updated successfully'});
      }

      this.isBusy = false;
    });
  }
}
