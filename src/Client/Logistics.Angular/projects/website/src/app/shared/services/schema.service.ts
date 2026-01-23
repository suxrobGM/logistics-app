import { DOCUMENT } from "@angular/common";
import { Injectable, inject } from "@angular/core";

export interface BlogPostSchema {
  title: string;
  excerpt: string;
  authorName: string;
  publishedAt: string;
  slug: string;
  featuredImage?: string;
}

export interface FaqItem {
  question: string;
  answer: string;
}

export interface BreadcrumbItem {
  name: string;
  url: string;
}

@Injectable({ providedIn: "root" })
export class SchemaService {
  private readonly document = inject(DOCUMENT);
  private readonly baseUrl = "https://logisticstms.com";

  public setOrganizationSchema(): void {
    this.setSchema("organization", {
      "@context": "https://schema.org",
      "@type": "Organization",
      name: "Logistics TMS",
      url: this.baseUrl,
      logo: `${this.baseUrl}/images/logo.png`,
      description: "Modern fleet management platform for trucking companies",
      contactPoint: {
        "@type": "ContactPoint",
        contactType: "sales",
      },
      sameAs: ["https://x.com/logisticstms", "https://linkedin.com/company/logisticstms"],
    });
  }

  public setBlogPostSchema(post: BlogPostSchema): void {
    this.setSchema("blogpost", {
      "@context": "https://schema.org",
      "@type": "BlogPosting",
      headline: post.title,
      description: post.excerpt,
      author: {
        "@type": "Person",
        name: post.authorName,
      },
      datePublished: post.publishedAt,
      image: post.featuredImage,
      url: `${this.baseUrl}/blog/${post.slug}`,
      publisher: {
        "@type": "Organization",
        name: "Logistics TMS",
        logo: {
          "@type": "ImageObject",
          url: `${this.baseUrl}/images/logo.png`,
        },
      },
    });
  }

  public setFaqSchema(faqs: FaqItem[]): void {
    this.setSchema("faq", {
      "@context": "https://schema.org",
      "@type": "FAQPage",
      mainEntity: faqs.map((faq) => ({
        "@type": "Question",
        name: faq.question,
        acceptedAnswer: {
          "@type": "Answer",
          text: faq.answer,
        },
      })),
    });
  }

  public setBreadcrumbSchema(items: BreadcrumbItem[]): void {
    this.setSchema("breadcrumb", {
      "@context": "https://schema.org",
      "@type": "BreadcrumbList",
      itemListElement: items.map((item, index) => ({
        "@type": "ListItem",
        position: index + 1,
        name: item.name,
        item: item.url,
      })),
    });
  }

  public setSoftwareApplicationSchema(): void {
    this.setSchema("software", {
      "@context": "https://schema.org",
      "@type": "SoftwareApplication",
      name: "Logistics TMS",
      applicationCategory: "BusinessApplication",
      operatingSystem: "Web",
      description: "Fleet management and dispatching software for trucking companies",
      offers: {
        "@type": "Offer",
        priceCurrency: "USD",
        price: "0",
        description: "Contact for pricing",
      },
    });
  }

  public removeSchema(id: string): void {
    const script = this.document.getElementById(`schema-${id}`);
    script?.remove();
  }

  public removeAllSchemas(): void {
    const scripts = this.document.querySelectorAll('script[type="application/ld+json"]');
    scripts.forEach((script) => script.remove());
  }

  private setSchema(id: string, schema: object): void {
    let script = this.document.getElementById(`schema-${id}`) as HTMLScriptElement | null;

    if (!script) {
      script = this.document.createElement("script");
      script.id = `schema-${id}`;
      script.type = "application/ld+json";
      this.document.head.appendChild(script);
    }

    script.textContent = JSON.stringify(schema);
  }
}
