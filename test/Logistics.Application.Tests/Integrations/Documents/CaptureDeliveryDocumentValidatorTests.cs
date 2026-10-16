using FluentValidation.TestHelper;
using Logistics.Application.Modules.Integrations.Documents.Commands;
using Xunit;

namespace Logistics.Application.Tests.Integrations.Documents;

public class CaptureDeliveryDocumentValidatorTests
{
    [Theory]
    [InlineData(500, true)]
    [InlineData(501, false)]
    public void CaptureProofOfDelivery_UnableToSignReasonLength_ValidatesMaxLength(int length, bool valid)
    {
        var command = new CaptureProofOfDeliveryCommand
        {
            LoadId = Guid.NewGuid(),
            Photos = [],
            UnableToSignReason = new string('x', length)
        };

        var result = new CaptureProofOfDeliveryValidator().TestValidate(command);

        Assert.Equal(valid, !result.Errors.Any(e => e.PropertyName == nameof(command.UnableToSignReason)));
    }

    [Theory]
    [InlineData(500, true)]
    [InlineData(501, false)]
    public void CaptureBillOfLading_UnableToSignReasonLength_ValidatesMaxLength(int length, bool valid)
    {
        var command = new CaptureBillOfLadingCommand
        {
            LoadId = Guid.NewGuid(),
            Photos = [],
            UnableToSignReason = new string('x', length)
        };

        var result = new CaptureBillOfLadingValidator().TestValidate(command);

        Assert.Equal(valid, !result.Errors.Any(e => e.PropertyName == nameof(command.UnableToSignReason)));
    }
}
