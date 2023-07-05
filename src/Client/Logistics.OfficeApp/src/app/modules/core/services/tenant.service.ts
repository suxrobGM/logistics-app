import {HttpHeaders} from '@angular/common/http';
import {Injectable} from '@angular/core';
import {CookieService} from '../../../shared/services/cookie.service';

@Injectable()
export class TenantService {
  constructor(private cookieService: CookieService) { }

  public getTenantId(): string {
    const urlParams = new URLSearchParams(window.location.search);
    const tenantSubDomain = this.getSubDomain(location.host);
    const tenantQuery = urlParams.get('tenant');
    const tenantCookie = this.cookieService.getCookie('X-Tenant');
    let tenantId = 'default';

    if (tenantSubDomain) {
      tenantId = tenantSubDomain;
    } else if (tenantQuery) {
      tenantId = tenantQuery;
    } else if (tenantCookie) {
      tenantId = tenantCookie;
    }

    if (tenantId === 'office') {
      tenantId = 'default';
    }

    return tenantId;
  }

  public createTenantHeaders(headers: HttpHeaders, tenantId: string): HttpHeaders {
    return headers.append('X-Tenant', tenantId);
  }

  public setTenantId(tenantId: string) {
    if (!tenantId) {
      return;
    }

    const currentTenant = this.cookieService.getCookie('X-Tenant');

    if (tenantId === currentTenant) {
      return;
    }

    this.cookieService.setCookie({
      name: 'X-Tenant',
      value: tenantId,
      session: true,
    });
  }

  private getSubDomain(host: string) {
    let subDomain = '';
    const domains = host.split('.');

    if (domains.length <= 2)
    {return subDomain;}

    subDomain = domains[0];
    return subDomain;
  }
}
