using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Common.Models;
using Vendo.FormBuilder.Application.Forms.Dtos;
using Vendo.FormBuilder.Domain.Enums;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Forms.Queries.GetForms;

public sealed record GetFormsQuery(
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
        var pagination = new PaginationQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var (items, totalCount) = await _formRepository.GetPagedAsync(
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
