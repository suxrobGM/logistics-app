import { DatePipe } from "@angular/common";
import { Component, computed, inject, input, signal, type OnInit } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import {
  Api,
  deleteContainer,
  getContainerById,
  type ContainerDto,
  type ContainerIsoType,
  type ContainerStatus,
} from "@logistics/shared/api";
import { containerIsoTypeOptions, containerStatusOptions } from "@logistics/shared/api/enums";
import {
  Card,
  Grid,
  Icon,
  Spinner,
  Stack,
  StatusBadge,
  Surface,
  Typography,
  UiButton,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { PageHeader } from "@/shared/components";

@Component({
  selector: "app-container-details",
  templateUrl: "./container-details.html",
  imports: [
    Card,
    DatePipe,
    Grid,
    Icon,
    PageHeader,
    RouterLink,
    Spinner,
    Stack,
    StatusBadge,
    Surface,
    Typography,
    UiButton,
  ],
})
export class ContainerDetails implements OnInit {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  protected readonly id = input.required<string>();

  protected readonly isLoading = signal(false);
  protected readonly isDeleting = signal(false);
  protected readonly container = signal<ContainerDto | null>(null);

  protected readonly title = computed(() => {
    const c = this.container();
    return c?.number ? `Container ${c.number}` : "Container";
  });

  ngOnInit(): void {
    this.fetchContainer();
  }

  protected isoTypeLabel(isoType?: ContainerIsoType): string {
    return containerIsoTypeOptions.find((o) => o.value === isoType)?.label ?? "";
  }

  protected statusLabel(status?: ContainerStatus): string {
    return containerStatusOptions.find((o) => o.value === status)?.label ?? "";
  }

  protected onDelete(): void {
    this.toast.confirm({
      header: "Delete Container",
      message: "Are you sure you want to delete this container? This action cannot be undone.",
      icon: "warning",
      acceptLabel: "Delete",
      rejectLabel: "Cancel",
      severity: "danger",
      accept: async () => {
        this.isDeleting.set(true);
        try {
          await this.api.invoke(deleteContainer, { id: this.id() });
          this.toast.showSuccess("Container has been deleted successfully");
          this.router.navigateByUrl("/containers");
        } catch (error: unknown) {
          const message =
            (error as { error?: { error?: string }; message?: string })?.error?.error ??
            (error as { message?: string })?.message ??
            "Failed to delete container";
          this.toast.showError(message);
        } finally {
          this.isDeleting.set(false);
        }
      },
    });
  }

  private async fetchContainer(): Promise<void> {
    this.isLoading.set(true);
    try {
      const container = await this.api.invoke(getContainerById, { id: this.id() });
      if (!container) return;
      this.container.set(container);
    } finally {
      this.isLoading.set(false);
    }
  }
}
