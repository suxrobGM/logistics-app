import type { Routes } from "@angular/router";
import { BlogPostAdd } from "./blog-post-add/blog-post-add";
import { BlogPostEdit } from "./blog-post-edit/blog-post-edit";
import { BlogPostsList } from "./blog-posts-list/blog-posts-list";

export const blogPostsRoutes: Routes = [
  {
    path: "",
    component: BlogPostsList,
    data: { breadcrumb: "Blog Posts" },
  },
  {
    path: "add",
    component: BlogPostAdd,
    data: { breadcrumb: "Add" },
  },
  {
    path: ":id/edit",
    component: BlogPostEdit,
    data: { breadcrumb: "Edit" },
  },
];
