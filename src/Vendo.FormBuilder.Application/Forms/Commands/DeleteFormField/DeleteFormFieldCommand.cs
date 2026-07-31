using Vendo.FormBuilder.Application.Common.Forms;
using Vendo.FormBuilder.Domain.Exceptions;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Forms.Commands.DeleteFormField;

public sealed record DeleteFormFieldCommand(
    long FormId,
    long FieldId,
    int SubscriberId,
    int? RestaurantId = null,
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
        var form = await FormAccess.GetAccessibleFormAsync(
            _formRepository,
            request.FormId,
            request.SubscriberId,
            request.RestaurantId,
            withDetails: true,
            cancellationToken);

        if (form.Status != Domain.Enums.FormStatus.Draft)
        {
            throw new ConflictException("Only draft forms can be modified. Create a new version to make changes.");
        }

        var field = form.GetField(request.FieldId);
        field.SoftDelete(request.DeletedBy);
        form.UpdatedAtUtc = DateTime.UtcNow;
        form.UpdatedBy = request.DeletedBy;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
