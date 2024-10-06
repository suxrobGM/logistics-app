import {Component, OnInit, ViewEncapsulation} from '@angular/core';
import {NgIf} from '@angular/common';
import {Router} from '@angular/router';
import {ProgressSpinnerModule} from 'primeng/progressspinner';
import {ButtonModule} from 'primeng/button';
import {AuthService} from '@/core/auth';


@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  encapsulation: ViewEncapsulation.None,
  standalone: true,
  imports: [
    NgIf,
    ProgressSpinnerModule,
    ButtonModule,
  ],
})
export class LoginComponent implements OnInit {
  public isAuthenticated: boolean;
  public isLoading: boolean;

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

  login() {
    this.isLoading = true;
    this.authService.login();
  }

  private redirectToHome() {
    if (this.isAuthenticated) {
      this.router.navigateByUrl('/home');
    }
  }
}
