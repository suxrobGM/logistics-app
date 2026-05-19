import { Component } from "@angular/core";
import { SectionContainer } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

@Component({
  selector: "web-account-deletion",
  templateUrl: "./account-deletion.html",
  imports: [SectionContainer, ScrollAnimateDirective],
})
export class AccountDeletion {
  protected readonly lastUpdated = "May 19, 2026";
  protected readonly developerName = "LogisticsX";
  protected readonly appName = "LogisticsX Driver";
  protected readonly contactEmail = "privacy@logisticsx.app";
  protected readonly gracePeriodDays = 30;

  protected readonly sections = [
    { id: "overview", title: "Overview" },
    { id: "in-app-steps", title: "Delete from the Driver App" },
    { id: "email-request", title: "Request Deletion by Email" },
    { id: "what-is-deleted", title: "What Gets Deleted" },
    { id: "what-is-retained", title: "What We Must Retain" },
    { id: "timeline", title: "Timeline & Cancellation" },
    { id: "partial-deletion", title: "Deleting Specific Data" },
    { id: "contact", title: "Contact Us" },
  ];

  protected readonly deletedData = [
    "Driver profile (name, email, phone, photo, license details)",
    "Account credentials and authentication tokens",
    "Saved app preferences and notification settings",
    "In-app messages and chat history",
    "Personal documents you uploaded (medical card, CDL scan)",
  ];

  protected readonly retainedData = [
    {
      label: "Hours of Service (HOS) / ELD logs",
      reason:
        "FMCSA 49 CFR §395.22(i) requires motor carriers to retain driver duty status records for 6 months.",
    },
    {
      label: "Trip, load, and delivery records",
      reason:
        "Required for tax, insurance, and DOT audit purposes. Retained for up to 7 years per IRS and DOT recordkeeping rules.",
    },
    {
      label: "Payroll, settlements, and tax forms",
      reason:
        "Retained for at least 4 years to comply with IRS, state labor, and 1099/W-2 reporting requirements.",
    },
    {
      label: "Safety and incident records",
      reason:
        "Accident reports and drug/alcohol test records must be kept per FMCSA Part 382 and 390.",
    },
  ];
}
