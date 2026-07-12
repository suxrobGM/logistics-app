import { Component, effect, inject, input, model, output, signal } from "@angular/core";
import {
  disabled,
  form,
  FormField,
  FormRoot,
  max,
  min,
  required,
  validate,
} from "@angular/forms/signals";
import {
  Api,
  createTimeEntry,
  getEmployees,
  updateTimeEntry,
  type CreateTimeEntryCommand,
  type EmployeeDto,
  type TimeEntryDto,
  type TimeEntryType,
  type UpdateTimeEntryCommand,
} from "@logistics/shared/api";
import { timeEntryTypeOptions } from "@logistics/shared/api/enums";
import {
  Grid,
  Spinner,
  Stack,
  UiButton,
  UiDateField,
  UiDialog,
  UiNumberField,
  UiSelectField,
  UiTextareaField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { UiFormField } from "@/shared/components";

// `totalHours` is calculated and disabled; the other nullable fields mirror the DTO shape.
const EMPTY = {
  employeeId: "",
  date: null as Date | null,
  startTime: null as Date | null,
  endTime: null as Date | null,
  totalHours: null as number | null,
  type: "regular" as TimeEntryType,
  notes: "",
};

@Component({
  selector: "app-timesheet-form-dialog",
  templateUrl: "./form-dialog.html",
  imports: [
    FormField,
    FormRoot,
    Grid,
    Spinner,
    Stack,
    UiButton,
    UiDateField,
    UiDialog,
    UiFormField,
    UiNumberField,
    UiSelectField,
    UiTextareaField,
    ValidatedForm,
  ],
})
export class TimesheetFormDialog {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly visible = model<boolean>(false);
  public readonly timeEntry = input<TimeEntryDto | null>(null);
  public readonly preselectedEmployeeId = input<string | null>(null);
  public readonly saved = output<void>();

  protected readonly employees = signal<EmployeeDto[]>([]);
  protected readonly typeOptions = timeEntryTypeOptions;

  protected readonly model = signal({ ...EMPTY });

  /**
   * The start/end ordering check is a cross-field `validate()` rule, so an out-of-order time makes
   * the form invalid and blocks submission.
   */
  protected readonly form = form(
    this.model,
    (p) => {
      required(p.employeeId, { message: "Employee is required." });
      required(p.date, { message: "Date is required." });
      required(p.startTime, { message: "Start time is required." });
      required(p.endTime, { message: "End time is required." });
      required(p.type, { message: "Type is required." });

      // Calculated, read-only field. Its 0–24 bound moves from the template [min]/[max]
      // (reserved Signal Forms state inputs on ui-number-field) into schema validators.
      disabled(p.totalHours, { when: () => true });
      min(p.totalHours, 0, { message: "Total hours cannot be negative." });
      max(p.totalHours, 24, { message: "Total hours cannot exceed 24." });

      validate(p.endTime, ({ valueOf }) => {
        const start = valueOf(p.startTime);
        const end = valueOf(p.endTime);
        return start && end && end.getTime() <= start.getTime()
          ? { kind: "timeOrder", message: "End time must be after start time." }
          : null;
      });
    },
    {
      submission: {
        action: async () => {
          const formValue = this.model();

          try {
            if (this.isEditMode) {
              const command: UpdateTimeEntryCommand = {
                id: this.timeEntry()!.id!,
                date: formValue.date ? this.formatDate(formValue.date) : undefined,
                startTime: formValue.startTime ? this.formatTime(formValue.startTime) : null,
                endTime: formValue.endTime ? this.formatTime(formValue.endTime) : null,
                totalHours: formValue.totalHours ?? undefined,
                type: formValue.type,
                notes: formValue.notes,
              };

              await this.api.invoke(updateTimeEntry, { id: this.timeEntry()!.id!, body: command });
              this.toastService.showSuccess("Timesheet entry updated successfully");
            } else {
              const command: CreateTimeEntryCommand = {
                employeeId: formValue.employeeId,
                date: formValue.date ? this.formatDate(formValue.date) : undefined,
                startTime: formValue.startTime ? this.formatTime(formValue.startTime) : undefined,
                endTime: formValue.endTime ? this.formatTime(formValue.endTime) : undefined,
                totalHours: formValue.totalHours ?? undefined,
                type: formValue.type,
                notes: formValue.notes,
              };

              await this.api.invoke(createTimeEntry, { body: command });
              this.toastService.showSuccess("Timesheet entry created successfully");
            }

            this.saved.emit();
            this.close();
          } catch {
            this.toastService.showError(
              this.isEditMode
                ? "Failed to update timesheet entry"
                : "Failed to create timesheet entry",
            );
          }
          return undefined;
        },
        onInvalid: () => {
          this.toastService.showError("Please fill in all required fields");
        },
      },
    },
  );

  protected get isEditMode(): boolean {
    return !!this.timeEntry()?.id;
  }

  protected get dialogTitle(): string {
    return this.isEditMode ? "Edit Timesheet Entry" : "Add Timesheet Entry";
  }

  constructor() {
    this.fetchEmployees();

    // Re-populate the form when the dialog opens for an existing entry.
    effect(() => {
      if (this.visible()) {
        const entry = this.timeEntry();
        if (entry) {
          this.populateForm(entry);
        }
      }
    });

    // Seed the employee when one is preselected and we are creating a new entry.
    effect(() => {
      const employeeId = this.preselectedEmployeeId();
      if (employeeId && !this.timeEntry()) {
        this.model.update((v) => ({ ...v, employeeId }));
      }
    });
  }

  close(): void {
    this.visible.set(false);
    this.resetForm();
  }

  private populateForm(entry: TimeEntryDto): void {
    this.model.set({
      employeeId: entry.employeeId ?? "",
      date: entry.date ? new Date(entry.date) : null,
      startTime: entry.startTime ? this.parseTime(entry.startTime) : null,
      endTime: entry.endTime ? this.parseTime(entry.endTime) : null,
      totalHours: entry.totalHours ?? null,
      type: (entry.type as TimeEntryType) ?? "regular",
      notes: entry.notes ?? "",
    });
  }

  private resetForm(): void {
    this.form().reset({ ...EMPTY, employeeId: this.preselectedEmployeeId() ?? "" });
  }

  private async fetchEmployees(): Promise<void> {
    try {
      // Fetch employees with Hourly salary type
      const result = await this.api.invoke(getEmployees, { PageSize: 100 });
      if (result.items) {
        // Filter for hourly employees if needed, but for now load all
        this.employees.set(result.items);
      }
    } catch {
      this.toastService.showError("Failed to load employees");
    }
  }

  private formatDate(date: Date): string {
    return date.toISOString().split("T")[0];
  }

  private formatTime(date: Date): string {
    const hours = date.getHours().toString().padStart(2, "0");
    const minutes = date.getMinutes().toString().padStart(2, "0");
    return `${hours}:${minutes}:00`;
  }

  private parseTime(timeStr: string): Date {
    const [hours, minutes] = timeStr.split(":").map(Number);
    const date = new Date();
    date.setHours(hours, minutes, 0, 0);
    return date;
  }

  protected calculateHours(): void {
    const { startTime, endTime } = this.model();

    if (startTime && endTime) {
      const diffMs = endTime.getTime() - startTime.getTime();

      // Out-of-order times clear the total; the cross-field validate() rule surfaces the error.
      if (diffMs <= 0) {
        this.model.update((v) => ({ ...v, totalHours: null }));
        return;
      }

      const diffHours = Math.round((diffMs / (1000 * 60 * 60)) * 100) / 100;
      this.model.update((v) => ({ ...v, totalHours: diffHours }));
    } else {
      this.model.update((v) => ({ ...v, totalHours: null }));
    }
  }
}
