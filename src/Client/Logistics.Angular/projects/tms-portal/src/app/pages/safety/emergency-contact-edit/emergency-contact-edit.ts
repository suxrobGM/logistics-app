import { Component, inject, input, type OnInit, signal } from "@angular/core";
import { Router } from "@angular/router";
import { Api, deleteEmergencyContact, getEmergencyContacts } from "@logistics/shared/api";
import type { EmergencyContactDto } from "@logistics/shared/api";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { PageHeader } from "@/shared/components";
import { ToastService } from "@/core/services";
import { EmergencyContactForm, type EmergencyContactFormValue } from "../components/emergency-contact-form/emergency-contact-form";

@Component({
  selector: "app-emergency-contact-edit",
  templateUrl: "./emergency-contact-edit.html",
  imports: [CardModule, PageHeader, EmergencyContactForm, ProgressSpinnerModule],
})
export class EmergencyContactEditPage implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly id = input.required<string>();

  protected readonly isLoading = signal(true);
  protected readonly contact = signal<EmergencyContactDto | null>(null);
  protected readonly initialValue = signal<Partial<EmergencyContactFormValue> | null>(null);

  async ngOnInit(): Promise<void> {
    await this.loadContact();
  }

  private async loadContact(): Promise<void> {
    this.isLoading.set(true);
    try {
      // Fetch contacts and find by ID (since there's no getById endpoint yet)
      const result = await this.api.invoke(getEmergencyContacts, { PageSize: 1000 });
      const contact = result?.items?.find((c) => c.id === this.id());

      if (contact) {
        this.contact.set(contact);
        this.initialValue.set({
          name: contact.name ?? "",
          contactType: contact.type ?? "safety_manager",
          phoneNumber: contact.phone ?? "",
          email: contact.email,
          priority: contact.priority ?? 1,
          isActive: contact.isActive ?? true,
        });
      } else {
        this.toastService.showError("Emergency contact not found");
        this.router.navigateByUrl("/safety/emergency");
      }
    } finally {
      this.isLoading.set(false);
    }
  }

  protected onSave(contact: EmergencyContactDto): void {
    this.router.navigateByUrl("/safety/emergency");
  }

  protected async onRemove(): Promise<void> {
    try {
      await this.api.invoke(deleteEmergencyContact, { id: this.id() });
      this.toastService.showSuccess("Emergency contact deleted successfully");
      this.router.navigateByUrl("/safety/emergency");
    } catch {
      this.toastService.showError("Failed to delete emergency contact");
    }
  }
}
