import {PagedQuery} from './PagedQuery_';

export interface PagedIntervalQuery extends PagedQuery {
  startDate: Date;
  endDate?: Date;
}
