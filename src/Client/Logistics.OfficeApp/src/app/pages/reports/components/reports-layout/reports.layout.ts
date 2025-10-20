import { CommonModule } from "@angular/common";
import { Component } from "@angular/core";
import { RouterOutlet } from "@angular/router";

@Component({
  selector: "app-reports-layout",
  templateUrl: "./reports.layout.html",
  imports: [CommonModule, RouterOutlet],
})
export class ReportsLayoutComponent {}
