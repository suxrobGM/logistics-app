import { Component } from "@angular/core";
import { RouterOutlet } from "@angular/router";
import { Navbar } from "../navbar/navbar";

@Component({
  selector: "cp-main-layout",
  templateUrl: "./main-layout.html",
  imports: [RouterOutlet, Navbar],
})
export class MainLayout {}
