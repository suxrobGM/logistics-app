import {Component, input, output} from "@angular/core";

export interface TripFormValue {
  tripName: string;
  startDate: Date;
  endDate: Date;
  description?: string;
  assignedDriverId?: string;
  assignedTruckId?: string;
}

@Component({
  selector: "app-trip-form",
  imports: [],
  templateUrl: "./trip-form.html",
})
export class TripForm {
  public readonly mode = input.required<"create" | "edit">();
  public readonly initial = input<Partial<TripFormValue> | null>(null);
  public readonly isLoading = input(false);

  public readonly save = output<TripFormValue>();
  public readonly remove = output<void>();
}
