import { Component, effect, inject, input, model, output, signal } from "@angular/core";
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { Api, createTimeEntry, getEmployees, updateTimeEntry } from "@logistics/shared/api";
import { timeEntryTypeOptions } from "@logistics/shared/api/enums";
import type {
  CreateTimeEntryCommand,
  EmployeeDto,
  TimeEntryDto,
  TimeEntryType,
  UpdateTimeEntryCommand,
} from "@logistics/shared/api/models";
import { ButtonModule } from "primeng/button";
import { DatePickerModule } from "primeng/datepicker";
import { DialogModule } from "primeng/dialog";
import { InputNumberModule } from "primeng/inputnumber";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { TextareaModule } from "primeng/textarea";
import { ToastService } from "@/core/services";
import { LabeledField } from "@/shared/components";

@Component({
  selector: "app-time-entry-form-dialog",
  templateUrl: "./time-entry-form-dialog.html",
  imports: [
    DialogModule,
    ProgressSpinnerModule,
    FormsModule,
    ReactiveFormsModule,
    ButtonModule,
    SelectModule,
    DatePickerModule,
    InputNumberModule,
    TextareaModule,
    LabeledField,
  ],
})
export class TimeEntryFormDialog {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly visible = model<boolean>(false);
  public readonly timeEntry = input<TimeEntryDto | null>(null);
  public readonly preselectedEmployeeId = input<string | null>(null);
  public readonly saved = output<void>();

  protected readonly employees = signal<EmployeeDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly typeOptions = timeEntryTypeOptions;
  protected readonly timeError = signal<string | null>(null);

  protected readonly form = new FormGroup({
    employeeId: new FormControl<string>("", Validators.required),
    date: new FormControl<Date | null>(null, Validators.required),
    startTime: new FormControl<Date | null>(null, Validators.required),
    endTime: new FormControl<Date | null>(null, Validators.required),
    totalHours: new FormControl<number | null>({ value: null, disabled: true }),
    type: new FormControl<TimeEntryType>("regular", Validators.required),
    notes: new FormControl<string>(""),
  });

  protected get isEditMode(): boolean {
    return !!this.timeEntry()?.id;
  }

  protected get dialogTitle(): string {
    return this.isEditMode ? "Edit Time Entry" : "Add Time Entry";
  }

  constructor() {
    this.fetchEmployees();

    // Watch for visible changes to re-populate form when dialog opens
    effect(
      () => {
        const isVisible = this.visible();
        if (isVisible) {
          const entry = this.timeEntry();
          if (entry) {
            this.populateForm(entry);
          }
        }
      },
      { allowSignalWrites: true },
    );

    // Watch for preselectedEmployeeId changes
    effect(
      () => {
        const employeeId = this.preselectedEmployeeId();
        if (employeeId && !this.timeEntry()) {
          this.form.patchValue({ employeeId });
        }
      },
      { allowSignalWrites: true },
    );
  }

  async submit(): Promise<void> {
    // Validate time
    if (this.timeError()) {
      this.toastService.showError(this.timeError()!);
      return;
    }

    if (this.form.invalid) {
      this.toastService.showError("Please fill in all required fields");
      return;
    }

    const formValue = this.form.getRawValue(); // getRawValue includes disabled controls
    this.isLoading.set(true);

    try {
      if (this.isEditMode) {
        const command: UpdateTimeEntryCommand = {
          id: this.timeEntry()!.id!,
          date: formValue.date ? this.formatDate(formValue.date) : undefined,
          startTime: formValue.startTime ? this.formatTime(formValue.startTime) : null,
          endTime: formValue.endTime ? this.formatTime(formValue.endTime) : null,
          totalHours: formValue.totalHours ?? undefined,
          type: formValue.type ?? undefined,
          notes: formValue.notes ?? null,
        };

        await this.api.invoke(updateTimeEntry, { id: this.timeEntry()!.id!, body: command });
        this.toastService.showSuccess("Time entry updated successfully");
      } else {
        const command: CreateTimeEntryCommand = {
          employeeId: formValue.employeeId!,
          date: formValue.date ? this.formatDate(formValue.date) : undefined,
          startTime: formValue.startTime ? this.formatTime(formValue.startTime) : undefined,
          endTime: formValue.endTime ? this.formatTime(formValue.endTime) : undefined,
          totalHours: formValue.totalHours ?? undefined,
          type: formValue.type ?? undefined,
          notes: formValue.notes ?? null,
        };

        await this.api.invoke(createTimeEntry, { body: command });
        this.toastService.showSuccess("Time entry created successfully");
      }

      this.saved.emit();
      this.close();
    } catch {
      this.toastService.showError(
        this.isEditMode ? "Failed to update time entry" : "Failed to create time entry",
      );
    } finally {
      this.isLoading.set(false);
    }
  }

  close(): void {
    this.visible.set(false);
    this.resetForm();
    this.timeError.set(null);
  }

  private populateForm(entry: TimeEntryDto): void {
    this.form.patchValue({
      employeeId: entry.employeeId ?? "",
      date: entry.date ? new Date(entry.date) : null,
      startTime: entry.startTime ? this.parseTime(entry.startTime) : null,
      endTime: entry.endTime ? this.parseTime(entry.endTime) : null,
      type: (entry.type as TimeEntryType) ?? "regular",
      notes: entry.notes ?? "",
    });
    // Set disabled control value
    this.form.controls.totalHours.setValue(entry.totalHours ?? null);
    this.timeError.set(null);
  }

  private resetForm(): void {
    this.form.reset({
      employeeId: this.preselectedEmployeeId() ?? "",
      date: null,
      startTime: null,
      endTime: null,
      type: "regular",
      notes: "",
    });
    this.form.controls.totalHours.setValue(null);
    this.timeError.set(null);
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
    const startTime = this.form.value.startTime;
    const endTime = this.form.value.endTime;

    if (startTime && endTime) {
      const diffMs = endTime.getTime() - startTime.getTime();

      // Validate start time is before end time
      if (diffMs <= 0) {
        this.timeError.set("End time must be after start time");
        this.form.controls.totalHours.setValue(null);
        return;
      }

      this.timeError.set(null);
      const diffHours = Math.round((diffMs / (1000 * 60 * 60)) * 100) / 100;
      this.form.controls.totalHours.setValue(diffHours);
    } else {
      this.form.controls.totalHours.setValue(null);
    }
  }
}
