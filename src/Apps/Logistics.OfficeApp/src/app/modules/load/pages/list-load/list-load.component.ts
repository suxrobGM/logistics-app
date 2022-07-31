import { AfterViewInit, Component, OnInit } from '@angular/core';
import { Load } from '@shared/models/load';
import { ApiClientService } from '@shared/services/api-client.service';

@Component({
  selector: 'app-list-load',
  templateUrl: './list-load.component.html',
  styleUrls: ['./list-load.component.scss']
})
export class ListLoadComponent implements AfterViewInit {
  loads!: Load[]
  constructor(private apiService: ApiClientService) { }

  ngAfterViewInit(): void {
    this.apiService.getLoads().subscribe(i => console.log(i));
  }
}
