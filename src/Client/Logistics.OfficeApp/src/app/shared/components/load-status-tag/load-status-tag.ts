import {CommonModule} from "@angular/common";
import {Component, input} from "@angular/core";
import {TagModule} from "primeng/tag";
import {LoadStatus, loadStatusOptions} from "@/core/api/models";

@Component({
  selector: "app-load-status-tag",
  templateUrl: "./load-status-tag.html",
  imports: [CommonModule, TagModule],
})
export class LoadStatusTag {
  public readonly status = input.required<LoadStatus>();

  getLoadStatusDesc(): string {
    return loadStatusOptions.find((option) => option.value === this.status())?.label ?? "";
  }

  getLoadStatusSeverity(): "success" | "info" {
    return this.status() === LoadStatus.Delivered ? "success" : "info";
  }

  getLoadStatusIcon(): string {
    return this.status() === LoadStatus.Delivered ? "pi pi-check" : "pi pi-truck";
  }
}
