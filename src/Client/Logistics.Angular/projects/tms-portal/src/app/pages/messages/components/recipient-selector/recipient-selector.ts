import { ChangeDetectionStrategy, Component, inject, input, output, signal } from "@angular/core";
import { Api, type EmployeeDto, getEmployees } from "@logistics/shared/api";
import {
  type AutoCompleteCompleteEvent,
  AutoCompleteModule,
  type AutoCompleteSelectEvent,
} from "primeng/autocomplete";
import { AvatarModule } from "primeng/avatar";
import { ButtonModule } from "primeng/button";
import { TooltipModule } from "primeng/tooltip";
import { Converters } from "@/shared/utils";

@Component({
  selector: "app-recipient-selector",
  templateUrl: "./recipient-selector.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [AvatarModule, AutoCompleteModule, ButtonModule, TooltipModule],
})
export class RecipientSelector {
  private readonly api = inject(Api);

  readonly currentUserId = input<string | null>(null);
  readonly selected = input<EmployeeDto | null>(null);
  readonly selectedChange = output<EmployeeDto | null>();

  protected readonly employeeSuggestions = signal<EmployeeDto[]>([]);

  protected async searchEmployees(event: AutoCompleteCompleteEvent): Promise<void> {
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

  protected onRecipientSelect(event: AutoCompleteSelectEvent): void {
    this.selectedChange.emit(event.value as EmployeeDto);
  }

  protected clearRecipient(): void {
    this.selectedChange.emit(null);
  }

  protected getInitials(name?: string | null): string {
    return Converters.getInitials(name);
  }
}
