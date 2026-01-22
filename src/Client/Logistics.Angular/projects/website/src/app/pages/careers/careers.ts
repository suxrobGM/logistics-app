import { Component } from "@angular/core";
import { Benefits, CareersCta, CareersHero, Culture, Positions } from "./sections";

@Component({
  selector: "web-careers",
  templateUrl: "./careers.html",
  imports: [CareersHero, Culture, Benefits, Positions, CareersCta],
})
export class Careers {}
