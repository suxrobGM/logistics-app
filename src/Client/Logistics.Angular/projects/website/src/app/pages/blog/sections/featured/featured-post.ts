import { Component } from "@angular/core";
import { Avatar, SectionContainer } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";
import { ButtonModule } from "primeng/button";

interface BlogPost {
  title: string;
  excerpt: string;
  category: string;
  author: string;
  authorInitials: string;
  date: string;
  readTime: string;
}

@Component({
  selector: "web-featured-post",
  templateUrl: "./featured-post.html",
  imports: [SectionContainer, ScrollAnimateDirective, ButtonModule, Avatar],
})
export class FeaturedPost {
  protected readonly featured: BlogPost = {
    title: "The Future of Fleet Management: AI and Predictive Analytics",
    excerpt:
      "Discover how artificial intelligence and predictive analytics are transforming fleet operations, from route optimization to predictive maintenance. Learn what these technologies mean for your business and how to prepare for the future.",
    category: "Industry",
    author: "Michael Chen",
    authorInitials: "MC",
    date: "January 15, 2026",
    readTime: "8 min read",
  };
}
