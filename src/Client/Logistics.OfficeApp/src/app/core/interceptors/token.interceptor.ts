import {inject} from "@angular/core";
import {HttpRequest, HttpEvent, HttpHandlerFn} from "@angular/common/http";
import {Observable} from "rxjs";
import {AuthService} from "@/core/auth";

export function tokenInterceptor(request: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> {
  const authService = inject(AuthService);
  let token = "";
  authService.getAccessToken().subscribe((i) => (token = i));
  const headers = {Authorization: ""};

  if (!request.headers.get("bypassAuthorization")) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  const newRequest = request.clone({
    setHeaders: headers,
  });
  return next(newRequest);
}
