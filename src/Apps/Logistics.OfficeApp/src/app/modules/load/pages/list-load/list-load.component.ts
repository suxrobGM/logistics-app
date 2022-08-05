import { Component, OnInit } from '@angular/core';
import { Load } from '@shared/models';
import { ApiService } from '@shared/services';
import { LazyLoadEvent } from 'primeng/api';

@Component({
  selector: 'app-list-load',
  templateUrl: './list-load.component.html',
  styleUrls: ['./list-load.component.scss']
})
export class ListLoadComponent implements OnInit {
  public loads: Load[];
  public isBusy: boolean;
  public totalRecords: number;
  public first: number;

  constructor(private apiService: ApiService) {
    this.loads = [];
    this.isBusy = false;
    this.totalRecords = 0;
    this.first = 0;
  }

  public ngOnInit(): void {
    this.isBusy = true;
  }

  public search(event: any) {
    const query = event.target.value;
    //this.isBusy = true;
    
    this.apiService.getLoads(query, 1).subscribe(result => {
      if (result.success && result.items) {
        this.loads = result.items;
        this.totalRecords = result.itemsCount!;
      }

      //this.isBusy = false;
    });
  }

  public load(event: LazyLoadEvent) {
    this.isBusy = true;
    const page = event.first! / event.rows! + 1;
    
    this.apiService.getLoads(undefined, page).subscribe(result => {
      if (result.success && result.items) {
        this.loads = result.items;
        this.totalRecords = result.itemsCount!;
      }

      this.isBusy = false;
    });
  }
}
