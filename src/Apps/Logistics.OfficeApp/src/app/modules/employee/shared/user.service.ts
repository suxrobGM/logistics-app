import { Observable } from 'rxjs';
import { User } from '@shared/models';
import { ApiService } from '@shared/services';

export class UserService {
  constructor(private apiService: ApiService) {}

  // public searchUser(event: any): Observable<User[] | undefined> {
  //   return this.apiService.getUsers(event.query).subscribe(result => {
  //     if (result.success && result.items) {
  //       return result.items;
  //     }
  //   });
  // }
}