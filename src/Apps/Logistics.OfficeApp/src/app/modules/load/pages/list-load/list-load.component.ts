import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { LazyLoadEvent } from 'primeng/api';
import { Load } from '@shared/models';
import { ApiService } from '@shared/services';
import { LoadStatus, LoadStatuses } from '@shared/types';

@Component({
  selector: 'app-list-load',
  templateUrl: './list-load.component.html',
  styleUrls: ['./list-load.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class ListLoadComponent implements OnInit {
  loads: Load[];
  loading: boolean;
  totalRecords: number;
  first: number;

  constructor(private apiService: ApiService) {
    this.loads = [];
    this.loading = false;
    this.totalRecords = 0;
    this.first = 0;
  }

  ngOnInit(): void {
    this.loading = true;
  }

  search(event: any) {
    const query = event.target.value;
    
    this.apiService.getLoads(query, '', 1).subscribe(result => {
      if (result.success && result.items) {
        this.loads = result.items;
        this.totalRecords = result.itemsCount!;
      }
    });
  }

  load(event: LazyLoadEvent) {
    this.loading = true;
    const page = event.first! / event.rows! + 1;
    const sortField = this.apiService.parseSortProperty(event.sortField, event.sortOrder);
    
    this.apiService.getLoads('', sortField, page, event.rows).subscribe(result => {
      if (result.success && result.items) {
        this.loads = result.items;
        this.totalRecords = result.itemsCount!;
      }

      this.loading = false;
    });
  }

  getLoadStatusName(status: LoadStatus): string {
    return LoadStatuses[status - 1].displayName
  }
}
