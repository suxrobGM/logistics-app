import { DatePipe } from "@angular/common";
import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { Api, getPublishedBlogPostBySlug } from "@logistics/shared/api";
import type { BlogPostDto } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { SkeletonModule } from "primeng/skeleton";
import { Avatar, SectionContainer } from "@/shared/components";
import { getReadTime } from "@/shared/utils";

@Component({
  selector: "web-blog-post",
  templateUrl: "./blog-post.html",
  imports: [SectionContainer, Avatar, ButtonModule, SkeletonModule, DatePipe, RouterLink],
})
export class BlogPost implements OnInit {
  private readonly api = inject(Api);
  private readonly router = inject(Router);

  protected readonly slug = input<string>();
  protected readonly post = signal<BlogPostDto | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly getReadTime = getReadTime;

  async ngOnInit(): Promise<void> {
    const slug = this.slug();

    if (!slug) {
      this.router.navigate(["/blog"]);
      return;
    }

    try {
      const result = await this.api.invoke(getPublishedBlogPostBySlug, { slug });
      this.post.set(result);
    } catch {
      this.error.set("Blog post not found");
    } finally {
      this.isLoading.set(false);
    }
  }
}
