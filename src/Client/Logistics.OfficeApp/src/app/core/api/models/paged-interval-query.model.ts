import {PagedQuery} from "./paged-query.model";

export interface PagedIntervalQuery extends PagedQuery {
  startDate: Date;
  endDate?: Date;
}
