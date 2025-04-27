import {HttpClient} from "@angular/common/http";
import {Observable} from "rxjs";
import {ApiBase} from "./api-base";
import {CancelSubscriptionCommand, Result, SubscriptionPlanDto} from "./models";

export class SubscriptionApi extends ApiBase {
  constructor(apiUrl: string, http: HttpClient) {
    super(apiUrl, http);
  }

  getSubscriptionPlans(): Observable<Result<SubscriptionPlanDto[]>> {
    return this.get("/subscriptions/plans");
  }

  cancelSubscription(command: CancelSubscriptionCommand): Observable<Result> {
    return this.put(`/subscriptions/${command.id}/cancel`, command);
  }
}
