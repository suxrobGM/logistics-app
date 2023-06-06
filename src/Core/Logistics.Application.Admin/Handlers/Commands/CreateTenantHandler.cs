using System.Text.RegularExpressions;

namespace Logistics.Application.Admin.Handlers.Commands;

internal sealed class CreateTenantHandler : RequestHandlerBase<CreateTenantCommand, ResponseResult>
{
    private readonly ITenantDatabaseService _tenantDatabase;
    private readonly IMainRepository _repository;

    public CreateTenantHandler(
        ITenantDatabaseService tenantDatabase,
        IMainRepository repository)
    {
        _tenantDatabase = tenantDatabase;
        _repository = repository;
    }

    protected override async Task<ResponseResult> HandleValidated(CreateTenantCommand req, CancellationToken cancellationToken)
    {
        var tenant = new Tenant
        {
            Name = req.Name?.Trim().ToLower(),
            DisplayName = req.DisplayName
        };
        tenant.Name = tenant.Name?.Trim().ToLower();
        tenant.ConnectionString = _tenantDatabase.GenerateConnectionString($"{tenant.Name}_logistics"); 

        if (string.IsNullOrEmpty(tenant.DisplayName))
        {
            tenant.DisplayName = tenant.Name;
        }

        var existingTenant = await _repository.GetAsync<Tenant>(i => i.Name == tenant.Name);
        if (existingTenant != null)
        {
            return ResponseResult.CreateError($"Tenant name '{tenant.Name}' is already taken, please chose another name");
        }

        var created = await _tenantDatabase.CreateDatabaseAsync(tenant.ConnectionString);
        if (!created)
        {
            return ResponseResult.CreateError("Could not create the tenant's database");
        }

        await _repository.AddAsync(tenant);
        await _repository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }

    protected override bool Validate(CreateTenantCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;
        var regex = new Regex(@"^[a-z]+\d*", RegexOptions.IgnoreCase);

        if (string.IsNullOrEmpty(request.Name))
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
