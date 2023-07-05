import {Injectable} from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
} from '@angular/common/http';
import {Observable} from 'rxjs';
import {OidcSecurityService} from 'angular-auth-oidc-client';

@Injectable()
export class TokenInterceptor implements HttpInterceptor {
  constructor(private oidcSecurityService: OidcSecurityService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    let token = '';
    this.oidcSecurityService.getAccessToken().subscribe((i) => token = i);
    const headers = {Authorization: ''};

    if (!request.headers.get('bypassAuthorization')) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const newRequest = request.clone({
      setHeaders: headers,
    });

    return next.handle(newRequest);
  }
}
