using Vendo.FormBuilder.Domain.Exceptions;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Forms.Commands.DeleteFormField;

public sealed record DeleteFormFieldCommand(
    Guid FormId,
    Guid FieldId,
    string? DeletedBy = null) : IRequest;

public sealed class DeleteFormFieldCommandHandler : IRequestHandler<DeleteFormFieldCommand>
{
    private readonly IFormRepository _formRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFormFieldCommandHandler(IFormRepository formRepository, IUnitOfWork unitOfWork)
    {
        _formRepository = formRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteFormFieldCommand request, CancellationToken cancellationToken)
    {
        var form = await _formRepository.GetByIdWithDetailsAsync(request.FormId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Form), request.FormId);

        if (form.Status != Domain.Enums.FormStatus.Draft)
        {
            throw new ConflictException("Only draft forms can be modified. Create a new version to make changes.");
        }

        var field = form.GetField(request.FieldId);
        field.SoftDelete(request.DeletedBy);
        form.UpdatedAtUtc = DateTime.UtcNow;
        form.UpdatedBy = request.DeletedBy;

        _formRepository.Update(form);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
