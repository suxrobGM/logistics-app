import {HttpInterceptorFn, provideHttpClient, withInterceptors} from "@angular/common/http";
import {EnvironmentProviders, Provider} from "@angular/core";
import {InjectionToken} from "@angular/core";
import {ApiService} from "./api.service";
import {tokenAuthInterceptor} from "./interceptors/token-auth.interceptor";

/**
 * Configuration for the API service.
 */
export interface ApiConfig {
  /**
   * Base path for the API.
   */
  baseUrl?: string;
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

export function provideApi(config: ApiConfig = {}): (Provider | EnvironmentProviders)[] {
  return [
    provideHttpClient(withInterceptors([tokenAuthInterceptor, ...(config.interceptors || [])])),

    ApiService,

    // expose the config object to the DI graph
    {provide: API_CONFIG, useValue: config},
  ];
}

export const API_CONFIG = new InjectionToken<ApiConfig>("Api Config");
