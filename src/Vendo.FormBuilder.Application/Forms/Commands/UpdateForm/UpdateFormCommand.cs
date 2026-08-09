using Vendo.FormBuilder.Application.Common.Forms;
using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Forms.Dtos;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Forms.Commands.UpdateForm;

public sealed record UpdateFormCommand(
    long FormId,
    int SubscriberId,
    string Name,
    string? Description,
    string RowVersion,
    string? UpdatedBy = null) : IRequest<FormDetailDto>;

public sealed class UpdateFormCommandHandler : IRequestHandler<UpdateFormCommand, FormDetailDto>
{
    private readonly IFormRepository _formRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFormCommandHandler(IFormRepository formRepository, IUnitOfWork unitOfWork)
    {
        _formRepository = formRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FormDetailDto> Handle(UpdateFormCommand request, CancellationToken cancellationToken)
    {
        var form = await FormAccess.GetAccessibleFormAsync(
            _formRepository,
            request.FormId,
            request.SubscriberId,
            withDetails: true,
            cancellationToken);

        form.Update(request.Name, request.Description, request.UpdatedBy);
        _formRepository.SetOriginalRowVersion(form, Convert.FromBase64String(request.RowVersion));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return form.ToDetailDto();
    }
}
