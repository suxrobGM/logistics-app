import { PagedIntervalQuery } from "../paged-interval.query";

export interface GetPaymentsQuery extends PagedIntervalQuery {
  /**
   * Get only subscription payments
   */
  subscriptionId?: string;
}
