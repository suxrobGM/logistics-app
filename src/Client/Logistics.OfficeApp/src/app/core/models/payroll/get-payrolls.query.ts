import {SearchableQuery} from "../searchable.query";

export interface GetPayrollsQuery extends SearchableQuery {
  employeeId?: string;
}
