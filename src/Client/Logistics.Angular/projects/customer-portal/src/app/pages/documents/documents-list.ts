import { Component } from '@angular/core';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'cp-documents-list',
  standalone: true,
  imports: [TableModule, ButtonModule],
  templateUrl: './documents-list.html',
})
export class DocumentsList {}
