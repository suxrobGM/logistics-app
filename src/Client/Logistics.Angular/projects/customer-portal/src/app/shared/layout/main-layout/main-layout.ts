import { Component } from "@angular/core";
import { RouterOutlet } from "@angular/router";
import { LicenseBanner } from "@logistics/shared/ui";
import { Navbar } from "../navbar/navbar";

@Component({
  selector: "cp-main-layout",
  templateUrl: "./main-layout.html",
  imports: [LicenseBanner, RouterOutlet, Navbar],
})
export class MainLayout {}
