import {SearchableQuery} from '../searchableQuery';

export interface GetPayrollsQuery extends SearchableQuery {
  employeeId?: string;
}
