using Vendo.FormBuilder.Application.Common.Forms;
using Vendo.FormBuilder.Domain.Exceptions;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Responses.Commands.DeleteFormResponse;

public sealed record DeleteFormResponseCommand(
    Guid ResponseId,
    int SubscriberId,
    int? RestaurantId = null,
    string? DeletedBy = null) : IRequest;

public sealed class DeleteFormResponseCommandHandler : IRequestHandler<DeleteFormResponseCommand>
{
    private readonly IFormRepository _formRepository;
    private readonly IFormResponseRepository _responseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFormResponseCommandHandler(
        IFormRepository formRepository,
        IFormResponseRepository responseRepository,
        IUnitOfWork unitOfWork)
    {
        _formRepository = formRepository;
        _responseRepository = responseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteFormResponseCommand request, CancellationToken cancellationToken)
    {
        var response = await _responseRepository.GetByIdForUpdateAsync(request.ResponseId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.FormResponse), request.ResponseId);

        _ = await FormAccess.GetAccessibleFormAsync(
            _formRepository,
            response.FormId,
            request.SubscriberId,
            request.RestaurantId,
            withDetails: false,
            cancellationToken);

        response.SoftDelete(request.DeletedBy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
