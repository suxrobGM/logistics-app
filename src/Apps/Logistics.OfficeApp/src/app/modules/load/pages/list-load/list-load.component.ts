import { AfterViewInit, Component, OnInit } from '@angular/core';
import { Load } from '@app/shared/models/load';
import { LoadService } from '../../shared/load.service';

@Component({
  selector: 'app-list-load',
  templateUrl: './list-load.component.html',
  styleUrls: ['./list-load.component.scss']
})
export class ListLoadComponent implements AfterViewInit {
  loads!: Load[]
  constructor(private loadService: LoadService) { }

  ngAfterViewInit(): void {
    this.loadService.getLoads().subscribe(i => console.log(i));
  }

}
