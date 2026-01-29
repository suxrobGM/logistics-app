import { Component, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import type { EmergencyContactDto, EmergencyContactType } from "@logistics/shared/api";
import type { MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { MenuModule } from "primeng/menu";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { DataContainer, PageHeader, SearchInput } from "@/shared/components";
import type { TagSeverity } from "@/shared/types";
import { EmergencyContactsStore } from "../store";

@Component({
  selector: "app-emergency-contacts",
  templateUrl: "./emergency-contacts.html",
  providers: [EmergencyContactsStore],
  imports: [
    ButtonModule,
    TooltipModule,
    CardModule,
    TableModule,
    MenuModule,
    TagModule,
    DataContainer,
    PageHeader,
    SearchInput,
  ],
})
export class EmergencyContactsPage {
  private readonly router = inject(Router);
  protected readonly store = inject(EmergencyContactsStore);

  protected readonly selectedRow = signal<EmergencyContactDto | null>(null);

  protected readonly actionMenuItems: MenuItem[] = [
    {
      label: "Edit",
      icon: "pi pi-pencil",
      command: () => this.editContact(this.selectedRow()!),
    },
    {
      label: "Delete",
      icon: "pi pi-trash",
      styleClass: "text-danger",
      command: () => this.deleteContact(this.selectedRow()!),
    },
  ];

  protected getContactTypeSeverity(type: EmergencyContactType | undefined): TagSeverity {
    switch (type) {
      case "safety_manager":
        return "info";
      case "dispatcher":
        return "secondary";
      case "emergency_services":
        return "danger";
      case "family_member":
        return "warn";
      default:
        return "secondary";
    }
  }

  protected getContactTypeLabel(type: EmergencyContactType | undefined): string {
    switch (type) {
      case "safety_manager":
        return "Safety Manager";
      case "dispatcher":
        return "Dispatcher";
      case "emergency_services":
        return "Emergency Services";
      case "family_member":
        return "Family Member";
      default:
        return type ?? "Unknown";
    }
  }

  protected onSearch(value: string): void {
    this.store.setSearch(value);
  }

  protected addContact(): void {
    this.router.navigate(["/safety/emergency/add"]);
  }

  protected editContact(contact: EmergencyContactDto): void {
    this.router.navigateByUrl(`/safety/emergency/${contact.id}/edit`);
  }

  protected deleteContact(contact: EmergencyContactDto): void {
    // TODO: Add delete confirmation dialog
    console.log("Delete contact:", contact.id);
  }
}
