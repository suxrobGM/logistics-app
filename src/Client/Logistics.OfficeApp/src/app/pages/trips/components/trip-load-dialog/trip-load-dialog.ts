import {Component, computed, input, model, output} from "@angular/core";
//import {AccordionModule} from "primeng/accordion";
import {DialogModule} from "primeng/dialog";
import {LoadDto, LoadType, TripLoadDto} from "@/core/api/models";
import {LoadFormComponent, LoadFormValue} from "@/shared/components";

@Component({
  selector: "app-trip-load-dialog",
  templateUrl: "./trip-load-dialog.html",
  imports: [DialogModule, LoadFormComponent], //SearchLoadComponent, AccordionModule],
})
export class TripLoadDialog {
  public readonly visible = model(false);
  public readonly assignedTruckId = input.required<string | undefined>();

  /**
   * When user creates a new load, this event is emitted with the created load data.
   */
  public readonly created = output<LoadFormValue>();

  /**
   * When user picks an existing load, this event is emitted with the selected load data.
   */
  public readonly picked = output<TripLoadDto>();

  protected readonly initialLoad = computed(
    () =>
      ({
        assignedTruckId: this.assignedTruckId(),
        type: LoadType.Vehicle,
      }) satisfies Partial<LoadFormValue>
  );

  protected pickLoad(load: LoadDto | null) {
    if (!load) {
      return;
    }

    this.picked.emit(load as TripLoadDto);
    this.visible.set(false);
  }

  protected createLoad(formValue: LoadFormValue): void {
    this.created.emit(formValue);
  }
}
