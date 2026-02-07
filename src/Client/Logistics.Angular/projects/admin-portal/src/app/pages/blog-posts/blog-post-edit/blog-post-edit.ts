import { Component, type OnInit, computed, inject, signal } from "@angular/core";
import { ActivatedRoute, Router, RouterModule } from "@angular/router";
import { ToastService } from "@logistics/shared";
import {
  Api,
  deleteBlogPost,
  getBlogPostById,
  publishBlogPost,
  unpublishBlogPost,
  updateBlogPost,
} from "@logistics/shared/api";
import type { BlogPostDto, UpdateBlogPostCommand } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { SkeletonModule } from "primeng/skeleton";
import { BlogPostForm, type BlogPostFormValue } from "@/shared/components";

@Component({
  selector: "adm-blog-post-edit",
  templateUrl: "./blog-post-edit.html",
  imports: [CardModule, ButtonModule, RouterModule, BlogPostForm, DividerModule, SkeletonModule],
})
export class BlogPostEdit implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly isLoading = signal<boolean>(false);
  protected readonly isFetching = signal<boolean>(true);
  protected readonly blogPost = signal<BlogPostDto | null>(null);

  ngOnInit(): void {
    this.fetchBlogPost();
  }

  private async fetchBlogPost(): Promise<void> {
    const id = this.route.snapshot.paramMap.get("id");
    if (!id) {
      this.router.navigateByUrl("/blog-posts");
      return;
    }

    this.isFetching.set(true);
    const post = await this.api.invoke(getBlogPostById, { id });

    if (!post) {
      this.toastService.showError("Blog post not found");
      this.router.navigateByUrl("/blog-posts");
      return;
    }

    this.blogPost.set(post);
    this.isFetching.set(false);
  }

  protected readonly initialValue = computed<Partial<BlogPostFormValue> | null>(() => {
    const post = this.blogPost();
    if (!post) return null;

    return {
      title: post.title ?? "",
      content: post.content ?? "",
      excerpt: post.excerpt ?? "",
      category: post.category ?? "",
      authorName: post.authorName ?? "",
      featuredImage: post.featuredImage ?? "",
      isFeatured: post.isFeatured ?? false,
    };
  });

  protected async onSave(formValue: BlogPostFormValue): Promise<void> {
    const post = this.blogPost();
    if (!post) return;

    this.isLoading.set(true);

    const command: UpdateBlogPostCommand = {
      id: post.id!,
      title: formValue.title,
      content: formValue.content,
      excerpt: formValue.excerpt || undefined,
      category: formValue.category || undefined,
      authorName: formValue.authorName,
      featuredImage: formValue.featuredImage || undefined,
      isFeatured: formValue.isFeatured,
    };

    await this.api.invoke(updateBlogPost, { id: post.id!, body: command });
    this.toastService.showSuccess("Blog post has been updated successfully");
    this.router.navigateByUrl("/blog-posts");

    this.isLoading.set(false);
  }

  protected async onRemove(): Promise<void> {
    const post = this.blogPost();
    if (!post) return;

    await this.api.invoke(deleteBlogPost, { id: post.id! });
    this.toastService.showSuccess("Blog post has been deleted successfully");
    this.router.navigateByUrl("/blog-posts");
  }

  protected async onPublish(): Promise<void> {
    const post = this.blogPost();
    if (!post) return;

    await this.api.invoke(publishBlogPost, { id: post.id! });
    this.toastService.showSuccess("Blog post has been published successfully");
    this.fetchBlogPost();
  }

  protected async onUnpublish(): Promise<void> {
    const post = this.blogPost();
    if (!post) return;

    await this.api.invoke(unpublishBlogPost, { id: post.id! });
    this.toastService.showSuccess("Blog post has been unpublished successfully");
    this.fetchBlogPost();
  }
}
