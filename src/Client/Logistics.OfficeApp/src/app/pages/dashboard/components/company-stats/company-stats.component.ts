import {CommonModule, CurrencyPipe} from "@angular/common";
import {Component, OnInit} from "@angular/core";
import {SharedModule} from "primeng/api";
import {CardModule} from "primeng/card";
import {ChartModule} from "primeng/chart";
import {SkeletonModule} from "primeng/skeleton";
import {ApiService} from "@/core/api";
import {CompanyStatsDto} from "@/core/models";
import {Converters} from "@/core/utils";

@Component({
  selector: "app-company-stats",
  standalone: true,
  templateUrl: "./company-stats.component.html",
  styleUrls: [],
  imports: [
    CommonModule,
    CurrencyPipe,
    CardModule,
    CommonModule,
    SkeletonModule,
    SharedModule,
    ChartModule,
  ],
})
export class CompanyStatsComponent implements OnInit {
  public isLoading: boolean;
  public rpm: number;
  public companyStats?: CompanyStatsDto;

  constructor(private apiService: ApiService) {
    this.isLoading = false;
    this.rpm = 0;
  }

  ngOnInit(): void {
    this.fetchCompanyStats();
  }

  calcRpm(gross?: number, distance?: number): number {
    if (gross == null || distance == null) {
      return 0;
    }

    return gross / Converters.metersTo(distance, "mi");
  }

  private fetchCompanyStats() {
    this.isLoading = true;

    this.apiService.getCompanyStats().subscribe((result) => {
      if (result.success && result.data) {
        const stats = result.data;
        this.companyStats = result.data;
        this.rpm = stats.totalGross / Converters.metersTo(stats.totalDistance, "mi");
      }

      this.isLoading = false;
    });
  }
}
