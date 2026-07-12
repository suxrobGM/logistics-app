import { Component, effect, inject, input, output, signal } from "@angular/core";
import { applyWhen, email, form, FormField, FormRoot, required } from "@angular/forms/signals";
import { RouterLink } from "@angular/router";
import { ToastService } from "@logistics/shared";
import type { Address, Region } from "@logistics/shared/api";
import { regionOptions } from "@logistics/shared/api/enums";
import {
  AddressForm,
  UiButton,
  UiFormField,
  UiSelectField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/ui";

export interface TenantFormValue {
  name: string;
  companyName: string;
  billingEmail: string;
  dotNumber: string;
  companyAddress: Address;
  ownerEmail: string;
  ownerFirstName: string;
  ownerLastName: string;
  region: Region;
}

/** Editable model shape — `companyAddress` is null until the address editor is fully filled. */
interface TenantFormModel {
  name: string;
  companyName: string;
  billingEmail: string;
  dotNumber: string;
  companyAddress: Address | null;
  ownerEmail: string;
  ownerFirstName: string;
  ownerLastName: string;
  region: Region;
}

const EMPTY: TenantFormModel = {
  name: "",
  companyName: "",
  billingEmail: "",
  dotNumber: "",
  companyAddress: null,
  ownerEmail: "",
  ownerFirstName: "",
  ownerLastName: "",
  region: "us",
};

@Component({
  selector: "adm-tenant-form",
  templateUrl: "./tenant-form.html",
  imports: [
    AddressForm,
    FormField,
    FormRoot,
    RouterLink,
    UiButton,
    UiFormField,
    UiSelectField,
    UiTextField,
    ValidatedForm,
  ],
})
export class TenantForm {
  private readonly toastService = inject(ToastService);
  protected readonly regionOptions = regionOptions;

  public readonly mode = input.required<"create" | "edit">();
  public readonly initial = input<Partial<TenantFormValue> | null>(null);
  public readonly isLoading = input(false);

  public readonly save = output<TenantFormValue>();
  public readonly remove = output<void>();

  protected readonly model = signal<TenantFormModel>({ ...EMPTY });

  /**
   * Owner fields are required only in create mode, via declarative `applyWhen`. The action just
   * re-raises `save`; the parent owns the API call and the button's loading state (`isLoading`).
   */
  protected readonly form = form(
    this.model,
    (p) => {
      required(p.name, { message: "Tenant name is required." });
      required(p.companyName, { message: "Company name is required." });
      required(p.billingEmail, { message: "Billing email is required." });
      email(p.billingEmail, { message: "Enter a valid email address." });
      required(p.region, { message: "Region is required." });
      required(p.companyAddress, { message: "Company address is required." });

      applyWhen(
        p,
        () => this.mode() === "create",
        (cp) => {
          required(cp.ownerFirstName, { message: "First name is required." });
          required(cp.ownerLastName, { message: "Last name is required." });
          required(cp.ownerEmail, { message: "Owner email is required." });
          email(cp.ownerEmail, { message: "Enter a valid email address." });
        },
      );
    },
    {
      submission: {
        action: async () => {
          this.save.emit(this.model() as TenantFormValue);
          return undefined;
        },
      },
    },
  );

  constructor() {
    effect(() => {
      const initial = this.initial();
      if (initial) {
        this.model.update((v) => ({ ...v, ...initial }));
      }
    });
  }

  protected askRemove(): void {
    this.toastService.confirm({
      message: "Are you sure that you want to delete this tenant? This action cannot be undone.",
      header: "Confirm Delete",
      icon: "warning",
      severity: "danger",
      accept: () => this.remove.emit(),
    });
  }
}
