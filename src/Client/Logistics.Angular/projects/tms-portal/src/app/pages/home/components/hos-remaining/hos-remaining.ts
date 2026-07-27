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
import { EldRulesService } from "@/core/services";

interface HosClock {
  label: string;
  icon: IconName;
  display: string;
  minutes: number;
  /** Below this, the clock is worth reacting to before booking the next load. */
  warnMinutes: number;
}

@Component({
  selector: "app-hos-remaining",
  templateUrl: "./hos-remaining.html",
  imports: [Card, Divider, EmptyState, Icon, Skeleton, Stack, Typography],
})
export class HosRemaining implements OnInit {
  private readonly api = inject(Api);
  private readonly authService = inject(AuthService);
  private readonly rules = inject(EldRulesService);

  protected readonly isLoading = signal(true);
  private readonly drivers = signal<DriverHosStatusDto[]>([]);

  /**
   * `Employee.CreateEmployeeFromUser` assigns `Employee.Id = user.Id`, so the OIDC `sub` is the
   * employee id the ELD endpoint keys on.
   */
  protected readonly status = computed<DriverHosStatusDto | null>(() => {
    const all = this.drivers();
    const userId = this.authService.getUserData()?.id;
    return all.find((d) => d.employeeId === userId) ?? (all.length === 1 ? all[0] : null);
  });

  protected readonly clocks = computed<HosClock[]>(() => {
    const hos = this.status();
    if (!hos) return [];

    const onDutyWarn = this.rules.onDutyWarnMinutes();
    return [
      {
        label: "Driving",
        icon: "gauge",
        display: hos.drivingTimeRemainingDisplay ?? "--",
        minutes: hos.drivingMinutesRemaining ?? 0,
        warnMinutes: this.rules.drivingWarnMinutes(),
      },
      {
        label: "On duty",
        icon: "clock",
        display: hos.onDutyTimeRemainingDisplay ?? "--",
        minutes: hos.onDutyMinutesRemaining ?? 0,
        warnMinutes: onDutyWarn,
      },
      {
        label: "Cycle",
        icon: "history",
        display: hos.cycleTimeRemainingDisplay ?? "--",
        minutes: hos.cycleMinutesRemaining ?? 0,
        warnMinutes: onDutyWarn,
      },
    ];
  });

  async ngOnInit(): Promise<void> {
    this.isLoading.set(true);
    void this.rules.load();
    const data = await this.api.invoke(getAllDriversHos);
    this.drivers.set(data ?? []);
    this.isLoading.set(false);
  }

  protected isLow(clock: HosClock): boolean {
    return clock.minutes <= clock.warnMinutes;
  }
}
