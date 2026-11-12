using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using MockQueryable;
using NSubstitute;

namespace Logistics.Application.Tests.TestKit;

internal static class UnitOfWorkStubs
{
    extension(ITenantUnitOfWork uow)
    {
        /// <summary>
        /// Wires <c>Repository&lt;T&gt;()</c> to a substitute whose <c>Query()</c> reads
        /// <paramref name="rows"/> on every call, so rows added to a list later are seen. Returns the
        /// repository for further setup or <c>Received</c> checks.
        /// </summary>
        public ITenantRepository<T, Guid> Stub<T>(params ICollection<T> rows)
            where T : class, IEntity<Guid>, ITenantEntity
        {
            var repo = Substitute.For<ITenantRepository<T, Guid>>();
            repo.Query().Returns(_ => rows.BuildMock());
            uow.Repository<T>().Returns(repo);
            return repo;
        }

        /// <summary>
        /// Every handler that maps a load runs <c>LoadLookupBuilder</c>, which reads these
        /// repositories. Stubbing them empty lets a test set up only the ones it asserts on - a
        /// later <c>Repository&lt;T&gt;().Returns(...)</c> replaces the stub.
        /// </summary>
        public void StubLoadLookups()
        {
            uow.Stub<Container>();
            uow.Stub<Terminal>();
            uow.Stub<Truck>();
            uow.Stub<Employee>();
            uow.Stub<TripStop>();
            uow.Stub<LoadInvoice>();
            uow.Stub<Customer>();
            uow.Stub<LoadAccessorial>();
        }
    }
}
