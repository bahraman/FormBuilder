using Vendo.FormBuilder.Application.Common.Forms;
using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Forms.Dtos;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Forms.Commands.ArchiveForm;

public sealed record ArchiveFormCommand(
    long FormId,
    int SubscriberId,
    string? UpdatedBy = null) : IRequest<FormDetailDto>;

public sealed class ArchiveFormCommandHandler : IRequestHandler<ArchiveFormCommand, FormDetailDto>
{
    private readonly IFormRepository _formRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveFormCommandHandler(IFormRepository formRepository, IUnitOfWork unitOfWork)
    {
        _formRepository = formRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FormDetailDto> Handle(ArchiveFormCommand request, CancellationToken cancellationToken)
    {
        var form = await FormAccess.GetAccessibleFormAsync(
            _formRepository,
            request.FormId,
            request.SubscriberId,
            withDetails: true,
            cancellationToken);

        form.Archive(request.UpdatedBy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return form.ToDetailDto();
    }
}
