import { DatePipe } from "@angular/common";
import { Component, computed, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import { Api, deleteBlogPost, publishBlogPost, unpublishBlogPost } from "@logistics/shared/api";
import type { BlogPostDto } from "@logistics/shared/api";
import { DataContainer, PageHeader, SearchInput } from "@logistics/shared/components";
import type { MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { MenuModule } from "primeng/menu";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { BlogPostsListStore } from "../store/blog-posts-list.store";

@Component({
  selector: "adm-blog-posts-list",
  templateUrl: "./blog-posts-list.html",
  providers: [BlogPostsListStore],
  imports: [
    ButtonModule,
    TooltipModule,
    CardModule,
    TableModule,
    MenuModule,
    DataContainer,
    PageHeader,
    SearchInput,
    TagModule,
    DatePipe,
  ],
})
export class BlogPostsList {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(BlogPostsListStore);

  protected readonly selectedPost = signal<BlogPostDto | null>(null);

  protected readonly actionMenuItems = computed<MenuItem[]>(() => {
    const post = this.selectedPost();
    if (!post) return [];

    const isDraft = post.status?.toLowerCase() === "draft";
    const isArchived = post.status?.toLowerCase() === "archived";
    const isPublished = post.status?.toLowerCase() === "published";

    const items: MenuItem[] = [
      {
        label: "Edit",
        icon: "pi pi-pen-to-square",
        command: () => this.editPost(),
      },
    ];

    if (isDraft || isArchived) {
      items.push({
        label: "Publish",
        icon: "pi pi-send",
        command: () => this.publishPost(),
      });
    }

    if (isPublished) {
      items.push({
        label: "Unpublish",
        icon: "pi pi-eye-slash",
        command: () => this.unpublishPost(),
      });
    }

    items.push(
      { separator: true },
      {
        label: "Delete",
        icon: "pi pi-trash",
        styleClass: "text-red-600",
        command: () => this.confirmDelete(),
      },
    );

    return items;
  });

  protected openActionMenu(
    post: BlogPostDto,
    menu: { toggle: (event: Event) => void },
    event: Event,
  ): void {
    this.selectedPost.set(post);
    menu.toggle(event);
  }

  protected search(value: string): void {
    this.store.setSearch(value);
  }

  protected addBlogPost(): void {
    this.router.navigate(["/blog-posts/add"]);
  }

  private editPost(): void {
    const post = this.selectedPost();
    if (post) {
      this.router.navigate(["/blog-posts", post.id, "edit"]);
    }
  }

  private async publishPost(): Promise<void> {
    const post = this.selectedPost();
    if (!post?.id) return;

    await this.api.invoke(publishBlogPost, { id: post.id });
    this.toastService.showSuccess("Blog post has been published successfully");
    this.store.retry();
  }

  private async unpublishPost(): Promise<void> {
    const post = this.selectedPost();
    if (!post?.id) return;

    await this.api.invoke(unpublishBlogPost, { id: post.id });
    this.toastService.showSuccess("Blog post has been unpublished successfully");
    this.store.retry();
  }

  private confirmDelete(): void {
    const post = this.selectedPost();
    if (!post?.id) return;

    this.toastService.confirmDelete("blog post", () => this.deleteBlogPost(post.id!));
  }

  private async deleteBlogPost(id: string): Promise<void> {
    await this.api.invoke(deleteBlogPost, { id });
    this.toastService.showSuccess("The blog post has been deleted successfully");
    this.store.removeItem(id);
  }

  protected getStatusSeverity(
    status: string,
  ): "success" | "secondary" | "info" | "warn" | "danger" | "contrast" {
    switch (status?.toLowerCase()) {
      case "published":
        return "success";
      case "draft":
        return "secondary";
      case "archived":
        return "warn";
      default:
        return "secondary";
    }
  }
}
