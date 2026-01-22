import { Component, signal } from "@angular/core";
import { Avatar, FilterTabs, SectionContainer, SectionHeader } from "@/shared/components";
import { ScrollAnimateDirective } from "@/shared/directives";

interface BlogPost {
  id: string;
  title: string;
  excerpt: string;
  category: string;
  author: string;
  authorInitials: string;
  date: string;
  readTime: string;
}

@Component({
  selector: "web-blog-list",
  templateUrl: "./blog-list.html",
  imports: [SectionContainer, SectionHeader, ScrollAnimateDirective, Avatar, FilterTabs],
})
export class BlogList {
  protected readonly categories = ["All", "Industry", "Product", "Tips", "Company"];
  protected readonly selectedCategory = signal("All");

  protected readonly posts: BlogPost[] = [
    {
      id: "1",
      title: "5 Ways to Reduce Fuel Costs in Your Fleet",
      excerpt:
        "Fuel is often the largest operating expense for trucking companies. Here are proven strategies to cut costs without sacrificing performance.",
      category: "Tips",
      author: "Sarah Rodriguez",
      authorInitials: "SR",
      date: "January 10, 2026",
      readTime: "5 min read",
    },
    {
      id: "2",
      title: "New Feature: Real-Time Driver Messaging",
      excerpt:
        "Introducing our new in-app messaging feature that keeps dispatchers and drivers connected like never before.",
      category: "Product",
      author: "David Thompson",
      authorInitials: "DT",
      date: "January 5, 2026",
      readTime: "3 min read",
    },
    {
      id: "3",
      title: "ELD Compliance: What Every Fleet Manager Needs to Know",
      excerpt:
        "Navigate the complexities of ELD regulations and ensure your fleet stays compliant with FMCSA requirements.",
      category: "Industry",
      author: "Michael Chen",
      authorInitials: "MC",
      date: "December 28, 2025",
      readTime: "7 min read",
    },
    {
      id: "4",
      title: "How ABC Trucking Reduced Costs by 23%",
      excerpt:
        "Learn how this mid-sized trucking company transformed their operations and dramatically cut costs with our platform.",
      category: "Company",
      author: "Emily Watson",
      authorInitials: "EW",
      date: "December 20, 2025",
      readTime: "6 min read",
    },
    {
      id: "5",
      title: "The Complete Guide to Driver Retention",
      excerpt:
        "Driver turnover is a major challenge in trucking. Discover strategies to keep your best drivers happy and engaged.",
      category: "Tips",
      author: "James Miller",
      authorInitials: "JM",
      date: "December 15, 2025",
      readTime: "10 min read",
    },
    {
      id: "6",
      title: "2026 Trucking Industry Trends to Watch",
      excerpt:
        "From autonomous vehicles to sustainability initiatives, here are the trends shaping the future of trucking.",
      category: "Industry",
      author: "Lisa Park",
      authorInitials: "LP",
      date: "December 10, 2025",
      readTime: "8 min read",
    },
  ];

  protected get filteredPosts(): BlogPost[] {
    if (this.selectedCategory() === "All") {
      return this.posts;
    }
    return this.posts.filter((p) => p.category === this.selectedCategory());
  }

  protected selectCategory(category: string): void {
    this.selectedCategory.set(category);
  }

  protected getCategoryColor(category: string): string {
    const colors: Record<string, string> = {
      Industry: "bg-blue-100 text-blue-700",
      Product: "bg-purple-100 text-purple-700",
      Tips: "bg-green-100 text-green-700",
      Company: "bg-amber-100 text-amber-700",
    };
    return colors[category] || "bg-gray-100 text-gray-700";
  }
}
