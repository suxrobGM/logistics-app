import {Injectable} from "@angular/core";
import {MessageService} from "primeng/api";

@Injectable({providedIn: "root"})
export class ToastService {
  constructor(private messageService: MessageService) {}

  showSuccess(message: string, title = "Notification") {
    this.messageService.add({key: "notification", severity: "success", summary: title, detail: message});
  }

  showError(message: string) {
    this.messageService.add({key: "notification", severity: "error", summary: "Error", detail: message});
  }
}
