import {Injectable} from "@angular/core";

@Injectable()
export class StorageService {
  get<T = unknown>(name: string): T | null {
    try {
      return JSON.parse(localStorage.getItem(name) as string) as T;
    }
    catch (error) {
      console.error("Error while parsing data from local storage", error);
      return null;
    }
  }

  set<T = unknown>(name: string, value: T): void {
    localStorage.setItem(name, JSON.stringify(value));
  }

  remove(name: string): void {
    localStorage.removeItem(name);
  }
}
