import { isPlatformBrowser } from "@angular/common";
import { inject, Injectable, PLATFORM_ID } from "@angular/core";

@Injectable({ providedIn: "root" })
export class StorageService {
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  get<T = unknown>(name: string): T | null {
    if (!this.isBrowser) {
      return null;
    }
    try {
      return JSON.parse(localStorage.getItem(name) as string) as T;
    } catch (error) {
      console.error("Error while parsing data from local storage", error);
      return null;
    }
  }

  set<T = unknown>(name: string, value: T): void {
    if (!this.isBrowser) {
      return;
    }
    localStorage.setItem(name, JSON.stringify(value));
  }

  remove(name: string): void {
    if (!this.isBrowser) {
      return;
    }
    localStorage.removeItem(name);
  }
}
