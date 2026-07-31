using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Vendo.FormBuilder.Application.Locations.Queries.GetCitiesByProvince;
using Vendo.FormBuilder.Application.Locations.Queries.GetProvinces;
using Vendo.FormBuilder.Domain.Exceptions;
using Vendo.FormBuilder.Infrastructure.Persistence;
using Vendo.FormBuilder.Infrastructure.Persistence.Repositories;

namespace Vendo.FormBuilder.UnitTests.Infrastructure;

public sealed class LocationLookupTests
{
    [Fact]
    public void Seeded_provinces_have_unique_ids_names_and_order()
    {
        using var db = CreateSeededContext();

        var provinces = db.Provinces.ToList();

        provinces.Should().NotBeEmpty();
        provinces.Select(p => p.Id).Should().OnlyHaveUniqueItems();
        provinces.Select(p => p.Name).Should().OnlyHaveUniqueItems();
        provinces.Should().OnlyContain(p => p.Id > 0 && p.OrderIndex > 0);
        provinces.Should().OnlyContain(p => !string.IsNullOrWhiteSpace(p.Name));
    }

    [Fact]
    public void Seeded_cities_have_unique_ids_and_belong_to_a_known_province()
    {
        using var db = CreateSeededContext();

        var provinceIds = db.Provinces.Select(p => p.Id).ToHashSet();
        var cities = db.Cities.ToList();

        cities.Should().NotBeEmpty();
        cities.Select(c => c.Id).Should().OnlyHaveUniqueItems();
        cities.Should().OnlyContain(c => provinceIds.Contains(c.ProvinceId));
        cities.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.Name));
    }

    [Fact]
    public void Seeded_city_names_are_unique_within_their_province()
    {
        using var db = CreateSeededContext();

        var duplicates = db.Cities
            .ToList()
            .GroupBy(c => new { c.ProvinceId, c.Name })
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key.ProvinceId}:{group.Key.Name}")
            .ToList();

        duplicates.Should().BeEmpty("the unique index on (ProvinceId, Name) would reject duplicates");
    }

    [Fact]
    public async Task GetProvinces_returns_every_province_ordered_by_order_index()
    {
        using var db = CreateSeededContext();
        var handler = new GetProvincesQueryHandler(new LocationRepository(db));

        var result = await handler.Handle(new GetProvincesQuery(), CancellationToken.None);

        result.Should().HaveCount(db.Provinces.Count());
        result.Select(p => p.OrderIndex).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetCities_returns_only_the_requested_province_ordered_by_order_index()
    {
        using var db = CreateSeededContext();
        var handler = new GetCitiesByProvinceQueryHandler(new LocationRepository(db));
        var provinceId = db.Provinces.OrderBy(p => p.OrderIndex).First().Id;

        var result = await handler.Handle(new GetCitiesByProvinceQuery(provinceId), CancellationToken.None);

        result.Should().NotBeEmpty();
        result.Should().OnlyContain(c => c.ProvinceId == provinceId);
        result.Select(c => c.OrderIndex).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetCities_throws_when_the_province_does_not_exist()
    {
        using var db = CreateSeededContext();
        var handler = new GetCitiesByProvinceQueryHandler(new LocationRepository(db));

        var act = () => handler.Handle(new GetCitiesByProvinceQuery(999_999), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static ApplicationDbContext CreateSeededContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
