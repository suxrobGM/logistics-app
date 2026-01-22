import { computed, inject } from "@angular/core";
import { Api, getPublishedBlogPosts } from "@logistics/shared/api";
import type { BlogPostDto } from "@logistics/shared/api";
import { patchState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";

interface BlogState {
  posts: BlogPostDto[];
  isLoading: boolean;
  error: string | null;
  selectedCategory: string;
}

const initialState: BlogState = {
  posts: [],
  isLoading: true,
  error: null,
  selectedCategory: "All",
};

export const BlogStore = signalStore(
  withState(initialState),

  withComputed((store) => ({
    featuredPost: computed(() => {
      const posts = store.posts();
      return posts.find((p) => p.isFeatured) ?? posts[0] ?? null;
    }),

    categories: computed((): string[] => {
      const cats = store
        .posts()
        .map((p) => p.category)
        .filter((c): c is string => c != null && c !== "");
      return ["All", ...new Set(cats)];
    }),

    filteredPosts: computed(() => {
      const category = store.selectedCategory();
      const posts = store.posts();
      if (category === "All") {
        return posts;
      }
      return posts.filter((p) => p.category === category);
    }),
  })),

  withMethods((store, api = inject(Api)) => ({
    async loadPosts(): Promise<void> {
      patchState(store, { isLoading: true, error: null });

      try {
        const response = await api.invoke(getPublishedBlogPosts, {
          PageSize: 100,
          OrderBy: "-PublishedAt",
        });
        patchState(store, {
          posts: response.items ?? [],
          isLoading: false,
        });
      } catch (err) {
        console.error("Failed to load blog posts:", err);
        patchState(store, {
          error: "Failed to load blog posts. Please try again later.",
          isLoading: false,
        });
      }
    },

    selectCategory(category: string): void {
      patchState(store, { selectedCategory: category });
    },
  })),
);
