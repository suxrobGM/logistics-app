import { Component, OnInit } from '@angular/core';
import { ApiService } from '@shared/services';
import { DateUtils } from '@shared/utils';

@Component({
  selector: 'app-overview',
  templateUrl: './overview.component.html',
  styleUrls: ['./overview.component.scss']
})
export class ReportPageComponent implements OnInit {

  constructor(
    private apiService: ApiService,
    private dateUtils: DateUtils,
  )
  {
  }

  public ngOnInit(): void {
    this.fetchMonthlyGrosses();
  }

  private fetchMonthlyGrosses() {
    const thisYear = this.dateUtils.thisYear();
    this.apiService.getMonthlyGrosses(thisYear)
      .subscribe(result => {
        console.log(result.value);
      });
  }
}
