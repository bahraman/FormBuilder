using Vendo.FormBuilder.Application.Common.Forms;
using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Forms.Dtos;
using MediatR;

namespace Vendo.FormBuilder.Application.Forms.Queries.GetFormById;

public sealed record GetFormByIdQuery(
    Guid FormId,
    Guid SubscriberId,
    Guid? RestaurantId = null) : IRequest<FormDetailDto>;

public sealed class GetFormByIdQueryHandler : IRequestHandler<GetFormByIdQuery, FormDetailDto>
{
    private readonly Domain.Interfaces.IFormRepository _formRepository;

    public GetFormByIdQueryHandler(Domain.Interfaces.IFormRepository formRepository)
    {
        _formRepository = formRepository;
    }

    public async Task<FormDetailDto> Handle(GetFormByIdQuery request, CancellationToken cancellationToken)
    {
        var form = await FormAccess.GetAccessibleFormAsync(
            _formRepository,
            request.FormId,
            request.SubscriberId,
            request.RestaurantId,
            withDetails: true,
            cancellationToken);

        return form.ToDetailDto();
    }
}
