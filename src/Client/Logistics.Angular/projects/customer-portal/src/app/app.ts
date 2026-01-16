import { Component } from "@angular/core";
import { RouterOutlet } from "@angular/router";

@Component({
  selector: "cp-root",
  templateUrl: "./app.html",
  imports: [RouterOutlet],
})
export class App {}
