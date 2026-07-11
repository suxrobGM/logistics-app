import { DatePipe } from "@angular/common";
import { Component, inject } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Icon, Skeleton } from "@logistics/shared/ui";
import { Avatar, FilterTabs, SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";
import { getReadTime } from "@/shared/utils";
import { BlogStore } from "../../store/blog.store";

@Component({
  selector: "web-blog-list",
  templateUrl: "./blog-list.html",
  imports: [
    Avatar,
    DatePipe,
    FilterTabs,
    Icon,
    RouterLink,
    ScrollAnimateDirective,
    SectionContainer,
    SectionHeader,
    Skeleton,
  ],
})
export class BlogList {
  protected readonly store = inject(BlogStore);
  protected readonly getReadTime = getReadTime;

  protected selectCategory(category: string): void {
    this.store.selectCategory(category);
  }

  protected getCategoryColor(category: string): string {
    const colors: Record<string, string> = {
      Industry: "bg-blue-100 text-blue-700",
      Product: "bg-purple-100 text-purple-700",
      Tips: "bg-green-100 text-green-700",
      Company: "bg-amber-100 text-amber-700",
    };
    return colors[category ?? ""] || "bg-gray-100 text-gray-700";
  }
}
