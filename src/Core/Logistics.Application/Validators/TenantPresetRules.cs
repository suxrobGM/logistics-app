using FluentValidation;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Validators;

public static class TenantPresetRules
{
    public static IRuleBuilderOptions<T, List<TenantPreset>> MustBeValidPresets<T>(
        this IRuleBuilder<T, List<TenantPreset>> rule) =>
        rule.Must(p => TenantPresetCatalog.IsValid(p))
            .WithMessage("Select at least one cargo preset: general freight, car hauler or intermodal.");
}
