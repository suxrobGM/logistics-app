import { Injectable } from '@angular/core';

/**
 * Global configuration for API endpoints
 */
@Injectable({ providedIn: 'root' })
export class ApiConfiguration {
  rootUrl: string = '';
}

/**
 * Parameters for API configuration
 */
export interface ApiConfigurationParams {
  rootUrl?: string;
}
