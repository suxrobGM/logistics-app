using Logistics.Shared.Enums;

namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdatePayrollHandler : RequestHandler<UpdatePayrollCommand, ResponseResult>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public UpdatePayrollHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<ResponseResult> HandleValidated(
        UpdatePayrollCommand req, CancellationToken cancellationToken)
    {
        var payroll = await _tenantUow.Repository<Payroll>().GetByIdAsync(req.Id);

        if (payroll is null)
        {
            return ResponseResult.CreateError($"Could not find a payroll with ID '{req.Id}'");
        }
        
        if (!string.IsNullOrEmpty(req.EmployeeId) && req.EmployeeId != payroll.EmployeeId)
        {
            var employee = await _tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId);

            if (employee is null)
            {
                return ResponseResult.CreateError($"Could not find an employer with ID '{req.EmployeeId}'");
            }
            
            payroll.Employee = employee;
        }

        if (req is { StartDate: not null, EndDate: not null } && 
            payroll.StartDate != req.StartDate &&
            payroll.EndDate != req.EndDate)
        {
            payroll.StartDate = req.StartDate.Value;
            payroll.EndDate = req.EndDate.Value;
        }
        
        if (req.PaymentStatus.HasValue && payroll.Payment.Status != req.PaymentStatus)
        {
            payroll.Payment.SetStatus(req.PaymentStatus.Value);
            payroll.Payment.Method = req.PaymentMethod;
            payroll.Payment.BillingAddress = req.PaymentBillingAddress;
        }
        
        _tenantUow.Repository<Payroll>().Update(payroll);
        await _tenantUow.SaveChangesAsync();
        return ResponseResult.CreateSuccess();
    }
}
