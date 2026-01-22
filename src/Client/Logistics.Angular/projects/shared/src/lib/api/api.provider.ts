import type { HttpInterceptorFn } from "@angular/common/http";
import { provideHttpClient, withFetch, withInterceptors } from "@angular/common/http";
import type { EnvironmentProviders } from "@angular/core";
import { InjectionToken, makeEnvironmentProviders } from "@angular/core";
import { ApiConfiguration } from "./generated/api-configuration";
import { errorHandlerInterceptor } from "./interceptors";
import { cacheInterceptor } from "./interceptors/cache.interceptor";
import { tokenAuthInterceptor } from "./interceptors/token-auth.interceptor";

/**
 * Configuration interface for the API provider.
 */
export interface ApiConfig {
  /**
   * Base path for the API.
   */
  baseUrl: string;
  /**
   * Cookie credentials flag.
   * If true, cookies will be sent with requests.
   */
  withCredentials?: boolean;
  /**
   * Function that returns the current access token (or null).
   */
  tokenGetter?: () => string | null;

  /**
   * Interceptors to be applied to the HTTP requests.
   */
  interceptors?: HttpInterceptorFn[];
}

/**
 * Injection token for the API configuration.
 * Used by interceptors to access the API settings.
 */
export const API_CONFIG = new InjectionToken<ApiConfig>("API_CONFIG");

/**
 * Provides the API configuration and HTTP client with interceptors.
 * @param config The API configuration options.
 * @returns The environment providers for the API.
 */
export function provideApi(config: ApiConfig): EnvironmentProviders {
  const interceptors: HttpInterceptorFn[] = [
    cacheInterceptor,
    tokenAuthInterceptor,
    errorHandlerInterceptor,
    ...(config.interceptors ?? []),
  ];

  return makeEnvironmentProviders([
    { provide: API_CONFIG, useValue: config },
    {
      provide: ApiConfiguration,
      useFactory: () => {
        const apiConfig = new ApiConfiguration();
        apiConfig.rootUrl = config.baseUrl;
        return apiConfig;
      },
    },
    provideHttpClient(withInterceptors(interceptors), withFetch()),
  ]);
}
