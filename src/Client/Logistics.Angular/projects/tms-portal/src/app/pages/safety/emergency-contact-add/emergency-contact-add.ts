import { Component, inject } from "@angular/core";
import { Router } from "@angular/router";
import type { EmergencyContactDto } from "@logistics/shared/api";
import { CardModule } from "primeng/card";
import { PageHeader } from "@/shared/components";
import { EmergencyContactForm } from "../components/emergency-contact-form/emergency-contact-form";

@Component({
  selector: "app-emergency-contact-add",
  templateUrl: "./emergency-contact-add.html",
  imports: [CardModule, PageHeader, EmergencyContactForm],
})
export class EmergencyContactAddPage {
  private readonly router = inject(Router);

  protected onSave(contact: EmergencyContactDto): void {
    this.router.navigateByUrl("/safety/emergency");
  }
}
