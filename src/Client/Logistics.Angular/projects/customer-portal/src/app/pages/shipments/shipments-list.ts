import { Component } from "@angular/core";
import { RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { TableModule } from "primeng/table";

@Component({
  selector: "cp-shipments-list",
  standalone: true,
  imports: [RouterLink, TableModule, ButtonModule],
  templateUrl: "./shipments-list.html",
})
export class ShipmentsList {}
