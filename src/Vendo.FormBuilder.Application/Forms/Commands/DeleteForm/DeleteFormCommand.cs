using Vendo.FormBuilder.Application.Common.Forms;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Forms.Commands.DeleteForm;

public sealed record DeleteFormCommand(
    long FormId,
    int SubscriberId,
    int? RestaurantId = null,
    string? DeletedBy = null) : IRequest;

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
        var form = await FormAccess.GetAccessibleFormAsync(
            _formRepository,
            request.FormId,
            request.SubscriberId,
            request.RestaurantId,
            withDetails: true,
            cancellationToken);

        form.SoftDelete(request.DeletedBy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
