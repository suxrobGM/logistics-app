import { Location } from "@angular/common";
import { Component, inject } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Icon } from "@logistics/shared/ui";
import { ButtonModule } from "primeng/button";

@Component({
  selector: "app-not-found",
  templateUrl: "./not-found.html",
  imports: [ButtonModule, Icon, RouterLink],
})
export class NotFoundComponent {
  private readonly location = inject(Location);

  protected goBack(): void {
    this.location.back();
  }
}
