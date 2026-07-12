import { DatePipe } from "@angular/common";
import { Component, computed, input } from "@angular/core";
import type { LoadStatus } from "@logistics/shared/api";
import { Icon, type IconName } from "@logistics/shared/ui";

interface StepConfig {
  status: LoadStatus;
  label: string;
  icon: IconName;
}

@Component({
  selector: "app-load-status-stepper",
  templateUrl: "./load-status-stepper.html",
  imports: [DatePipe, Icon],
})
export class LoadStatusStepper {
  public readonly currentStatus = input.required<LoadStatus>();
  public readonly createdAt = input<string | null | undefined>(null);
  public readonly dispatchedAt = input<string | null | undefined>(null);
  public readonly pickedUpAt = input<string | null | undefined>(null);
  public readonly deliveredAt = input<string | null | undefined>(null);
  public readonly cancelledAt = input<string | null | undefined>(null);

  protected readonly steps: StepConfig[] = [
    { status: "draft", label: "Draft", icon: "file-pen" },
    { status: "dispatched", label: "Dispatched", icon: "send" },
    { status: "picked_up", label: "Picked Up", icon: "truck" },
    { status: "delivered", label: "Delivered", icon: "circle-check" },
  ];

  protected readonly isCancelled = computed(() => this.currentStatus() === "cancelled");

  protected readonly currentStepIndex = computed(() => {
    const status = this.currentStatus();
    if (status === "cancelled") {
      return -1;
    }
    return this.steps.findIndex((s) => s.status === status);
  });

  protected isStepCompleted(index: number): boolean {
    if (this.isCancelled()) {
      return false;
    }
    return index < this.currentStepIndex();
  }

  protected isStepCurrent(index: number): boolean {
    if (this.isCancelled()) {
      return false;
    }
    return index === this.currentStepIndex();
  }

  protected isStepPending(index: number): boolean {
    if (this.isCancelled()) {
      return true;
    }
    return index > this.currentStepIndex();
  }

  protected getStepDate(status: LoadStatus): string | null | undefined {
    switch (status) {
      case "draft":
        return this.createdAt();
      case "dispatched":
        return this.dispatchedAt();
      case "picked_up":
        return this.pickedUpAt();
      case "delivered":
        return this.deliveredAt();
      default:
        return null;
    }
  }
}
