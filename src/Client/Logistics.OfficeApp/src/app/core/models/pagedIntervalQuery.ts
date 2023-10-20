import {PagedQuery} from './pagedQuery';

export interface PagedIntervalQuery extends PagedQuery {
  startDate: Date;
  endDate?: Date;
}
