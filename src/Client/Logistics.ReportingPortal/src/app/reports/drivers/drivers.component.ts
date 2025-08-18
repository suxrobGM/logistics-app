import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpParams } from '@angular/common/http';

@Component({
  selector: 'app-drivers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './drivers.component.html',
  styleUrls: ['./drivers.component.scss']
})
export class DriversComponent {
  private http = inject(HttpClient);
  apiBase = '/reports';

  from?: string;
  to?: string;
  search = '';
  page = 1;
  pageSize = 10;

  items = signal<any[]>([]);
  totalCount = signal(0);
  totalGross = signal(0);
  totalDistance = signal(0);

  ngOnInit() {
    this.load();
  }

  load() {
    let params = new HttpParams()
      .set('page', this.page)
      .set('pageSize', this.pageSize);
    if (this.from) params = params.set('from', this.from);
    if (this.to) params = params.set('to', this.to);
    if (this.search) params = params.set('search', this.search);
    this.http.get<any>(`${this.apiBase}/drivers`, { params }).subscribe(res => {
      if (res.success) {
        this.items.set(res.data.items);
        this.totalCount.set(res.data.totalCount);
        this.totalGross.set(res.data.totalGross);
        this.totalDistance.set(res.data.totalDistance);
      }
    });
  }

  reset() {
    this.from = undefined;
    this.to = undefined;
    this.search = '';
    this.page = 1;
    this.pageSize = 10;
    this.load();
  }

  export(format: string) {
    let params = new HttpParams();
    if (this.from) params = params.set('from', this.from);
    if (this.to) params = params.set('to', this.to);
    if (this.search) params = params.set('search', this.search);
    params = params.set('format', format);
    this.http.get(`${this.apiBase}/drivers/export`, { params, responseType: 'blob', observe: 'response' }).subscribe(resp => {
      const contentDisposition = resp.headers.get('content-disposition') || '';
      const match = /filename="?([^";]+)"?/.exec(contentDisposition);
      const filename = match ? match[1] : `drivers-report.${format}`;
      const blob = new Blob([resp.body as BlobPart]);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = filename;
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }
}

