import { Component, inject, type OnInit } from "@angular/core";
import { BlogHero, BlogList, FeaturedPost } from "./sections";
import { BlogStore } from "./store/blog.store";

@Component({
  selector: "web-blog",
  templateUrl: "./blog.html",
  imports: [BlogHero, FeaturedPost, BlogList],
  providers: [BlogStore],
})
export class Blog implements OnInit {
  private readonly store = inject(BlogStore);

  ngOnInit(): void {
    this.store.loadPosts();
  }
}
