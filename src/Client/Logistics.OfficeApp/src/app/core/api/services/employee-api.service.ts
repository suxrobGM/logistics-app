import { Observable } from "rxjs";
import { ApiBase } from "../api-base";
import {
  CreateEmployeeCommand,
  EmployeeDto,
  PagedResult,
  RemoveEmployeeRoleCommand,
  SearchableQuery,
  UpdateEmployeeCommand,
} from "../models";
import { Result } from "../models";

export class EmployeeApiService extends ApiBase {
  getEmployee(userId: string): Observable<Result<EmployeeDto>> {
    const url = `/employees/${userId}`;
    return this.get(url);
  }

  getEmployees(query?: SearchableQuery): Observable<PagedResult<EmployeeDto>> {
    const url = `/employees?${this.stringfySearchableQuery(query)}`;
    return this.get(url);
  }

  getDrivers(query?: SearchableQuery): Observable<PagedResult<EmployeeDto>> {
    const url = `/employees?${this.stringfySearchableQuery(query)}&role=tenant.driver`;
    return this.get(url);
  }

  createEmployee(command: CreateEmployeeCommand): Observable<Result> {
    const url = `/employees`;
    return this.post(url, command);
  }

  removeRoleFromEmployee(command: RemoveEmployeeRoleCommand): Observable<Result> {
    const url = `/employees/${command.userId}/remove-role`;
    return this.post(url, command);
  }

  updateEmployee(command: UpdateEmployeeCommand): Observable<Result> {
    const url = `/employees/${command.userId}`;
    return this.put(url, command);
  }

  deleteEmployee(employeeId: string): Observable<Result> {
    const url = `/employees/${employeeId}`;
    return this.delete(url);
  }
}
