import { Component } from "@angular/core";

@Component({
  selector: "web-footer",
  templateUrl: "./footer.html",
})
export class Footer {
  protected readonly currentYear = new Date().getFullYear();
}
