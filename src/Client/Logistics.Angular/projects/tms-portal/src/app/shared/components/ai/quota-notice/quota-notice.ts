import { Component, input } from "@angular/core";
import { Icon, Stack } from "@logistics/shared/ui";
import { QuotaNoticeClasses, type QuotaNotice as Notice } from "@/core/store";

/** The usage strip above an agent composer. Renders nothing until the store raises a notice. */
@Component({
  selector: "app-quota-notice",
  templateUrl: "./quota-notice.html",
  imports: [Icon, Stack],
})
export class QuotaNotice {
  public readonly notice = input<Notice | null>(null);

  protected readonly noticeClasses = QuotaNoticeClasses;
}
