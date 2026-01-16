import { Injectable, inject, signal } from "@angular/core";
import { Api, getCurrentUserPermissions } from "@logistics/shared/api";

@Injectable({ providedIn: "root" })
export class PermissionService {
  private readonly api = inject(Api);

  private readonly _permissions = signal<string[]>([]);
  private loaded = false;

  public readonly permissions = this._permissions.asReadonly();

  async loadPermissions(): Promise<void> {
    if (this.loaded) {
      return;
    }

    try {
      const permissions = await this.api.invoke(getCurrentUserPermissions);
      this._permissions.set(permissions);
      this.loaded = true;
    } catch (error) {
      console.error("Failed to load permissions:", error);
      this._permissions.set([]);
    }
  }

  hasPermission(permission: string): boolean {
    return this._permissions().includes(permission);
  }

  hasAnyPermission(...permissions: string[]): boolean {
    return permissions.some((p) => this._permissions().includes(p));
  }

  hasAllPermissions(...permissions: string[]): boolean {
    return permissions.every((p) => this._permissions().includes(p));
  }

  clearPermissions(): void {
    this._permissions.set([]);
    this.loaded = false;
  }
}
