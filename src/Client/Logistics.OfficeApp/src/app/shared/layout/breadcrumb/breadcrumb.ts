import { Component, inject, signal } from "@angular/core";
import { ActivatedRoute, NavigationEnd, Router } from "@angular/router";
import { MenuItem } from "primeng/api";
import { BreadcrumbModule } from "primeng/breadcrumb";
import { filter } from "rxjs";

@Component({
  selector: "app-breadcrumb",
  templateUrl: "./breadcrumb.html",
  styleUrl: "./breadcrumb.css",
  imports: [BreadcrumbModule],
})
export class Breadcrumb {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  protected readonly home: MenuItem;
  protected readonly menuItems = signal<MenuItem[]>([]);

  constructor() {
    this.home = {
      icon: "pi pi-home",
      routerLink: "/",
    };

    this.router.events.pipe(filter((event) => event instanceof NavigationEnd)).subscribe(() => {
      this.menuItems.set(this.createBreadcrumbs(this.route.root));
    });
  }

  private createBreadcrumbs(route: ActivatedRoute, url = "", items: MenuItem[] = []): MenuItem[] {
    if (!route) {
      return items;
    }

    for (const child of route.children) {
      const routeURL = child.snapshot.url.map((segment) => segment.path).join("/");
      if (routeURL !== "") {
        url += `/${routeURL}`;
      }

      const label = child.snapshot.data["breadcrumb"] as string;

      if (label) {
        items.push({ label, routerLink: url });
      }

      this.createBreadcrumbs(child, url, items);
    }

    return items;
  }
}
