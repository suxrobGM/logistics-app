import { Component } from "@angular/core";
import { ContactForm, ContactHero, ContactInfo } from "./sections";

@Component({
  selector: "web-contact",
  templateUrl: "./contact.html",
  imports: [ContactHero, ContactForm, ContactInfo],
})
export class Contact {}
