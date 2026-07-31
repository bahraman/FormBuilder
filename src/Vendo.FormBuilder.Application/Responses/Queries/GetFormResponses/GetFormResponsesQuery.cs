using Vendo.FormBuilder.Application.Common.Forms;
using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Common.Models;
using Vendo.FormBuilder.Application.Responses.Dtos;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Responses.Queries.GetFormResponses;

public sealed record GetFormResponsesQuery(
    long FormId,
    int SubscriberId,
    int? RestaurantId = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<FormResponseDto>>;

public sealed class GetFormResponsesQueryHandler : IRequestHandler<GetFormResponsesQuery, PagedResult<FormResponseDto>>
{
    private readonly IFormRepository _formRepository;
    private readonly IFormResponseRepository _responseRepository;

    public GetFormResponsesQueryHandler(
        IFormRepository formRepository,
        IFormResponseRepository responseRepository)
    {
        _formRepository = formRepository;
        _responseRepository = responseRepository;
    }

    public async Task<PagedResult<FormResponseDto>> Handle(
        GetFormResponsesQuery request,
        CancellationToken cancellationToken)
    {
        // Enforce tenant isolation before returning any responses.
        _ = await FormAccess.GetAccessibleFormAsync(
            _formRepository,
            request.FormId,
            request.SubscriberId,
            request.RestaurantId,
            withDetails: false,
            cancellationToken);

        var pagination = new PaginationQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var (items, totalCount) = await _responseRepository.GetByFormIdPagedAsync(
            request.FormId,
            pagination.PageNumber,
            pagination.PageSize,
            cancellationToken);

        return new PagedResult<FormResponseDto>
        {
            Items = items.Select(r => r.ToDto()).ToList(),
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }
}
