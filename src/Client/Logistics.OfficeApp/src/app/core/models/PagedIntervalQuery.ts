import {PagedQuery} from './PagedQuery';

export interface PagedIntervalQuery extends PagedQuery {
  startDate: Date;
  endDate?: Date;
}
