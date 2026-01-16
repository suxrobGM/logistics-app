import { Component } from "@angular/core";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { InputTextModule } from "primeng/inputtext";

@Component({
  selector: "cp-account-settings",
  standalone: true,
  imports: [CardModule, ButtonModule, InputTextModule],
  templateUrl: "./account-settings.html",
})
export class AccountSettings {}
