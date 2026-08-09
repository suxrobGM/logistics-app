import { inject, Injectable, signal } from "@angular/core";
import { Permission } from "@logistics/shared";
import type { IconName } from "@logistics/shared/ui";
import { PermissionService } from "@/core/auth";
import type { NavSection } from "@/shared/layout/nav-menu";
import { RecentPagesService } from "./recent-pages.service";

export interface SearchableItem {
  id: string;
  label: string;
  section: string;
  /** Canonical `<ui-icon name>` - the palette renders it through `<ui-icon>`, not a class string. */
  icon?: IconName;
  route?: string;
  type: "page" | "action" | "recent";
  keywords?: string[];
}

/** Page rows inherit their gate from the `NavItem`; these have no nav entry, so they carry their own. */
const QUICK_ACTIONS: (SearchableItem & { permission: string })[] = [
  {
    id: "action-create-load",
    label: "Create Load",
    section: "Quick Actions",
    icon: "plus",
    route: "/loads/add",
    type: "action",
    keywords: ["new", "add", "load"],
    permission: Permission.Load.Manage,
  },
  {
    id: "action-add-employee",
    label: "Add Employee",
    section: "Quick Actions",
    icon: "user-plus",
    route: "/employees/add",
    type: "action",
    keywords: ["new", "hire", "driver"],
    permission: Permission.Employee.Manage,
  },
  {
    id: "action-add-customer",
    label: "Add Customer",
    section: "Quick Actions",
    icon: "building-2",
    route: "/customers/add",
    type: "action",
    keywords: ["new", "client"],
    permission: Permission.Customer.Manage,
  },
  {
    id: "action-add-expense",
    label: "Add Expense",
    section: "Quick Actions",
    icon: "banknote",
    route: "/expenses/add",
    type: "action",
    keywords: ["new", "cost"],
    permission: Permission.Expense.Manage,
  },
  {
    id: "action-add-truck",
    label: "Add Truck",
    section: "Quick Actions",
    icon: "truck",
    route: "/trucks/add",
    type: "action",
    keywords: ["new", "vehicle", "fleet"],
    permission: Permission.Truck.Manage,
  },
];

@Injectable({ providedIn: "root" })
export class CommandPaletteService {
  private readonly recentPagesService = inject(RecentPagesService);
  private readonly permissionService = inject(PermissionService);

  public readonly isOpen = signal(false);

  private searchIndex: SearchableItem[] = [];
  private quickActions: SearchableItem[] = [];

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

    this.quickActions = QUICK_ACTIONS.filter((action) =>
      this.permissionService.hasPermission(action.permission),
    );
    this.searchIndex = [...items, ...this.quickActions];
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
      icon: "history",
    }));

    return [...recent, ...this.quickActions.slice(0, 3)];
  }
}
