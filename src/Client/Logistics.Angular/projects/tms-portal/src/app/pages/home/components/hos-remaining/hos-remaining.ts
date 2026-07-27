import { Component, computed, inject, signal, type OnInit } from "@angular/core";
import { Api, getAllDriversHos, type DriverHosStatusDto } from "@logistics/shared/api";
import {
  Card,
  Divider,
  EmptyState,
  Icon,
  Skeleton,
  Stack,
  Typography,
  type IconName,
} from "@logistics/shared/ui";
import { AuthService } from "@/core/auth";

interface HosClock {
  label: string;
  icon: IconName;
  display: string;
  minutes: number;
}

/** Below this many minutes left the clock is worth reacting to before booking the next load. */
const LOW_MINUTES = 60;

@Component({
  selector: "app-hos-remaining",
  templateUrl: "./hos-remaining.html",
  imports: [Card, Divider, EmptyState, Icon, Skeleton, Stack, Typography],
})
export class HosRemaining implements OnInit {
  private readonly api = inject(Api);
  private readonly authService = inject(AuthService);

  protected readonly isLoading = signal(true);
  private readonly drivers = signal<DriverHosStatusDto[]>([]);

  /**
   * `Employee.CreateEmployeeFromUser` assigns `Employee.Id = user.Id`, so the OIDC `sub` is the
   * employee id the ELD endpoint keys on. A solo tenant with a single mapped driver still resolves
   * when the signed-in owner is not that driver themselves.
   */
  protected readonly status = computed<DriverHosStatusDto | null>(() => {
    const all = this.drivers();
    const userId = this.authService.getUserData()?.id;
    return all.find((d) => d.employeeId === userId) ?? (all.length === 1 ? all[0] : null);
  });

  protected readonly clocks = computed<HosClock[]>(() => {
    const hos = this.status();
    if (!hos) return [];
    return [
      {
        label: "Driving",
        icon: "gauge",
        display: hos.drivingTimeRemainingDisplay ?? "--",
        minutes: hos.drivingMinutesRemaining ?? 0,
      },
      {
        label: "On duty",
        icon: "clock",
        display: hos.onDutyTimeRemainingDisplay ?? "--",
        minutes: hos.onDutyMinutesRemaining ?? 0,
      },
      {
        label: "Cycle",
        icon: "history",
        display: hos.cycleTimeRemainingDisplay ?? "--",
        minutes: hos.cycleMinutesRemaining ?? 0,
      },
    ];
  });

  async ngOnInit(): Promise<void> {
    this.isLoading.set(true);
    const data = await this.api.invoke(getAllDriversHos);
    this.drivers.set(data ?? []);
    this.isLoading.set(false);
  }

  protected isLow(minutes: number): boolean {
    return minutes <= LOW_MINUTES;
  }
}
