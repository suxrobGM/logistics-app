using AutoMapper;
using Logistics.Application.Contracts;
using Logistics.Application.Contracts.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Repositories;
using MediatR;

namespace Logistics.Application.Handlers.Commands;

internal sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, DataResult>
{
    private readonly IMapper mapper;
    private readonly IRepository<User> userRepository;

    public CreateUserCommandHandler(
        IMapper mapper,
        IRepository<User> userRepository)
    {
        this.mapper = mapper;
        this.userRepository = userRepository;
    }

    public async Task<DataResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        await userRepository.AddAsync(mapper.Map<User>(request));
        await userRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }
}
