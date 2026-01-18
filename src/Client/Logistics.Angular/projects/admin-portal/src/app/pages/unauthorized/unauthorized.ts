import { Component } from "@angular/core";
import { RouterModule } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";

@Component({
  selector: "adm-unauthorized",
  templateUrl: "./unauthorized.html",
  imports: [ButtonModule, CardModule, RouterModule],
})
export class Unauthorized {}
