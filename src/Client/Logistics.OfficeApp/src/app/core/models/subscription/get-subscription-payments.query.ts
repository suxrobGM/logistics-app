import {PagedQuery} from "../paged-query";

export interface GetSubscriptionPaymentsQuery extends PagedQuery {
  subscriptionId?: string;
}
