import { Component, inject, input, linkedSignal, output } from "@angular/core";
import { form, FormField, FormRoot, required } from "@angular/forms/signals";
import { RouterLink } from "@angular/router";
import { ToastService } from "@logistics/shared";
import {
  UiButton,
  UiCheckboxField,
  UiEditor,
  UiFormField,
  UiTextareaField,
  UiTextField,
  ValidatedForm,
} from "@logistics/shared/ui";

export interface BlogPostFormValue {
  title: string;
  content: string;
  excerpt: string;
  category: string;
  authorName: string;
  featuredImage: string;
  isFeatured: boolean;
}

const EMPTY: BlogPostFormValue = {
  title: "",
  content: "",
  excerpt: "",
  category: "",
  authorName: "",
  featuredImage: "",
  isFeatured: false,
};

@Component({
  selector: "adm-blog-post-form",
  templateUrl: "./blog-post-form.html",
  imports: [
    FormField,
    FormRoot,
    RouterLink,
    UiButton,
    UiCheckboxField,
    UiEditor,
    UiFormField,
    UiTextareaField,
    UiTextField,
    ValidatedForm,
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

  /** Seeded from `initial()`; resets to those values whenever the input changes. */
  protected readonly model = linkedSignal<BlogPostFormValue>(() => ({
    ...EMPTY,
    ...(this.initial() ?? {}),
  }));

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.title, { message: "Title is required." });
      required(p.content, { message: "Content is required." });
      required(p.authorName, { message: "Author name is required." });
    },
    {
      submission: {
        action: async () => {
          this.save.emit(this.model());
          return undefined;
        },
      },
    },
  );

  protected askRemove(): void {
    this.toastService.confirm({
      message: "Are you sure that you want to delete this blog post? This action cannot be undone.",
      header: "Confirm Delete",
      icon: "warning",
      severity: "danger",
      accept: () => this.remove.emit(),
    });
  }

  protected askPublish(): void {
    this.toastService.confirm({
      message: "Are you sure you want to publish this blog post? It will be visible to the public.",
      header: "Confirm Publish",
      icon: "send",
      accept: () => this.publish.emit(),
    });
  }

  protected askUnpublish(): void {
    this.toastService.confirm({
      message:
        "Are you sure you want to unpublish this blog post? It will no longer be visible to the public.",
      header: "Confirm Unpublish",
      icon: "hide",
      severity: "warning",
      accept: () => this.unpublish.emit(),
    });
  }
}
