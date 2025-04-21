import {SearchableQuery} from "../searchable-query.model";

export interface GetPayrollsQuery extends SearchableQuery {
  employeeId?: string;
}
