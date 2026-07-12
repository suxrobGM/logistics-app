import {
  booleanAttribute,
  Component,
  ElementRef,
  input,
  numberAttribute,
  output,
  signal,
  viewChild,
} from "@angular/core";
import { UiButton } from "../../action/button";
import { Icon } from "../../icons/icon/icon";

/**
 * File picker, used at both of its call sites, in both of its modes:
 *
 *   - `basic`    a single choose button              (expenses / expense-receipt-upload)
 *   - `advanced` a drag-and-drop zone with projected content, a choose button, and a file list
 *                (loads / load-import)
 *
 * =================================================================================================
 * THIS COMPONENT DOES NOT UPLOAD ANYTHING. THAT IS THE POINT, AND IT FIXES A LIVE BUG.
 * =================================================================================================
 * Both call sites set `[auto]="true"` but the old file-upload widget was given no `url` and no
 * `customUpload`. So on every file selection it ran:
 *
 *     this.uploading = true;
 *     this.http.request(this.method, this.url, { body: formData, ... })   // this.url is UNDEFINED
 *
 * — firing a spurious `POST` to `undefined` (i.e. `/undefined`, a 404) and latching the widget into a
 * permanent "uploading" state, while the REAL upload happened in the app's own `(onSelect)` handler.
 * Nothing bound `(onUpload)` or `(onError)`, so the failure was swallowed and invisible.
 *
 * The app already owns the upload — `load-import` posts the PDF itself, `expense-receipt-upload`
 * calls `uploadExpenseReceipt`. So this component's whole job is to hand over `File[]` and get out of
 * the way. `auto` disappears with the phantom request.
 *
 * The `accept` / `maxFileSize` guards are enforced HERE (not just advertised to the file dialog), so
 * a drag-and-dropped file gets the same checks as a picked one — `accept` on `<input type="file">`
 * filters the browser's dialog but does nothing for a drop.
 */
@Component({
  selector: "ui-file-upload",
  templateUrl: "./file-upload.html",
  imports: [UiButton, Icon],
})
export class UiFileUpload {
  public readonly mode = input<"basic" | "advanced">("basic");

  /** Same syntax as the native `accept` attribute, e.g. `.pdf` or `image/*,.pdf`. */
  public readonly accept = input<string>("");
  /** Bytes. 0 means no limit. */
  public readonly maxFileSize = input(0, { transform: numberAttribute });
  public readonly multiple = input(false, { transform: booleanAttribute });
  public readonly disabled = input(false, { transform: booleanAttribute });
  public readonly chooseLabel = input<string>("Choose");

  /** The accepted files. Never emitted empty. */
  public readonly filesSelect = output<File[]>();
  /** A file the guards rejected, with the reason — so the page can surface it. */
  public readonly rejected = output<string>();

  private readonly fileInput = viewChild.required<ElementRef<HTMLInputElement>>("fileInput");

  protected readonly dragging = signal(false);
  protected readonly files = signal<File[]>([]);

  protected choose(): void {
    if (this.disabled()) {
      return;
    }
    this.fileInput().nativeElement.click();
  }

  protected onInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.handle(Array.from(input.files ?? []));
    // Reset, so picking the SAME file twice in a row still fires a change event.
    input.value = "";
  }

  protected onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragging.set(false);
    if (this.disabled()) {
      return;
    }
    this.handle(Array.from(event.dataTransfer?.files ?? []));
  }

  protected onDragOver(event: DragEvent): void {
    event.preventDefault();
    if (!this.disabled()) {
      this.dragging.set(true);
    }
  }

  protected onDragLeave(): void {
    this.dragging.set(false);
  }

  protected remove(file: File): void {
    this.files.update((files) => files.filter((f) => f !== file));
  }

  private handle(incoming: File[]): void {
    if (incoming.length === 0) {
      return;
    }

    const files = this.multiple() ? incoming : incoming.slice(0, 1);
    const accepted: File[] = [];

    for (const file of files) {
      const rejection = this.reject(file);
      if (rejection) {
        this.rejected.emit(rejection);
        continue;
      }
      accepted.push(file);
    }

    if (accepted.length === 0) {
      return;
    }

    this.files.set(this.multiple() ? [...this.files(), ...accepted] : accepted);
    this.filesSelect.emit(accepted);
  }

  /** Returns a human-readable reason, or null when the file is fine. */
  private reject(file: File): string | null {
    const max = this.maxFileSize();
    if (max > 0 && file.size > max) {
      return `${file.name} is larger than the ${this.megabytes(max)} limit.`;
    }
    if (!this.matchesAccept(file)) {
      return `${file.name} is not an accepted file type.`;
    }
    return null;
  }

  /**
   * Mirror the browser's `accept` matching: a comma-separated list of extensions (`.pdf`), exact MIME
   * types (`application/pdf`) and wildcard MIME types (`image/*`).
   */
  private matchesAccept(file: File): boolean {
    const accept = this.accept().trim();
    if (!accept) {
      return true;
    }

    return accept
      .split(",")
      .map((token) => token.trim().toLowerCase())
      .filter(Boolean)
      .some((token) => {
        if (token.startsWith(".")) {
          return file.name.toLowerCase().endsWith(token);
        }
        if (token.endsWith("/*")) {
          return file.type.toLowerCase().startsWith(token.slice(0, -1));
        }
        return file.type.toLowerCase() === token;
      });
  }

  private megabytes(bytes: number): string {
    return `${Math.round(bytes / (1024 * 1024))} MB`;
  }
}
