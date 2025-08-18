import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpParams } from '@angular/common/http';

@Component({
  selector: 'app-loads',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './loads.component.html',
  styleUrls: ['./loads.component.scss']
})
export class LoadsComponent {
  private http = inject(HttpClient);
  apiBase = '/reports';

  // filters
  from?: string;
  to?: string;
  search = '';
  page = 1;
  pageSize = 10;

  // data
  items = signal<any[]>([]);
  totalCount = signal(0);
  totalRevenue = signal(0);
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
    this.http.get<any>(`${this.apiBase}/loads`, { params }).subscribe(res => {
      if (res.success) {
        this.items.set(res.data.items);
        this.totalCount.set(res.data.totalCount);
        this.totalRevenue.set(res.data.totalRevenue);
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
    this.http.get(`${this.apiBase}/loads/export`, { params, responseType: 'blob', observe: 'response' }).subscribe(resp => {
      const contentDisposition = resp.headers.get('content-disposition') || '';
      const match = /filename="?([^";]+)"?/.exec(contentDisposition);
      const filename = match ? match[1] : `loads-report.${format}`;
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

