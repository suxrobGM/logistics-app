using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using MockQueryable;
using NSubstitute;

namespace Logistics.Application.Tests.TestKit;

/// <summary>
/// Every handler that maps a load runs <c>LoadLookupResolver</c>, which reads these repositories.
/// Stubbing them empty lets a test set up only the ones it asserts on - a later
/// <c>Repository&lt;T&gt;().Returns(...)</c> replaces the stub.
/// </summary>
internal static class LoadLookupStubs
{
    public static void StubLoadLookups(this ITenantUnitOfWork uow)
    {
        Empty<Container>(uow);
        Empty<Terminal>(uow);
        Empty<Truck>(uow);
        Empty<Employee>(uow);
        Empty<TripStop>(uow);
        Empty<LoadInvoice>(uow);
        Empty<Customer>(uow);
    }

    private static void Empty<T>(ITenantUnitOfWork uow) where T : class, IEntity<Guid>, ITenantEntity
    {
        var repo = Substitute.For<ITenantRepository<T, Guid>>();
        repo.Query().Returns(Array.Empty<T>().BuildMock());
        uow.Repository<T>().Returns(repo);
    }
}
