import {CommonModule} from "@angular/common";
import {Component, model, signal} from "@angular/core";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {DialogModule} from "primeng/dialog";

@Component({
  selector: "app-payment-method-dialog",
  templateUrl: "./payment-method-dialog.component.html",
  imports: [CommonModule, CardModule, ButtonModule, DialogModule],
})
export class PaymentMethodDialogComponent {
  readonly showDialog = model(false);
  readonly isLoading = signal(false);
}
