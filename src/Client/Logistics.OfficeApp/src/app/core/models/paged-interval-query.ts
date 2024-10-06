import {PagedQuery} from "./paged-query";

export interface PagedIntervalQuery extends PagedQuery {
  startDate: Date;
  endDate?: Date;
}
