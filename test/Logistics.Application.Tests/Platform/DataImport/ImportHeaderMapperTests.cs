using Logistics.Application.Modules.Platform.DataImport.Constants;
using Logistics.Application.Modules.Platform.DataImport.Services;
using Logistics.Domain.Primitives.Enums;
using Xunit;

namespace Logistics.Application.Tests.Platform.DataImport;

public class ImportHeaderMapperTests
{
    [Theory]
    [InlineData("Unit #")]
    [InlineData("unit_number")]
    [InlineData("TRUCK NUMBER")]
    [InlineData("Truck #")]
    [InlineData("﻿Unit Number")]
    public void Suggest_TruckNumberSpellings_MapToNumber(string header)
    {
        var mapping = ImportHeaderMapper.Suggest([header], ImportEntityType.Trucks, null);

        Assert.Equal(ImportFieldKeys.Number, mapping[0].FieldKey);
    }

    [Theory]
    [InlineData("Load #")]
    [InlineData("Pro #")]
    [InlineData("Reference")]
    public void Suggest_LoadReferenceSpellings_MapToReference(string header)
    {
        var mapping = ImportHeaderMapper.Suggest([header], ImportEntityType.Loads, null);

        Assert.Equal(ImportFieldKeys.Reference, mapping[0].FieldKey);
    }

    [Fact]
    public void Suggest_McNumber_MapsForCustomers()
    {
        var mapping = ImportHeaderMapper.Suggest(["Company Name", "MC#", "Zip Code"], ImportEntityType.Customers, null);

        Assert.Equal([ImportFieldKeys.Name, ImportFieldKeys.McNumber, ImportFieldKeys.Zip],
            mapping.Select(m => m.FieldKey));
    }

    [Fact]
    public void Suggest_PrefixedAddressHeaders_MapToPickupAndDelivery()
    {
        var mapping = ImportHeaderMapper.Suggest(
            ["Origin City", "Shipper Zip", "Consignee State", "Delivery Address"], ImportEntityType.Loads, null);

        Assert.Equal(["pickupCity", "pickupZip", "deliveryState", "deliveryStreet"], mapping.Select(m => m.FieldKey));
    }

    [Fact]
    public void Suggest_SameFieldTwice_OnlyLeftmostColumnMaps()
    {
        var mapping = ImportHeaderMapper.Suggest(["Unit #", "Truck Number"], ImportEntityType.Trucks, null);

        Assert.Equal(ImportFieldKeys.Number, mapping[0].FieldKey);
        Assert.Null(mapping[1].FieldKey);
    }

    [Fact]
    public void Suggest_UnknownHeader_StaysUnmapped()
    {
        var mapping = ImportHeaderMapper.Suggest(["Favourite colour"], ImportEntityType.Trucks, null);

        Assert.Null(mapping[0].FieldKey);
    }

    [Fact]
    public void Suggest_DatatruckPreset_MapsSnakeCaseExport()
    {
        var mapping = ImportHeaderMapper.Suggest(
            ["unit_number", "trailer_type", "plate_number", "plate_state"], ImportEntityType.Trailers,
            ImportPresets.Datatruck);

        Assert.Equal(
            [ImportFieldKeys.Number, ImportFieldKeys.Type, ImportFieldKeys.LicensePlate, ImportFieldKeys.LicensePlateState],
            mapping.Select(m => m.FieldKey));
    }

    [Theory]
    [InlineData(ImportEntityType.Customers)]
    [InlineData(ImportEntityType.Drivers)]
    [InlineData(ImportEntityType.Trucks)]
    [InlineData(ImportEntityType.Trailers)]
    [InlineData(ImportEntityType.Loads)]
    public void BuildTemplate_HeadersMapBackToEveryField(ImportEntityType entityType)
    {
        var headers = ImportRowMapper.BuildTemplate(entityType).TrimEnd().Split(',');

        var mapping = ImportHeaderMapper.Suggest(headers, entityType, null);

        Assert.Equal(ImportFieldCatalog.For(entityType).Select(f => f.Key), mapping.Select(m => m.FieldKey));
    }
}
