import {Component, EventEmitter, Input, OnChanges, Output} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {CalendarModule} from 'primeng/calendar';
import {ButtonModule} from 'primeng/button';
import {DateUtils} from '@core/utils';


@Component({
  selector: 'app-range-calendar',
  standalone: true,
  templateUrl: './range-calendar.component.html',
  styleUrls: [],
  imports: [
    CommonModule,
    CalendarModule,
    FormsModule,
    ButtonModule,
  ],
})
export class RangeCalendarComponent implements OnChanges {
  public dates: Date[];
  public todayDate: Date;

  @Input() label: string;
  @Input() startDate?: Date;
  @Input() endDate?: Date;
  @Output() startDateChange = new EventEmitter<Date>();
  @Output() endDateChange = new EventEmitter<Date>();
  @Output() applyButtonClick = new EventEmitter<void>();

  constructor() {
    this.label = 'Select range:';
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
