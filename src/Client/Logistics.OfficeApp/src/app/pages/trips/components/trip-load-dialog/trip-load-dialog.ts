import {Component, computed, input, model, output} from "@angular/core";
import {AccordionModule} from "primeng/accordion";
import {DialogModule} from "primeng/dialog";
import {CreateTripLoadCommand, LoadDto} from "@/core/api/models";
import {LoadFormComponent, LoadFormValue, SearchLoadComponent} from "@/shared/components";

@Component({
  selector: "app-trip-load-dialog",
  templateUrl: "./trip-load-dialog.html",
  imports: [DialogModule, LoadFormComponent, SearchLoadComponent, AccordionModule],
})
export class TripLoadDialog {
  public readonly visible = model(false);
  public readonly assignedTruckId = input.required<string | undefined>();

  /**
   * When user creates a new load, this event is emitted with the created load data.
   */
  public readonly created = output<CreateTripLoadCommand>();

  /**
   * When user picks an existing load, this event is emitted with the selected load data.
   */
  public readonly picked = output<LoadDto>();

  protected readonly initialLoad = computed(() => ({
    assignedTruckId: this.assignedTruckId(),
  }));

  protected onPickExisting(load: LoadDto | null) {
    if (!load) {
      return;
    }

    this.picked.emit(load);
    this.visible.set(false);
  }

  protected createLoad(formValue: LoadFormValue): void {
    this.created.emit({
      name: formValue.name,
      originAddress: formValue.originAddress!,
      originLocation: formValue.originLocation,
      destinationAddress: formValue.destinationAddress!,
      destinationLocation: formValue.destinationLocation,
      deliveryCost: formValue.deliveryCost!,
      distance: formValue.distance,
      assignedDispatcherId: formValue.assignedDispatcherId!,
      customerId: formValue.customer!.id,
      type: formValue.loadType,
    });
  }
}
