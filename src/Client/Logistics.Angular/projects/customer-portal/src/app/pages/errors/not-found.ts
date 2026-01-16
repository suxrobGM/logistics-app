import { Component } from "@angular/core";
import { RouterLink } from "@angular/router";
import { ButtonModule } from "primeng/button";

@Component({
  selector: "cp-not-found",
  templateUrl: "./not-found.html",
  imports: [RouterLink, ButtonModule],
})
export class NotFound {}
