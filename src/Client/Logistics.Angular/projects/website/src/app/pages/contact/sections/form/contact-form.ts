import { Component, inject, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Api, type ContactSubject, createContactSubmission } from "@logistics/shared/api";
import { LabeledField } from "@logistics/shared/components";
import type { SelectOption } from "@logistics/shared/models";
import { ButtonModule } from "primeng/button";
import { InputTextModule } from "primeng/inputtext";
import { SelectModule } from "primeng/select";
import { TextareaModule } from "primeng/textarea";
import { SectionContainer } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

@Component({
  selector: "web-contact-form",
  templateUrl: "./contact-form.html",
  imports: [
    SectionContainer,
    ScrollAnimateDirective,
    ReactiveFormsModule,
    InputTextModule,
    ButtonModule,
    SelectModule,
    TextareaModule,
    LabeledField,
  ],
})
export class ContactForm {
  private readonly api = inject(Api);

  protected readonly isLoading = signal(false);
  protected readonly isSubmitted = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly subjectOptions: SelectOption<ContactSubject>[] = [
    { label: "General Inquiry", value: "general" },
    { label: "Sales", value: "sales" },
    { label: "Technical Support", value: "support" },
    { label: "Partnership", value: "partnership" },
    { label: "Press & Media", value: "press" },
  ];

  protected readonly form = new FormGroup({
    firstName: new FormControl("", { validators: [Validators.required], nonNullable: true }),
    lastName: new FormControl("", { validators: [Validators.required], nonNullable: true }),
    email: new FormControl("", {
      validators: [Validators.required, Validators.email],
      nonNullable: true,
    }),
    phone: new FormControl("", { nonNullable: true }),
    subject: new FormControl("", { validators: [Validators.required], nonNullable: true }),
    message: new FormControl("", { validators: [Validators.required], nonNullable: true }),
  });

  protected async onSubmit(): Promise<void> {
    if (this.form.invalid || this.isLoading()) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    try {
      const formValue = this.form.getRawValue();

      await this.api.invoke(createContactSubmission, {
        body: {
          firstName: formValue.firstName,
          lastName: formValue.lastName,
          email: formValue.email,
          phone: formValue.phone,
          subject: formValue.subject as ContactSubject,
          message: formValue.message,
        },
      });

      this.isSubmitted.set(true);
      this.form.reset();
    } catch (error) {
      console.error("Error submitting contact form:", error);
      this.errorMessage.set("Failed to send your message. Please try again.");
    } finally {
      this.isLoading.set(false);
    }
  }

  protected resetForm(): void {
    this.isSubmitted.set(false);
    this.errorMessage.set(null);
  }
}
