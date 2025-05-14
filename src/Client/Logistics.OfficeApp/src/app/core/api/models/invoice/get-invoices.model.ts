import {PagedQuery} from "../paged-query.model";

export interface GetInvoicesQuery extends PagedQuery {
  /**
   * Filter payrolls by Employee ID
   */
  employeeId?: string;

  /**
   * Filter payrolls by Employee Name
   */
  employeeName?: string;

  /**
   * Filter payrolls by Load ID
   */
  loadId?: string;
}
