import { Location } from "@angular/common";
import { Component, inject } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Icon, UiButton } from "@logistics/shared/ui";

@Component({
  selector: "app-not-found",
  templateUrl: "./not-found.html",
  imports: [Icon, RouterLink, UiButton],
})
export class NotFoundComponent {
  private readonly location = inject(Location);

  protected goBack(): void {
    this.location.back();
  }
}
