import { Component } from "@angular/core";
import { BlogHero, BlogList, FeaturedPost } from "./sections";

@Component({
  selector: "web-blog",
  templateUrl: "./blog.html",
  imports: [BlogHero, FeaturedPost, BlogList],
})
export class Blog {}
