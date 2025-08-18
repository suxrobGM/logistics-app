import { PagedIntervalQuery } from "./paged-interval.query";

export interface SearchableIntervalQuery extends PagedIntervalQuery {
  search?: string;
}
