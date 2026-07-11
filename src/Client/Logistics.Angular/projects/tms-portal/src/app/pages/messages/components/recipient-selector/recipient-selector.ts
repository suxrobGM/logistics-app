import { Component, inject, input, output, signal } from "@angular/core";
import { Api, getEmployees, type EmployeeDto } from "@logistics/shared/api";
import {
  Avatar,
  Icon,
  Stack,
  Typography,
  UiAutocompleteField,
  UiButton,
  UiTooltip,
  type UiAutocompleteCompleteEvent,
} from "@logistics/shared/ui";
import { UserAvatar } from "@/shared/components";
import { Converters } from "@/shared/utils";

@Component({
  selector: "app-recipient-selector",
  templateUrl: "./recipient-selector.html",
  imports: [Avatar, Icon, Stack, Typography, UiAutocompleteField, UiButton, UiTooltip, UserAvatar],
})
export class RecipientSelector {
  private readonly api = inject(Api);

  readonly currentUserId = input<string | null>(null);
  readonly selected = input<EmployeeDto | null>(null);
  readonly selectedChange = output<EmployeeDto | null>();

  protected readonly employeeSuggestions = signal<EmployeeDto[]>([]);

  protected async searchEmployees(event: UiAutocompleteCompleteEvent): Promise<void> {
    const query = event.query;
    if (!query || query.length < 2) {
      this.employeeSuggestions.set([]);
      return;
    }

    try {
      const result = await this.api.invoke(getEmployees, {
        Search: query,
        PageSize: 10,
      });

      const filtered = (result.items ?? []).filter(
        (e: EmployeeDto) => e.id !== this.currentUserId(),
      );
      this.employeeSuggestions.set(filtered);
    } catch {
      this.employeeSuggestions.set([]);
    }
  }

  protected onRecipientSelect(employee: EmployeeDto | null): void {
    this.selectedChange.emit(employee);
  }

  protected clearRecipient(): void {
    this.selectedChange.emit(null);
  }

  protected getInitials(name?: string | null): string {
    return Converters.getInitials(name);
  }
}
