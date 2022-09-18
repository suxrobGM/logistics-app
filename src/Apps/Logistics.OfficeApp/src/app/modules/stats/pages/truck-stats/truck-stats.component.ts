import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-truck-stats',
  templateUrl: './truck-stats.component.html',
  styleUrls: ['./truck-stats.component.scss']
})
export class TruckStatsComponent implements OnInit {
  id?: string;

  constructor(private route: ActivatedRoute) 
  {
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.id = params['id'];
    });
  }

}
