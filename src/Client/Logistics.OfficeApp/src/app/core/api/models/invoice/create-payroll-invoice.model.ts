export interface CreatePayrollInvoiceCommand {
  employeeId: string;
  periodStart: Date;
  periodEnd: Date;
}
