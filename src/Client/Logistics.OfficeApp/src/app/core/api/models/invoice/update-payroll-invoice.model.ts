export interface UpdatePayrollInvoiceCommand {
  id: string;
  employeeId: string;
  periodStart: Date;
  periodEnd: Date;
}
