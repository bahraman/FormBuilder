using Vendo.FormBuilder.Application.Common.Forms;
using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Forms.Dtos;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Forms.Commands.PublishForm;

public sealed record PublishFormCommand(
    long FormId,
    int SubscriberId,
    string? UpdatedBy = null) : IRequest<FormDetailDto>;

public sealed class PublishFormCommandHandler : IRequestHandler<PublishFormCommand, FormDetailDto>
{
    private readonly IFormRepository _formRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PublishFormCommandHandler(IFormRepository formRepository, IUnitOfWork unitOfWork)
    {
        _formRepository = formRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FormDetailDto> Handle(PublishFormCommand request, CancellationToken cancellationToken)
    {
        var form = await FormAccess.GetAccessibleFormAsync(
            _formRepository,
            request.FormId,
            request.SubscriberId,
            withDetails: true,
            cancellationToken);

        form.Publish(request.UpdatedBy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return form.ToDetailDto();
    }
}
