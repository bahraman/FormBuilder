using FormBuilder.Application.Common.Mappings;
using FormBuilder.Application.Forms.Dtos;
using FormBuilder.Domain.Exceptions;
using FormBuilder.Domain.Interfaces;
using MediatR;

namespace FormBuilder.Application.Forms.Queries.GetFormById;

public sealed record GetFormByIdQuery(Guid FormId) : IRequest<FormDetailDto>;

public sealed class GetFormByIdQueryHandler : IRequestHandler<GetFormByIdQuery, FormDetailDto>
{
    private readonly IFormRepository _formRepository;

    public GetFormByIdQueryHandler(IFormRepository formRepository)
    {
        _formRepository = formRepository;
    }

    public async Task<FormDetailDto> Handle(GetFormByIdQuery request, CancellationToken cancellationToken)
    {
        var form = await _formRepository.GetByIdWithDetailsAsync(request.FormId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Form), request.FormId);

        return form.ToDetailDto();
    }
}
