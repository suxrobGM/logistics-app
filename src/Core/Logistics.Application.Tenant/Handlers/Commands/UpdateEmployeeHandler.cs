namespace Logistics.Application.Handlers.Commands;

internal sealed class UpdateEmployeeHandler : RequestHandlerBase<UpdateEmployeeCommand, DataResult>
{
    private readonly ITenantRepository<Employee> _userRepository;

    public UpdateEmployeeHandler(
        ITenantRepository<Employee> userRepository)
    {
        _userRepository = userRepository;
    }

    protected override async Task<DataResult> HandleValidated(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var userEntity = await _userRepository.GetAsync(request.Id!);

        if (userEntity == null)
        {
            return DataResult.CreateError("Could not find the specified user");
        }

        //userEntity.FirstName = request.FirstName;
        //userEntity.LastName = request.LastName;
        //userEntity.UserName = request.UserName;
        userEntity.Role = EmployeeRole.Get(request.Role!);

        _userRepository.Update(userEntity);
        await _userRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(UpdateEmployeeCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
