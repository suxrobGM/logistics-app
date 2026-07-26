import { DatePipe } from "@angular/common";
import { Component, computed, inject, signal, type OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import {
  Api,
  connectQuickBooks,
  disconnectQuickBooks,
  getQuickBooksConnection,
  type AccountingConnectionDto,
} from "@logistics/shared/api";
import { Badge, Card, Spinner, Stack, UiButton } from "@logistics/shared/ui";
import { ToastService } from "@/core/services";

/** QuickBooks Online connection. Rendered by the Integrations settings tab. */
@Component({
  selector: "app-quickbooks-card",
  templateUrl: "./quickbooks-card.html",
  imports: [Badge, Card, DatePipe, Spinner, Stack, UiButton],
})
export class QuickbooksCard implements OnInit {
  private readonly api = inject(Api);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  protected readonly loading = signal(true);
  protected readonly connecting = signal(false);
  protected readonly connection = signal<AccountingConnectionDto | null>(null);

  protected readonly isConnected = computed(() => this.connection()?.isConnected ?? false);

  ngOnInit(): void {
    const status = this.route.snapshot.queryParamMap.get("status");
    if (status === "success") {
      this.toast.showSuccess("QuickBooks connected successfully.");
    } else if (status === "error") {
      this.toast.showError("Could not connect to QuickBooks. Please try again.");
    }
    this.load();
  }

  protected async load(): Promise<void> {
    this.loading.set(true);
    try {
      const data = await this.api.invoke(getQuickBooksConnection);
      this.connection.set(data ?? null);
    } catch {
      this.toast.showError("Failed to load QuickBooks connection.");
    } finally {
      this.loading.set(false);
    }
  }

  protected async connect(): Promise<void> {
    this.connecting.set(true);
    try {
      const result = await this.api.invoke(connectQuickBooks);
      if (result?.url) {
        // OAuth flow leaves the app; the callback redirects back to this page with ?status=.
        window.location.href = result.url;
      } else {
        this.connecting.set(false);
      }
    } catch {
      this.toast.showError("Could not start the QuickBooks connection.");
      this.connecting.set(false);
    }
  }

  protected disconnect(): void {
    this.toast.confirm({
      header: "Disconnect QuickBooks",
      message:
        "Stop syncing to QuickBooks? Existing records already pushed to QuickBooks are kept.",
      icon: "warning",
      severity: "danger",
      accept: async () => {
        try {
          await this.api.invoke(disconnectQuickBooks);
          this.toast.showSuccess("QuickBooks disconnected.");
          await this.load();
        } catch {
          this.toast.showError("Failed to disconnect QuickBooks.");
        }
      },
    });
  }
}
