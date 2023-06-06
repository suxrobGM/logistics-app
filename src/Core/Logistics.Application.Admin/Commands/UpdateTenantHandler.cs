using System.Text.RegularExpressions;

namespace Logistics.Application.Admin.Commands;

internal sealed class UpdateTenantHandler : RequestHandlerBase<UpdateTenantCommand, ResponseResult>
{
    private readonly IMainRepository _repository;

    public UpdateTenantHandler(IMainRepository repository)
    {
        _repository = repository;
    }

    protected override async Task<ResponseResult> HandleValidated(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetAsync<Tenant>(request.Id!);

        if (tenant == null)
            return ResponseResult.CreateError("Could not find the tenant");

        tenant.Name = request.Name?.Trim().ToLower();
        tenant.DisplayName = string.IsNullOrEmpty(request.DisplayName) ? tenant.Name : request.DisplayName?.Trim();
        
        _repository.Update(tenant);
        await _repository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
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
