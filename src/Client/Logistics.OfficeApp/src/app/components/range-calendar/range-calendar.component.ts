import {CommonModule} from "@angular/common";
import {Component, Input, OnChanges, output} from "@angular/core";
import {FormsModule} from "@angular/forms";
import {ButtonModule} from "primeng/button";
import {CalendarModule} from "primeng/calendar";
import {DateUtils} from "@/core/utilities";

@Component({
  selector: "app-range-calendar",
  standalone: true,
  templateUrl: "./range-calendar.component.html",
  styleUrls: [],
  imports: [CommonModule, CalendarModule, FormsModule, ButtonModule],
})
export class RangeCalendarComponent implements OnChanges {
  public dates: Date[];
  public todayDate: Date;

  @Input() label: string;
  @Input() startDate?: Date;
  @Input() endDate?: Date;
  public readonly startDateChange = output<Date>();
  public readonly endDateChange = output<Date>();
  public readonly applyButtonClick = output<void>();

  constructor() {
    this.label = "Select range:";
    this.todayDate = DateUtils.today();
    this.dates = [];
  }

  ngOnChanges(): void {
    if (this.startDate && this.endDate) {
      this.dates = [this.startDate, this.endDate];
    }
  }

  areDatesValid(): boolean {
    return DateUtils.isValidRange(this.dates);
  }

  onClickButton() {
    this.startDate = this.dates[0];
    this.endDate = this.dates[1];

    this.startDateChange.emit(this.startDate);
    this.endDateChange.emit(this.endDate);
    this.applyButtonClick.emit();
  }
}
