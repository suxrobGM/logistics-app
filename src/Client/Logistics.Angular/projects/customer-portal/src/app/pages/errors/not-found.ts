import { Component } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Icon, Stack, Typography, UiButton } from "@logistics/shared/ui";

@Component({
  selector: "cp-not-found",
  templateUrl: "./not-found.html",
  imports: [Icon, RouterLink, Stack, Typography, UiButton],
})
export class NotFound {}
