import { Component } from "@angular/core";
import { RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";

@Component({
  selector: "cp-dashboard",
  standalone: true,
  imports: [RouterLink, CardModule, ButtonModule],
  templateUrl: "./dashboard.html",
})
export class Dashboard {}
