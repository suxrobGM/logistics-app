import { Component, computed, input } from "@angular/core";
import { AbstractControl } from "@angular/forms";

@Component({
  selector: "shared-labeled-field",
  templateUrl: "./labeled-field.html",
})
export class LabeledField {
  public readonly label = input<string | null>(null);
  public readonly for = input<string | null>(null);
  public readonly required = input(false);
  public readonly hint = input<string | null>(null);
  public readonly control = input<AbstractControl | null>(null);
  protected readonly isFieldInvalid = computed(
    () => this.control()?.invalid && (this.control()?.touched || this.control()?.dirty),
  );
}
