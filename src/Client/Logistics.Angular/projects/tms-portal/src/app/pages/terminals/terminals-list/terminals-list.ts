import { Component, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import { Permission } from "@logistics/shared";
import type { TerminalDto, TerminalType } from "@logistics/shared/api";
import { terminalTypeOptions } from "@logistics/shared/api/enums";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { SelectModule } from "primeng/select";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { DataContainer, PageHeader, SearchInput } from "@/shared/components";
import { TerminalsListStore } from "../store";

@Component({
  selector: "app-terminals-list",
  templateUrl: "./terminals-list.html",
  providers: [TerminalsListStore],
  imports: [
    ButtonModule,
    CardModule,
    TableModule,
    TagModule,
    SelectModule,
    TooltipModule,
    DataContainer,
    PageHeader,
    SearchInput,
  ],
})
export class TerminalsList {
  private readonly router = inject(Router);
  protected readonly store = inject(TerminalsListStore);
  protected readonly Permission = Permission;
  protected readonly typeOptions = terminalTypeOptions;

  protected readonly typeFilter = signal<TerminalType | null>(null);
  protected readonly countryFilter = signal<string | null>(null);

  protected search(value: string): void {
    this.store.setSearch(value);
  }

  protected onTypeChange(type: TerminalType | null): void {
    this.typeFilter.set(type);
    this.applyFilters();
  }

  protected onCountryChange(value: string): void {
    const code = value?.trim().toUpperCase() || null;
    this.countryFilter.set(code);
    this.applyFilters();
  }

  protected addTerminal(): void {
    this.router.navigate(["/terminals/add"]);
  }

  protected editTerminal(terminal: TerminalDto): void {
    if (terminal.id) {
      this.router.navigate(["/terminals", terminal.id]);
    }
  }

  protected typeLabel(type?: TerminalType): string {
    return terminalTypeOptions.find((opt) => opt.value === type)?.label ?? "";
  }

  private applyFilters(): void {
    const filters: Record<string, unknown> = {};
    const type = this.typeFilter();
    const country = this.countryFilter();
    if (type) filters["Type"] = type;
    if (country) filters["CountryCode"] = country;
    this.store.setFilters(filters);
  }
}
