import {SearchableQuery} from '../SearchableQuery';

export interface GetPayrollsQuery extends SearchableQuery {
  employeeId?: string;
}
