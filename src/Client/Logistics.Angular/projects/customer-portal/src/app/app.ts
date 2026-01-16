import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'cp-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.html',
})
export class App {}
