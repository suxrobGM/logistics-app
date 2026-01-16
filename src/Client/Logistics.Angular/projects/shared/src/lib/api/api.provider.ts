import { Provider } from '@angular/core';
import { ApiConfiguration, ApiConfigurationParams } from './api-configuration';

/**
 * Provider for API configuration
 */
export function provideApi(params: ApiConfigurationParams): Provider[] {
  return [
    {
      provide: ApiConfiguration,
      useValue: params,
    },
  ];
}
