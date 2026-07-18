using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Responses.Dtos;
using Vendo.FormBuilder.Domain.Exceptions;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Responses.Queries.GetFormResponseById;

public sealed record GetFormResponseByIdQuery(Guid ResponseId) : IRequest<FormResponseDto>;

public sealed class GetFormResponseByIdQueryHandler : IRequestHandler<GetFormResponseByIdQuery, FormResponseDto>
{
    private readonly IFormResponseRepository _responseRepository;

    public GetFormResponseByIdQueryHandler(IFormResponseRepository responseRepository)
    {
        _responseRepository = responseRepository;
    }

    public async Task<FormResponseDto> Handle(GetFormResponseByIdQuery request, CancellationToken cancellationToken)
    {
        var response = await _responseRepository.GetByIdAsync(request.ResponseId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.FormResponse), request.ResponseId);

        return response.ToDto();
    }
}
