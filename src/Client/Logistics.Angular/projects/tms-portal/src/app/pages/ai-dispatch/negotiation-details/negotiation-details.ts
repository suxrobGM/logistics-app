import { Component, computed, DestroyRef, inject, input, signal, type OnInit } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { getApiErrorMessage, PageHeader, Permission } from "@logistics/shared";
import {
  Api,
  closeNegotiation,
  getNegotiationById,
  type RateNegotiationDto,
} from "@logistics/shared/api";
import { CurrencyFormatPipe, DateFormatPipe } from "@logistics/shared/pipes";
import {
  Alert,
  Card,
  Icon,
  Spinner,
  Stack,
  StatusBadge,
  Typography,
  UiButton,
} from "@logistics/shared/ui";
import { PermissionService } from "@/core/auth";
import { AIDispatchHubService, ToastService } from "@/core/services";
import { isOpenNegotiation } from "@/shared/utils";

@Component({
  selector: "app-negotiation-details",
  templateUrl: "./negotiation-details.html",
  imports: [
    Alert,
    Card,
    CurrencyFormatPipe,
    DateFormatPipe,
    Icon,
    PageHeader,
    Spinner,
    Stack,
    StatusBadge,
    Typography,
    UiButton,
  ],
})
export class NegotiationDetails implements OnInit {
  public readonly id = input.required<string>();

  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly permissionService = inject(PermissionService);
  private readonly hub = inject(AIDispatchHubService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly canManage = computed(() =>
    this.permissionService.hasPermission(Permission.Negotiation.Manage),
  );

  protected readonly isLoading = signal(false);
  protected readonly isClosing = signal(false);
  protected readonly negotiation = signal<RateNegotiationDto | null>(null);

  protected readonly isOpen = computed(() => isOpenNegotiation(this.negotiation()?.status));

  ngOnInit(): void {
    this.load();
    void this.hub.acquireDispatchBoard(this.destroyRef);

    // The broadcast carries no messages, so a status change refreshes the timeline from the API.
    this.hub.negotiationReceived$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((updated) => {
      if (updated.id === this.id()) {
        void this.load();
      }
    });
  }

  protected confirmClose(): void {
    this.toastService.confirm({
      message:
        "Closing ends the thread and disables its reply address, so anything the broker sends " +
        "afterwards is dropped. Continue?",
      header: "Close negotiation",
      icon: "warning",
      severity: "danger",
      accept: () => this.close(),
    });
  }

  private async close(): Promise<void> {
    this.isClosing.set(true);
    try {
      await this.api.invoke(closeNegotiation, {
        id: this.id(),
        body: { reason: "Closed by the dispatcher" },
      });
      this.toastService.showSuccess("Negotiation closed");
      await this.load();
    } catch (error: unknown) {
      this.toastService.showError(getApiErrorMessage(error, "Failed to close the negotiation"));
    } finally {
      this.isClosing.set(false);
    }
  }

  private async load(): Promise<void> {
    this.isLoading.set(true);
    try {
      this.negotiation.set(await this.api.invoke(getNegotiationById, { id: this.id() }));
    } catch {
      this.toastService.showError("Failed to load the negotiation");
    } finally {
      this.isLoading.set(false);
    }
  }
}
