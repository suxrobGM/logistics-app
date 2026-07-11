import { DatePipe } from "@angular/common";
import { Component, inject } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Icon, Skeleton, UiButton } from "@logistics/shared/ui";
import { Avatar, SectionContainer } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";
import { getReadTime } from "@/shared/utils";
import { BlogStore } from "../../store/blog.store";

@Component({
  selector: "web-featured-post",
  templateUrl: "./featured-post.html",
  imports: [
    Avatar,
    DatePipe,
    Icon,
    RouterLink,
    ScrollAnimateDirective,
    SectionContainer,
    Skeleton,
    UiButton,
  ],
})
export class FeaturedPost {
  protected readonly store = inject(BlogStore);
  protected readonly getReadTime = getReadTime;
}
