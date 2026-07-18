using FormBuilder.Domain.Exceptions;
using FormBuilder.Domain.Interfaces;
using MediatR;

namespace FormBuilder.Application.Forms.Commands.DeleteForm;

public sealed record DeleteFormCommand(Guid FormId, string? DeletedBy = null) : IRequest;

public sealed class DeleteFormCommandHandler : IRequestHandler<DeleteFormCommand>
{
    private readonly IFormRepository _formRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFormCommandHandler(IFormRepository formRepository, IUnitOfWork unitOfWork)
    {
        _formRepository = formRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteFormCommand request, CancellationToken cancellationToken)
    {
        var form = await _formRepository.GetByIdWithDetailsAsync(request.FormId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Form), request.FormId);

        form.SoftDelete(request.DeletedBy);
        _formRepository.Update(form);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
