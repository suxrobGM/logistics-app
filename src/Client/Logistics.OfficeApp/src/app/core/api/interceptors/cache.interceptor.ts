import { type HttpInterceptorFn, HttpResponse } from "@angular/common/http";
import { inject } from "@angular/core";
import { of } from "rxjs";
import { tap } from "rxjs/operators";
import { HttpCacheService } from "../../services/http-cache.service";
import { getTtlForUrl } from "../cache.config";

/**
 * HTTP interceptor for caching GET requests based on predefined TTL rules.
 * Invalidates cache on mutation requests (POST, PUT, PATCH, DELETE).
 * @param req The request object
 * @param next The next interceptor in the chain
 * @returns The HTTP event stream
 */
export const cacheInterceptor: HttpInterceptorFn = (req, next) => {
  const cacheService = inject(HttpCacheService);

  // Only cache GET requests
  if (req.method !== "GET") {
    // Invalidate cache on mutations
    if (["POST", "PUT", "PATCH", "DELETE"].includes(req.method)) {
      const basePath = getBasePath(req.url);
      cacheService.invalidate(basePath);
    }
    return next(req);
  }

  // Generate cache key (includes tenant header for multi-tenant support)
  const tenantId = req.headers.get("X-Tenant") ?? "";
  const cacheKey = `${tenantId}:${req.urlWithParams}`;

  // Check cache
  const cached = cacheService.get<HttpResponse<unknown>>(cacheKey);
  if (cached) {
    return of(cached.clone());
  }

  // Get TTL for this URL
  const ttl = getTtlForUrl(req.url);
  if (ttl === null) {
    return next(req); // Don't cache if no rule matches
  }

  // Make request and cache response
  return next(req).pipe(
    tap((event) => {
      if (event instanceof HttpResponse && event.ok) {
        cacheService.set(cacheKey, event.clone(), ttl);
      }
    }),
  );
};

/**
 * Extracts the base API path from a URL for cache invalidation.
 * e.g., from "/api/drivers/123" returns "/drivers"
 * @param url The full request URL
 * @returns The base API path
 */
function getBasePath(url: string): string {
  const match = url.match(/\/api(\/[^/?]+)/);
  return match ? match[1] : url;
}
