import type { Routes } from "@angular/router";
import { BlogPostsList } from "./blog-posts-list/blog-posts-list";
import { BlogPostAdd } from "./blog-post-add/blog-post-add";
import { BlogPostEdit } from "./blog-post-edit/blog-post-edit";

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
