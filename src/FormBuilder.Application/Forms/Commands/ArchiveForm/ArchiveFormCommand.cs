using FormBuilder.Application.Common.Mappings;
using FormBuilder.Application.Forms.Dtos;
using FormBuilder.Domain.Exceptions;
using FormBuilder.Domain.Interfaces;
using MediatR;

namespace FormBuilder.Application.Forms.Commands.ArchiveForm;

public sealed record ArchiveFormCommand(Guid FormId, string? UpdatedBy = null) : IRequest<FormDetailDto>;

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
        var form = await _formRepository.GetByIdWithDetailsAsync(request.FormId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Form), request.FormId);

        form.Archive(request.UpdatedBy);
        _formRepository.Update(form);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return form.ToDetailDto();
    }
}
