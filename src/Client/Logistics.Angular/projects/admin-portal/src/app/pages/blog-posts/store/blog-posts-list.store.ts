import { getBlogPosts, type BlogPostDto } from "@logistics/shared/api";
import { createListStore } from "@logistics/shared/stores";

/**
 * Store for the blog posts list page.
 */
export const BlogPostsListStore = createListStore<BlogPostDto>(getBlogPosts, {
  defaultSortField: "-CreatedAt",
  defaultPageSize: 10,
});
