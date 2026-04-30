import { Component, DestroyRef, effect, inject, input, signal } from "@angular/core";
import { DomSanitizer, type SafeResourceUrl } from "@angular/platform-browser";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { TooltipModule } from "primeng/tooltip";

@Component({
  selector: "ui-pdf-viewer",
  templateUrl: "./pdf-viewer.html",
  imports: [CardModule, ButtonModule, TooltipModule],
})
export class PdfViewer {
  private readonly sanitizer = inject(DomSanitizer);
  private readonly destroyRef = inject(DestroyRef);

  /** The PDF file to display */
  public readonly file = input<File | null>(null);

  /** Optional title for the card header */
  public readonly title = input<string>("PDF Preview");

  /** Height of the PDF viewer */
  public readonly height = input<string>("500px");

  /** Whether to show the card wrapper */
  public readonly showCard = input<boolean>(true);

  /** Whether the viewer is collapsible */
  public readonly collapsible = input<boolean>(true);

  protected readonly pdfUrl = signal<SafeResourceUrl | null>(null);
  protected readonly isCollapsed = signal(false);
  private currentBlobUrl: string | null = null;

  constructor() {
    // React to file changes
    effect(() => {
      const file = this.file();
      if (file) {
        this.createPreview(file);
      } else {
        this.revokeUrl();
        this.pdfUrl.set(null);
      }
    });

    // Cleanup on destroy
    this.destroyRef.onDestroy(() => this.revokeUrl());
  }

  protected toggleCollapse(): void {
    this.isCollapsed.update((v) => !v);
  }

  protected openInNewTab(): void {
    if (this.currentBlobUrl) {
      window.open(this.currentBlobUrl, "_blank");
    }
  }

  private createPreview(file: File): void {
    this.revokeUrl();
    this.currentBlobUrl = URL.createObjectURL(file);
    this.pdfUrl.set(this.sanitizer.bypassSecurityTrustResourceUrl(this.currentBlobUrl));
  }

  private revokeUrl(): void {
    if (this.currentBlobUrl) {
      URL.revokeObjectURL(this.currentBlobUrl);
      this.currentBlobUrl = null;
    }
  }
}
