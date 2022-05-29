using System.Text.RegularExpressions;

namespace Logistics.Application.Handlers.Commands;

internal sealed class UpdateTenantCommandHandler : RequestHandlerBase<UpdateTenantCommand, DataResult>
{
    private readonly IMainRepository<Tenant> _repository;

    public UpdateTenantCommandHandler(IMainRepository<Tenant> repository)
    {
        _repository = repository;
    }

    protected override async Task<DataResult> HandleValidated(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetAsync(request.Id!);

        if (tenant == null)
        {
            return DataResult.CreateError("Could not find the tenant");
        }

        tenant.Name = request.Name?.Trim()?.ToLower();

        if (string.IsNullOrEmpty(request.DisplayName))
        {
            tenant.DisplayName = tenant.Name;
        }
        else
        {
            tenant.DisplayName = request.DisplayName?.Trim();
        }
        
        _repository.Update(tenant);
        await _repository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(UpdateTenantCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;
        var regex = new Regex(@"^[a-z]+\d*", RegexOptions.IgnoreCase);

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }
        else if (string.IsNullOrEmpty(request.Name))
        {
            errorDescription = "Tenant name is not specified";
        }
        else if (request.Name.Length < 4)
        {
            errorDescription = "The length of the tenant name must have at least 4 characters";
        }
        else if (!regex.IsMatch(request.Name))
        {
            errorDescription = "The format of the tenant name must start with English letters and may end with numbers";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
