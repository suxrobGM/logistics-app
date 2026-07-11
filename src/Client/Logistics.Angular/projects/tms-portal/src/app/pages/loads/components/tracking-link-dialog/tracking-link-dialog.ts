import { DatePipe } from "@angular/common";
import { Component, inject, input, model, signal } from "@angular/core";
import {
  Api,
  createTrackingLink,
  getTrackingLinksForLoad,
  revokeTrackingLink,
  type TrackingLinkDto,
} from "@logistics/shared/api";
import {
  Badge,
  Icon,
  Spinner,
  UiButton,
  UiDataTable,
  UiDialog,
  UiTooltip,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { SendTrackingLinkDialog } from "../send-tracking-link-dialog/send-tracking-link-dialog";

@Component({
  selector: "app-tracking-link-dialog",
  templateUrl: "./tracking-link-dialog.html",
  imports: [
    Badge,
    DatePipe,
    Icon,
    SendTrackingLinkDialog,
    Spinner,
    UiButton,
    UiDataTable,
    UiDialog,
    UiTooltip,
  ],
})
export class TrackingLinkDialog {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly loadId = input.required<string>();
  public readonly visible = model<boolean>(false);

  protected readonly trackingLinks = signal<TrackingLinkDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly isGenerating = signal(false);
  protected readonly showEmailDialog = signal(false);
  protected readonly selectedLinkId = signal<string | null>(null);

  onShow(): void {
    this.fetchTrackingLinks();
  }

  async generateLink(): Promise<void> {
    this.isGenerating.set(true);
    try {
      const result = await this.api.invoke(createTrackingLink, {
        body: { loadId: this.loadId() },
      });
      this.trackingLinks.update((links) => [result, ...links]);
      this.toastService.showSuccess("Tracking link generated");
    } catch {
      this.toastService.showError("Failed to generate tracking link");
    } finally {
      this.isGenerating.set(false);
    }
  }

  async copyToClipboard(url: string): Promise<void> {
    try {
      await navigator.clipboard.writeText(url);
      this.toastService.showSuccess("Tracking link copied to clipboard");
    } catch {
      this.toastService.showError("Failed to copy link");
    }
  }

  async revokeLink(id: string): Promise<void> {
    try {
      await this.api.invoke(revokeTrackingLink, { id });
      this.trackingLinks.update((links) =>
        links.map((link) => (link.id === id ? { ...link, isActive: false } : link)),
      );
      this.toastService.showSuccess("Tracking link revoked");
    } catch {
      this.toastService.showError("Failed to revoke tracking link");
    }
  }

  openEmailDialog(link: TrackingLinkDto): void {
    this.selectedLinkId.set(link.id!);
    this.showEmailDialog.set(true);
  }

  close(): void {
    this.visible.set(false);
  }

  private async fetchTrackingLinks(): Promise<void> {
    this.isLoading.set(true);
    try {
      const result = await this.api.invoke(getTrackingLinksForLoad, {
        loadId: this.loadId(),
      });
      this.trackingLinks.set(result ?? []);
    } catch {
      this.toastService.showError("Failed to load tracking links");
    } finally {
      this.isLoading.set(false);
    }
  }

  getLinkStatus(link: TrackingLinkDto): "success" | "warn" | "danger" {
    if (!link.isActive) return "danger";
    if (link.isExpired) return "warn";
    return "success";
  }

  getLinkStatusLabel(link: TrackingLinkDto): string {
    if (!link.isActive) return "Revoked";
    if (link.isExpired) return "Expired";
    return "Active";
  }
}
