import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {Router} from '@angular/router';
import {AuthService} from '@core/auth';


@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  encapsulation: ViewEncapsulation.None,
})
export class LoginComponent implements OnInit {
  isAuthenticated: boolean;
  isLoading: boolean;

  constructor(
    private authService: AuthService,
    private router: Router)
  {
    this.isAuthenticated = false;
    this.isLoading = false;
  }

  ngOnInit(): void {
    this.authService.onCheckingAuth().subscribe(() => this.isLoading = true);
    this.authService.onCheckingAuthFinished().subscribe(() => {
      this.isLoading = false;
      this.redirectToHome();
    });

    this.authService.onAuthenticated().subscribe((isAuthenticated) => {
      this.isAuthenticated = isAuthenticated;
      this.redirectToHome();
    });
  }

  private redirectToHome() {
    if (this.isAuthenticated) {
      this.router.navigateByUrl('/home');
    }
  }
}
