import {CommonModule} from "@angular/common";
import {Component, input} from "@angular/core";
import {PayrollDto, SalaryType, salaryTypeOptions} from "@/core/api/models";

@Component({
  selector: "app-payroll-details",
  standalone: true,
  templateUrl: "./payroll-details.component.html",
  imports: [CommonModule],
})
export class PayrollDetailsComponent {
  readonly salaryType = SalaryType;
  readonly payroll = input.required<PayrollDto>();

  getSalaryTypeDesc(enumValue: SalaryType): string {
    return salaryTypeOptions.find((option) => option.value === enumValue)?.label ?? "Unknown";
  }
}
