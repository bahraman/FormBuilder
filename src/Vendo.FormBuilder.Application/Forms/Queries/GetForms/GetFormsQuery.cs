using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Common.Models;
using Vendo.FormBuilder.Application.Forms.Dtos;
using Vendo.FormBuilder.Domain.Common;
using Vendo.FormBuilder.Domain.Enums;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Forms.Queries.GetForms;

public sealed record GetFormsQuery(
    Guid SubscriberId,
    Guid? RestaurantId = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? Search = null,
    FormStatus? Status = null) : IRequest<PagedResult<FormSummaryDto>>;

public sealed class GetFormsQueryHandler : IRequestHandler<GetFormsQuery, PagedResult<FormSummaryDto>>
{
    private readonly IFormRepository _formRepository;

    public GetFormsQueryHandler(IFormRepository formRepository)
    {
        _formRepository = formRepository;
    }

    public async Task<PagedResult<FormSummaryDto>> Handle(GetFormsQuery request, CancellationToken cancellationToken)
    {
        var tenant = TenantScope.ForSubscriber(request.SubscriberId, request.RestaurantId);

        var pagination = new PaginationQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var (items, totalCount) = await _formRepository.GetPagedAsync(
            tenant.SubscriberId,
            tenant.RestaurantId,
            pagination.PageNumber,
            pagination.PageSize,
            request.Search,
            request.Status,
            cancellationToken);

        return new PagedResult<FormSummaryDto>
        {
            Items = items.Select(f => f.ToSummaryDto()).ToList(),
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }
}
