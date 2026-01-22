import { Component } from "@angular/core";
import { AboutHero, Mission, Story, Team, Values } from "./sections";

@Component({
  selector: "web-about",
  templateUrl: "./about.html",
  imports: [AboutHero, Story, Mission, Team, Values],
})
export class About {}
