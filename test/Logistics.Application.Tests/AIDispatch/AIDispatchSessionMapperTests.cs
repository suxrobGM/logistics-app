using Logistics.Domain.Entities;
using Logistics.Mappings;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class AIDispatchSessionMapperTests
{
    [Fact]
    public void ToDto_StartedAtIsTheSessionCreationTime()
    {
        var session = new AgentSession { CreatedAt = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc) };

        Assert.Equal(session.CreatedAt, session.ToDto().StartedAt);
    }
}
