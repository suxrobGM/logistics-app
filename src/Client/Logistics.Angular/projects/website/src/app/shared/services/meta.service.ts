import { DOCUMENT } from "@angular/common";
import { Injectable, inject } from "@angular/core";
import { Meta, Title } from "@angular/platform-browser";

export interface PageMeta {
  title: string;
  description: string;
  canonicalUrl: string;
  keywords?: string;
  ogImage?: string;
  ogType?: "website" | "article";
  noIndex?: boolean;
}

@Injectable({ providedIn: "root" })
export class MetaService {
  private readonly meta = inject(Meta);
  private readonly title = inject(Title);
  private readonly document = inject(DOCUMENT);

  public updateMeta(config: PageMeta): void {
    this.title.setTitle(`${config.title} | Logistics TMS`);

    this.meta.updateTag({ name: "description", content: config.description });

    if (config.keywords) {
      this.meta.updateTag({ name: "keywords", content: config.keywords });
    }

    this.meta.updateTag({ property: "og:title", content: config.title });
    this.meta.updateTag({ property: "og:description", content: config.description });
    this.meta.updateTag({ property: "og:type", content: config.ogType ?? "website" });
    this.meta.updateTag({ property: "og:url", content: config.canonicalUrl });

    if (config.ogImage) {
      this.meta.updateTag({ property: "og:image", content: config.ogImage });
    }

    this.meta.updateTag({ name: "twitter:title", content: config.title });
    this.meta.updateTag({ name: "twitter:description", content: config.description });

    if (config.ogImage) {
      this.meta.updateTag({ name: "twitter:image", content: config.ogImage });
    }

    this.setCanonicalUrl(config.canonicalUrl);

    if (config.noIndex) {
      this.meta.updateTag({ name: "robots", content: "noindex, nofollow" });
    } else {
      this.meta.removeTag('name="robots"');
    }
  }

  private setCanonicalUrl(url: string): void {
    let link: HTMLLinkElement | null = this.document.querySelector('link[rel="canonical"]');

    if (!link) {
      link = this.document.createElement("link");
      link.setAttribute("rel", "canonical");
      this.document.head.appendChild(link);
    }

    link.setAttribute("href", url);
  }
}
