import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AppConfig } from '@app/configs/app.config';
import { Load } from '@app/shared/models/load';
import { Observable } from 'rxjs';

@Injectable()
export class LoadService {

  constructor(private httpClient: HttpClient) { }

  getLoads(searchQuery = '', page = 1, pageSize = 10): Observable<any> {  
    const query = `${AppConfig.apiHost}/load/list?search=${searchQuery}page=${page}&pageSize=${pageSize}`
    return this.httpClient.get(query).pipe((i) => {
      console.log(i);
      return i;
    });
  }
}
