namespace Logistics.Application.Admin.Commands;

internal sealed class UpdateUserHandler : RequestHandlerBase<UpdateUserCommand, ResponseResult>
{
    private readonly IMainRepository _mainRepository;

    public UpdateUserHandler(IMainRepository mainRepository)
    {
        _mainRepository = mainRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _mainRepository.GetAsync<User>(request.Id);

        if (user == null)
            return ResponseResult.CreateError("Could not find the specified user");

        if (!string.IsNullOrEmpty(request.FirstName))
            user.FirstName = request.FirstName;

        if (!string.IsNullOrEmpty(request.LastName))
            user.LastName = request.LastName;

        if (!string.IsNullOrEmpty(request.PhoneNumber))
            user.PhoneNumber = request.PhoneNumber;

        _mainRepository.Update(user);
        await _mainRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
