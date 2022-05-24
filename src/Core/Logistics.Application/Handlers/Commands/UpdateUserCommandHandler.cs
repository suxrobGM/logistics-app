namespace Logistics.Application.Handlers.Commands;

internal sealed class UpdateUserCommandHandler : RequestHandlerBase<UpdateUserCommand, DataResult>
{
    private readonly ITenantRepository<User> userRepository;

    public UpdateUserCommandHandler(
        ITenantRepository<User> userRepository)
    {
        this.userRepository = userRepository;
    }

    protected override async Task<DataResult> HandleValidated(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var userEntity = await userRepository.GetAsync(request.Id!);

        if (userEntity == null)
        {
            return DataResult.CreateError("Could not find the specified user");
        }

        userEntity.FirstName = request.FirstName;
        userEntity.LastName = request.LastName;
        userEntity.UserName = request.UserName;
        userEntity.Email = request.Email;
        userEntity.PhoneNumber = request.PhoneNumber;

        userRepository.Update(userEntity);
        await userRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(UpdateUserCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }
        else if (string.IsNullOrEmpty(request.FirstName))
        {
            errorDescription = "First name is an empty string";
        }
        else if (string.IsNullOrEmpty(request.LastName))
        {
            errorDescription = "Last name is an empty string";
        }
        else if (string.IsNullOrEmpty(request.Email))
        {
            errorDescription = "Email is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
