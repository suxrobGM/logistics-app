import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Truck } from '@shared/models';
import { ApiService } from '@shared/services';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-edit-truck',
  templateUrl: './edit-truck.component.html',
  styleUrls: ['./edit-truck.component.scss']
})
export class EditTruckComponent implements OnInit {
  private truck?: Truck;
  public isBusy: boolean;
  public editMode: boolean;
  public form: FormGroup;
  public id?: string; 
  public headerText: string;
  
  constructor(
    private apiService: ApiService,
    private messageService: MessageService,
  ) 
  { 
    this.headerText = 'Edit a truck';
    this.isBusy = false;
    this.editMode = true;
    this.form = new FormGroup({
      'truckNumber': new FormControl(0, Validators.required),
      'driver': new FormControl(''),
    });
  }

  public ngOnInit(): void {
    this.id = history.state.id;
    
    if (!this.id) {
      this.editMode = false;
      this.headerText = 'Add a new truck'
      return;
    }

    this.fetchTruck(this.id);
  }

  public onSubmit() {
    const truck: Truck = {
      id: this.truck?.id,
      truckNumber: this.form.value.truckNumber,
      driverId: this.form.value.driver,
      driverName: this.form.value.driver,
    }
    
    this.isBusy = true;

    if (this.editMode) {
      this.apiService.updateTruck(truck).subscribe(result => {
        if (result.success) {
          this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: 'Truck has been updated successfully'});
        }
  
        this.isBusy = false;
      });
    }
    else {
      this.apiService.createTruck(truck).subscribe(result => {
        if (result.success) {
          this.messageService.add({key: 'notification', severity: 'success', summary: 'Notification', detail: 'Truck has been created successfully'});
        }
  
        this.isBusy = false;
      });
    }
  }

  private fetchTruck(id: string) {
    this.isBusy = true;
    this.apiService.getTruck(id).subscribe(result => {
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
}
