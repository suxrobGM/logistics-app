import {CommonModule} from "@angular/common";
import {Component, effect, input, model, output, signal} from "@angular/core";
import {FormsModule} from "@angular/forms";
import {ButtonModule} from "primeng/button";
import {DatePicker} from "primeng/datepicker";
import {DateUtils} from "@/shared/utils";

@Component({
  selector: "app-range-calendar",
  templateUrl: "./range-calendar.html",
  imports: [CommonModule, DatePicker, FormsModule, ButtonModule],
})
export class RangeCalendar {
  protected readonly todayDate = DateUtils.today();
  protected readonly dates = signal<Date[]>([]);

  public readonly label = input("Select range:");
  public readonly startDate = model<Date | null>(null);
  public readonly endDate = model<Date | null>(null);
  public readonly startDateChange = output<Date | null>();
  public readonly endDateChange = output<Date | null>();
  public readonly applyButtonClick = output<void>();

  constructor() {
    effect(() => {
      if (this.startDate() && this.endDate()) {
        this.dates.set([this.startDate()!, this.endDate()!]);
      }
    });
  }

  areDatesValid(): boolean {
    return DateUtils.isValidRange(this.dates());
  }

  onClickButton(): void {
    this.startDate.set(this.dates()[0]);
    this.endDate.set(this.dates()[1]);

    this.startDateChange.emit(this.startDate());
    this.endDateChange.emit(this.endDate());
    this.applyButtonClick.emit();
  }
}
