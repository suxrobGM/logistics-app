import { Component, effect, inject, input, output } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { RouterLink } from "@angular/router";
import { LabeledField, ValidationSummary } from "@logistics/shared/components";
import { ToastService } from "@logistics/shared";
import { ButtonModule } from "primeng/button";
import { InputTextModule } from "primeng/inputtext";
import { TextareaModule } from "primeng/textarea";
import { EditorModule } from "primeng/editor";
import { CheckboxModule } from "primeng/checkbox";
import { ProgressSpinnerModule } from "primeng/progressspinner";

export interface BlogPostFormValue {
  title: string;
  content: string;
  excerpt: string;
  category: string;
  authorName: string;
  featuredImage: string;
  isFeatured: boolean;
}

@Component({
  selector: "adm-blog-post-form",
  templateUrl: "./blog-post-form.html",
  imports: [
    ButtonModule,
    ValidationSummary,
    ReactiveFormsModule,
    RouterLink,
    ProgressSpinnerModule,
    LabeledField,
    InputTextModule,
    TextareaModule,
    EditorModule,
    CheckboxModule,
  ],
})
export class BlogPostForm {
  private readonly toastService = inject(ToastService);

  public readonly mode = input.required<"create" | "edit">();
  public readonly initial = input<Partial<BlogPostFormValue> | null>(null);
  public readonly isLoading = input(false);
  public readonly status = input<string | null | undefined>(null);

  public readonly save = output<BlogPostFormValue>();
  public readonly remove = output<void>();
  public readonly publish = output<void>();
  public readonly unpublish = output<void>();

  protected readonly form = new FormGroup({
    title: new FormControl("", { validators: Validators.required, nonNullable: true }),
    content: new FormControl("", { validators: Validators.required, nonNullable: true }),
    excerpt: new FormControl("", { nonNullable: true }),
    category: new FormControl("", { nonNullable: true }),
    authorName: new FormControl("", { validators: Validators.required, nonNullable: true }),
    featuredImage: new FormControl("", { nonNullable: true }),
    isFeatured: new FormControl(false, { nonNullable: true }),
  });

  constructor() {
    effect(() => {
      if (this.initial()) {
        this.patch(this.initial()!);
      }
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      return;
    }
    this.save.emit(this.form.getRawValue() as BlogPostFormValue);
  }

  protected askRemove(): void {
    this.toastService.confirm({
      message: "Are you sure that you want to delete this blog post? This action cannot be undone.",
      header: "Confirm Delete",
      icon: "pi pi-exclamation-triangle",
      acceptButtonStyleClass: "p-button-danger",
      accept: () => this.remove.emit(),
    });
  }

  protected askPublish(): void {
    this.toastService.confirm({
      message: "Are you sure you want to publish this blog post? It will be visible to the public.",
      header: "Confirm Publish",
      icon: "pi pi-send",
      accept: () => this.publish.emit(),
    });
  }

  protected askUnpublish(): void {
    this.toastService.confirm({
      message: "Are you sure you want to unpublish this blog post? It will no longer be visible to the public.",
      header: "Confirm Unpublish",
      icon: "pi pi-eye-slash",
      acceptButtonStyleClass: "p-button-warning",
      accept: () => this.unpublish.emit(),
    });
  }

  private patch(src: Partial<BlogPostFormValue>): void {
    this.form.patchValue({
      ...src,
    });
  }
}
