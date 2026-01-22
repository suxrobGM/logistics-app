import { Injectable, signal } from "@angular/core";

@Injectable({
  providedIn: "root",
})
export class DemoDialogService {
  public readonly visible = signal(false);

  public open(): void {
    this.visible.set(true);
  }

  public close(): void {
    this.visible.set(false);
  }
}
