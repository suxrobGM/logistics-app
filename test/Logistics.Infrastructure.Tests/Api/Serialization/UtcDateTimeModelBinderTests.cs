using Logistics.API.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Logistics.Infrastructure.Tests.Api.Serialization;

public class UtcDateTimeModelBinderTests
{
    private readonly UtcDateTimeModelBinder _sut = new();

    [Fact]
    public async Task BindModelAsync_DateWithOffset_BindsUtc()
    {
        var context = CreateContext<DateTime?>("2026-03-01T05:00:00-05:00");

        await _sut.BindModelAsync(context);

        var bound = Assert.IsType<DateTime>(context.Result.Model);
        Assert.Equal(new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc), bound);
        Assert.Equal(DateTimeKind.Utc, bound.Kind);
    }

    [Fact]
    public async Task BindModelAsync_EmptyNullable_BindsNull()
    {
        var context = CreateContext<DateTime?>("");

        await _sut.BindModelAsync(context);

        Assert.True(context.Result.IsModelSet);
        Assert.Null(context.Result.Model);
    }

    [Fact]
    public async Task BindModelAsync_NotADate_AddsModelError()
    {
        var context = CreateContext<DateTime?>("soon");

        await _sut.BindModelAsync(context);

        Assert.False(context.Result.IsModelSet);
        Assert.False(context.ModelState.IsValid);
    }

    [Fact]
    public void GetBinder_DateTimeAndNullable_ReturnsBinder_OtherTypes_ReturnNull()
    {
        var provider = new UtcDateTimeModelBinderProvider();

        Assert.NotNull(provider.GetBinder(new TestProviderContext(typeof(DateTime))));
        Assert.NotNull(provider.GetBinder(new TestProviderContext(typeof(DateTime?))));
        Assert.Null(provider.GetBinder(new TestProviderContext(typeof(DateOnly))));
    }

    private static DefaultModelBindingContext CreateContext<T>(string value)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = QueryString.Create("fromDate", value);

        return new DefaultModelBindingContext
        {
            ModelName = "fromDate",
            ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(T)),
            ModelState = new ModelStateDictionary(),
            ValueProvider = new QueryStringValueProvider(BindingSource.Query, httpContext.Request.Query, null)
        };
    }

    private sealed class TestProviderContext(Type modelType) : ModelBinderProviderContext
    {
        public override ModelMetadata Metadata { get; } = new EmptyModelMetadataProvider().GetMetadataForType(modelType);
        public override IModelMetadataProvider MetadataProvider { get; } = new EmptyModelMetadataProvider();
        public override BindingInfo BindingInfo { get; } = new();
        public override IModelBinder CreateBinder(ModelMetadata metadata) => throw new NotSupportedException();
    }
}
