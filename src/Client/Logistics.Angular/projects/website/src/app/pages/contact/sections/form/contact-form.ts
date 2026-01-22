import { Component, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { SectionContainer } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";
import { ButtonModule } from "primeng/button";
import { InputTextModule } from "primeng/inputtext";
import { SelectModule } from "primeng/select";
import { TextareaModule } from "primeng/textarea";

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
  ],
})
export class ContactForm {
  protected readonly isLoading = signal(false);
  protected readonly isSubmitted = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly subjectOptions = [
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
      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 1500));
      this.isSubmitted.set(true);
      this.form.reset();
    } catch {
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
