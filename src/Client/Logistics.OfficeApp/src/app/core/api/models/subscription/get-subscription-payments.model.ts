import {PagedQuery} from "../paged-query.model";

export interface GetSubscriptionPaymentsQuery extends PagedQuery {
  subscriptionId?: string;
}
