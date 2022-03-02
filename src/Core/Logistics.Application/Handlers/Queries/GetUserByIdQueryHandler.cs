﻿namespace Logistics.Application.Handlers.Queries;

internal sealed class GetUserByIdQueryHandler : RequestHandlerBase<GetUserByIdQuery, DataResult<UserDto>>
{
    private readonly IRepository<User> userRepository;

    public GetUserByIdQueryHandler(
        IRepository<User> userRepository)
    {
        this.userRepository = userRepository;
    }

    protected override async Task<DataResult<UserDto>> HandleValidated(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var userEntity = await userRepository.GetAsync(request.Id!) ??
                        await userRepository.GetAsync(i => i.ExternalId == request.Id);

        if (userEntity == null)
        {
            return DataResult<UserDto>.CreateError("Could not find the specified user");
        }

        var user = new UserDto
        {
            Id = userEntity.Id,
            ExternalId = userEntity.ExternalId,
            UserName = userEntity.UserName,
            FirstName = userEntity.FirstName,
            LastName = userEntity.LastName,
            Email = userEntity.Email,
            PhoneNumber = userEntity.PhoneNumber
        };

        return DataResult<UserDto>.CreateSuccess(user);
    }

    protected override bool Validate(GetUserByIdQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}