import { inject, Injectable } from "@angular/core";
import {
  Api,
  impersonateUser,
  type ImpersonateUserCommand,
  type ImpersonateUserResult,
} from "@logistics/shared/api";

@Injectable({ providedIn: "root" })
export class ImpersonationService {
  private readonly api = inject(Api);

  async impersonate(request: ImpersonateUserCommand): Promise<ImpersonateUserResult> {
    return this.api.invoke(impersonateUser, { body: request });
  }
}
