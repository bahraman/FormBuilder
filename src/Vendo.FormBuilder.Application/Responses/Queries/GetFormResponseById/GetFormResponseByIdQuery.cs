using Vendo.FormBuilder.Application.Common.Forms;
using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Responses.Dtos;
using Vendo.FormBuilder.Domain.Exceptions;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Responses.Queries.GetFormResponseById;

public sealed record GetFormResponseByIdQuery(
    Guid ResponseId,
    Guid SubscriberId,
    Guid? RestaurantId = null) : IRequest<FormResponseDto>;

public sealed class GetFormResponseByIdQueryHandler : IRequestHandler<GetFormResponseByIdQuery, FormResponseDto>
{
    private readonly IFormRepository _formRepository;
    private readonly IFormResponseRepository _responseRepository;

    public GetFormResponseByIdQueryHandler(
        IFormRepository formRepository,
        IFormResponseRepository responseRepository)
    {
        _formRepository = formRepository;
        _responseRepository = responseRepository;
    }

    public async Task<FormResponseDto> Handle(GetFormResponseByIdQuery request, CancellationToken cancellationToken)
    {
        var response = await _responseRepository.GetByIdAsync(request.ResponseId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.FormResponse), request.ResponseId);

        // Ensure the parent form is visible within the caller's tenant scope.
        _ = await FormAccess.GetAccessibleFormAsync(
            _formRepository,
            response.FormId,
            request.SubscriberId,
            request.RestaurantId,
            withDetails: false,
            cancellationToken);

        return response.ToDto();
    }
}
