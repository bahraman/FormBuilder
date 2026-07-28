using FluentValidation;

namespace Vendo.FormBuilder.Application.Locations.Queries.GetCitiesByProvince;

public sealed class GetCitiesByProvinceQueryValidator : AbstractValidator<GetCitiesByProvinceQuery>
{
    public GetCitiesByProvinceQueryValidator()
    {
        RuleFor(x => x.ProvinceId)
            .GreaterThan(0)
            .WithMessage("ProvinceId must be a positive integer.");
    }
}
