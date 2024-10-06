export interface UpdatePayrollCommand {
  id: string;
  startDate?: Date;
  endDate?: Date;
  employeeId?: string;
}
