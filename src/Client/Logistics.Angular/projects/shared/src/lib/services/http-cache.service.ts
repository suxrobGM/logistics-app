import { Injectable } from "@angular/core";

interface CacheEntry {
  response: unknown;
  expiry: number;
}

/**
 * Simple in-memory HTTP cache service.
 */
@Injectable({ providedIn: "root" })
export class HttpCacheService {
  private readonly cache = new Map<string, CacheEntry>();

  /**
   * Retrieves a cached response if it exists and is not expired.
   * @param key Cache key
   * @returns Cached response or null if not found/expired
   */
  get<T>(key: string): T | null {
    const entry = this.cache.get(key);
    if (!entry) return null;
    if (Date.now() > entry.expiry) {
      this.cache.delete(key);
      return null;
    }
    return entry.response as T;
  }

  /**
   * Caches a response with a specified TTL.
   * @param key Cache key
   * @param response Response to cache
   * @param ttlMs Time to live in milliseconds
   */
  set(key: string, response: unknown, ttlMs: number): void {
    this.cache.set(key, {
      response,
      expiry: Date.now() + ttlMs,
    });
  }

  /**
   * Invalidates cache entries matching the given pattern.
   * @param pattern String or RegExp to match cache keys
   */
  invalidate(pattern: string | RegExp): void {
    const keys = Array.from(this.cache.keys());
    for (const key of keys) {
      if (typeof pattern === "string" ? key.includes(pattern) : pattern.test(key)) {
        this.cache.delete(key);
      }
    }
  }

  /**
   * Clears the entire cache.
   */
  clear(): void {
    this.cache.clear();
  }
}
