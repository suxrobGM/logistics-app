import { DatePipe } from "@angular/common";
import { Component, inject, input, signal, type OnInit } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { Api, getPublishedBlogPostBySlug, type BlogPostDto } from "@logistics/shared/api";
import { Icon, Skeleton, UiButton } from "@logistics/shared/ui";
import { Avatar, SectionContainer } from "@/shared/components";
import { getReadTime } from "@/shared/utils";

@Component({
  selector: "web-blog-post",
  templateUrl: "./blog-post.html",
  imports: [Avatar, DatePipe, Icon, RouterLink, SectionContainer, Skeleton, UiButton],
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
