import { Component } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Icon } from "@logistics/shared/ui";
import { GITHUB_REPO_URL } from "@/shared/constants";

@Component({
  selector: "web-footer",
  templateUrl: "./footer.html",
  imports: [Icon, RouterLink],
})
export class Footer {
  protected readonly currentYear = new Date().getFullYear();
  protected readonly githubUrl = GITHUB_REPO_URL;
}
