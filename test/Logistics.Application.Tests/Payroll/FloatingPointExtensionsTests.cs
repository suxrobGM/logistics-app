using Logistics.Domain.Primitives;
using Xunit;

namespace Logistics.Application.Tests.Payroll;

public class FloatingPointExtensionsTests
{
    [Fact]
    public void ToDecimal_Double_DropsBinaryNoise()
    {
        Assert.Equal(0.3m, (0.1 + 0.2).ToDecimal());
        Assert.Equal(8.1m, 8.1.ToDecimal());
    }

    [Fact]
    public void ToDecimal_Float_DropsBinaryNoise()
    {
        Assert.Equal(0.3m, 0.3f.ToDecimal());
    }

    [Fact]
    public void ToDecimal_ShareOfGross_KeepsMidpointRoundingStable()
    {
        var share = 1000m * 0.3f.ToDecimal();

        Assert.Equal(300m, share);
    }

    [Fact]
    public void ToDecimal_TinyValue_ParsesExponentForm()
    {
        Assert.Equal(0.00001m, 0.00001.ToDecimal());
    }
}
