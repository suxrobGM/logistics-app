import {TestBed} from '@angular/core/testing';
import {TenantInterceptor} from './tenant.interceptor';

describe('TenantInterceptor', () => {
  beforeEach(() => TestBed.configureTestingModule({
    providers: [
      TenantInterceptor,
    ],
  }));

  it('should be created', () => {
    const interceptor: TenantInterceptor = TestBed.inject(TenantInterceptor);
    expect(interceptor).toBeTruthy();
  });
});
