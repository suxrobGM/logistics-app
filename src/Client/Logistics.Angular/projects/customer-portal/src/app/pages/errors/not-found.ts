import { Component } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Icon, Stack, Typography } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";

@Component({
  selector: "cp-not-found",
  templateUrl: "./not-found.html",
  imports: [RouterLink, ButtonModule, Icon, Stack, Typography],
})
export class NotFound {}
