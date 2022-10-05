import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { Router } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class HomeComponent implements OnInit {
  isAuthenticated: boolean;
  isLoading: boolean;

  constructor(
    private oidcService: OidcSecurityService,
    private router: Router) 
  {
    this.isAuthenticated = false;
    this.isLoading = false;
  }

  ngOnInit(): void {
    this.oidcService.isLoading$.subscribe((isLoading) => {
      this.isLoading = isLoading;

      if (this.isAuthenticated && !this.isLoading) {
        this.router.navigateByUrl('/dashboard');
      }
    });

    this.oidcService.isAuthenticated$.subscribe(({isAuthenticated}) => {
      this.isAuthenticated = isAuthenticated;
    });

    this.oidcService.isAuthenticated().subscribe(isAuthenticated => {
      if (isAuthenticated) {
        this.router.navigateByUrl('/dashboard');
      }
    });
  }
}
