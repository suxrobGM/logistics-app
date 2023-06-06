import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { Router } from '@angular/router';
import { EventTypes, OidcSecurityService, PublicEventsService } from 'angular-auth-oidc-client';
import { filter } from 'rxjs';

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
    private eventService: PublicEventsService,
    private router: Router) {
    this.isAuthenticated = false;
    this.isLoading = false;
  }

  ngOnInit(): void {
    this.eventService.registerForEvents()
      .pipe(filter((notifaction) => notifaction.type === EventTypes.CheckingAuth))
      .subscribe((value) => {
        this.isLoading = true;
      });

    this.eventService.registerForEvents()
      .pipe(filter((notifaction) => notifaction.type === EventTypes.CheckingAuthFinished))
      .subscribe((value) => {
        this.isLoading = false;

        if (this.isAuthenticated) {
          this.router.navigateByUrl('/dashboard');
        }
      });

    this.oidcService.isAuthenticated$.subscribe(({ isAuthenticated }) => {
      this.isAuthenticated = isAuthenticated;
    });

    this.oidcService.isAuthenticated().subscribe(isAuthenticated => {
      if (isAuthenticated) {
        this.router.navigateByUrl('/dashboard');
      }
    });
  }
}
