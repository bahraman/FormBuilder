using FormBuilder.Application.Common.Mappings;
using FormBuilder.Application.Common.Models;
using FormBuilder.Application.Responses.Dtos;
using FormBuilder.Domain.Exceptions;
using FormBuilder.Domain.Interfaces;
using MediatR;

namespace FormBuilder.Application.Responses.Queries.GetFormResponses;

public sealed record GetFormResponsesQuery(
    Guid FormId,
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
        _ = await _formRepository.GetByIdAsync(request.FormId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Form), request.FormId);

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
