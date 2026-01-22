import { getBlogPosts } from "@logistics/shared/api";
import type { BlogPostDto } from "@logistics/shared/api";
import { createListStore } from "@/shared/stores";

/**
 * Store for the blog posts list page.
 */
export const BlogPostsListStore = createListStore<BlogPostDto>(getBlogPosts, {
  defaultSortField: "-CreatedAt",
  defaultPageSize: 10,
});
