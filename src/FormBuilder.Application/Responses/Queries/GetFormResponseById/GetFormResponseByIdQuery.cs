using FormBuilder.Application.Common.Mappings;
using FormBuilder.Application.Responses.Dtos;
using FormBuilder.Domain.Exceptions;
using FormBuilder.Domain.Interfaces;
using MediatR;

namespace FormBuilder.Application.Responses.Queries.GetFormResponseById;

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
