import {PagedIntervalQuery} from "../paged-interval-query.model";

export interface GetPaymentsQuery extends PagedIntervalQuery {
  /**
   * Get only subscription payments
   */
  subscriptionId?: string;
}
