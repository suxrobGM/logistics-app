namespace Logistics.Application.Admin.Commands;

internal sealed class UpdateUserHandler : RequestHandler<UpdateUserCommand, ResponseResult>
{
    private readonly IMainRepository _mainRepository;

    public UpdateUserHandler(IMainRepository mainRepository)
    {
        _mainRepository = mainRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        UpdateUserCommand req, CancellationToken cancellationToken)
    {
        var user = await _mainRepository.GetAsync<User>(req.Id);

        if (user == null)
            return ResponseResult.CreateError("Could not find the specified user");

        if (!string.IsNullOrEmpty(req.FirstName))
            user.FirstName = req.FirstName;

        if (!string.IsNullOrEmpty(req.LastName))
            user.LastName = req.LastName;

        if (!string.IsNullOrEmpty(req.PhoneNumber))
            user.PhoneNumber = req.PhoneNumber;

        _mainRepository.Update(user);
        await _mainRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
