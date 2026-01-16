import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'cp-dashboard',
  standalone: true,
  imports: [RouterLink, CardModule, ButtonModule],
  templateUrl: './dashboard.html',
})
export class Dashboard {}
