import { Component } from "@angular/core";
import { RouterLink } from "@angular/router";

@Component({
  selector: "web-footer",
  templateUrl: "./footer.html",
  imports: [RouterLink],
})
export class Footer {
  protected readonly currentYear = new Date().getFullYear();
}
