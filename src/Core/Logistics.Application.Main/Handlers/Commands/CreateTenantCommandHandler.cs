using System.Text.RegularExpressions;

namespace Logistics.Application.Handlers.Commands;

internal sealed class CreateTenantCommandHandler : RequestHandlerBase<CreateTenantCommand, DataResult>
{
    private readonly IDatabaseProviderService _databaseProvider;
    private readonly IMapper _mapper;
    private readonly IMainRepository<Tenant> _repository;

    public CreateTenantCommandHandler(
        IDatabaseProviderService databaseProvider,
        IMapper mapper,
        IMainRepository<Tenant> repository)
    {
        _databaseProvider = databaseProvider;
        _mapper = mapper;
        _repository = repository;
    }

    protected override async Task<DataResult> HandleValidated(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = _mapper.Map<Tenant>(request);
        tenant.Name = tenant.Name?.Trim()?.ToLower();
        tenant.ConnectionString = _databaseProvider.GenerateConnectionString(tenant.Name!);

        if (string.IsNullOrEmpty(tenant.DisplayName))
        {
            tenant.DisplayName = tenant.Name;
        }

        var existingTenant = await _repository.GetAsync(i => i.Name == tenant.Name);
        if (existingTenant != null)
        {
            return DataResult.CreateError($"Tenant name '{tenant.Name}' is already taken, please chose another name");
        }

        var created = await _databaseProvider.CreateDatabaseAsync(tenant.ConnectionString);
        if (!created)
        {
            return DataResult.CreateError("Could not create the tenant's database");
        }

        await _repository.AddAsync(tenant);
        await _repository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
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
