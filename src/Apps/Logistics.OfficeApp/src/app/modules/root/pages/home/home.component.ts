import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  isAuthenticated: boolean

  constructor(
    private oidcSecurityService: OidcSecurityService,
    private router: Router) 
  {
    this.isAuthenticated = false;
  }

  ngOnInit(): void {
    this.oidcSecurityService.isAuthenticated().subscribe(result => {
      this.isAuthenticated = result;

      if (this.isAuthenticated) {
        this.router.navigateByUrl('/dashboard');
      }
    });
  }
}
