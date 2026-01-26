import { Component, inject, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { ToastService } from "@logistics/shared";
import { Api, createBlogPost } from "@logistics/shared/api";
import type { CreateBlogPostCommand } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { BlogPostForm, type BlogPostFormValue } from "@/shared/components";

@Component({
  selector: "adm-blog-post-add",
  templateUrl: "./blog-post-add.html",
  imports: [CardModule, ButtonModule, RouterModule, BlogPostForm, DividerModule],
})
export class BlogPostAdd {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly isLoading = signal<boolean>(false);

  protected async onSave(formValue: BlogPostFormValue): Promise<void> {
    this.isLoading.set(true);

    const command: CreateBlogPostCommand = {
      title: formValue.title,
      content: formValue.content,
      excerpt: formValue.excerpt || undefined,
      category: formValue.category || undefined,
      authorName: formValue.authorName,
      featuredImage: formValue.featuredImage || undefined,
      isFeatured: formValue.isFeatured,
    };

    await this.api.invoke(createBlogPost, { body: command });
    this.toastService.showSuccess("Blog post has been created successfully");
    this.router.navigateByUrl("/blog-posts");

    this.isLoading.set(false);
  }
}
