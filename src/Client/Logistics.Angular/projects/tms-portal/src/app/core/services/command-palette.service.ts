import { Injectable, inject, signal } from "@angular/core";
import type { NavItem, NavSection } from "@/shared/layout/nav-menu";
import { RecentPagesService } from "./recent-pages.service";

export interface SearchableItem {
  id: string;
  label: string;
  section: string;
  icon?: string;
  route?: string;
  type: "page" | "action" | "recent";
  keywords?: string[];
}

const QUICK_ACTIONS: SearchableItem[] = [
  {
    id: "action-create-load",
    label: "Create Load",
    section: "Quick Actions",
    icon: "pi pi-plus",
    route: "/loads/add",
    type: "action",
    keywords: ["new", "add", "load"],
  },
  {
    id: "action-add-employee",
    label: "Add Employee",
    section: "Quick Actions",
    icon: "pi pi-user-plus",
    route: "/employees/add",
    type: "action",
    keywords: ["new", "hire", "driver"],
  },
  {
    id: "action-add-customer",
    label: "Add Customer",
    section: "Quick Actions",
    icon: "pi pi-building",
    route: "/customers/add",
    type: "action",
    keywords: ["new", "client"],
  },
  {
    id: "action-add-expense",
    label: "Add Expense",
    section: "Quick Actions",
    icon: "pi pi-money-bill",
    route: "/expenses/add",
    type: "action",
    keywords: ["new", "cost"],
  },
  {
    id: "action-add-truck",
    label: "Add Truck",
    section: "Quick Actions",
    icon: "pi pi-truck",
    route: "/trucks/add",
    type: "action",
    keywords: ["new", "vehicle", "fleet"],
  },
];

@Injectable({ providedIn: "root" })
export class CommandPaletteService {
  private readonly recentPagesService = inject(RecentPagesService);

  public readonly isOpen = signal(false);

  private searchIndex: SearchableItem[] = [];

  open(): void {
    this.isOpen.set(true);
  }

  close(): void {
    this.isOpen.set(false);
  }

  toggle(): void {
    this.isOpen.set(!this.isOpen());
  }

  buildIndex(sections: NavSection[]): void {
    const items: SearchableItem[] = [];
    const routeLabels = new Map<string, string>();

    for (const section of sections) {
      for (const item of section.items) {
        if (item.route) {
          items.push({
            id: item.id,
            label: item.label,
            section: section.label,
            icon: item.icon,
            route: item.route,
            type: "page",
          });
          routeLabels.set(item.route, item.label);
        }

        if (item.children) {
          for (const child of item.children) {
            if (child.route) {
              items.push({
                id: child.id,
                label: child.label,
                section: `${section.label} > ${item.label}`,
                icon: item.icon,
                route: child.route,
                type: "page",
              });
              routeLabels.set(child.route, child.label);
            }
          }
        }
      }
    }

    this.searchIndex = [...items, ...QUICK_ACTIONS];
    this.recentPagesService.setRouteLabels(routeLabels);
  }

  search(query: string): SearchableItem[] {
    if (!query.trim()) {
      return this.getDefaultResults();
    }

    const lower = query.toLowerCase();
    return this.searchIndex
      .filter((item) => {
        if (item.label.toLowerCase().includes(lower)) return true;
        if (item.section.toLowerCase().includes(lower)) return true;
        if (item.keywords?.some((k) => k.includes(lower))) return true;
        return false;
      })
      .slice(0, 10);
  }

  private getDefaultResults(): SearchableItem[] {
    const recent: SearchableItem[] = this.recentPagesService.recentPages().map((p) => ({
      id: `recent-${p.route}`,
      label: p.label,
      section: "Recent",
      route: p.route,
      type: "recent" as const,
      icon: "pi pi-history",
    }));

    return [...recent, ...QUICK_ACTIONS.slice(0, 3)];
  }
}
