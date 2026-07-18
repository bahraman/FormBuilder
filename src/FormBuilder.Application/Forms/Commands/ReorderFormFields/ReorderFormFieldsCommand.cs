using FormBuilder.Application.Common.Mappings;
using FormBuilder.Application.Forms.Dtos;
using FormBuilder.Domain.Exceptions;
using FormBuilder.Domain.Interfaces;
using MediatR;

namespace FormBuilder.Application.Forms.Commands.ReorderFormFields;

public sealed record ReorderFormFieldsCommand(
    Guid FormId,
    IReadOnlyList<FieldOrderItemDto> FieldOrders,
    string? UpdatedBy = null) : IRequest<FormDetailDto>;

public sealed class ReorderFormFieldsCommandHandler : IRequestHandler<ReorderFormFieldsCommand, FormDetailDto>
{
    private readonly IFormRepository _formRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReorderFormFieldsCommandHandler(IFormRepository formRepository, IUnitOfWork unitOfWork)
    {
        _formRepository = formRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FormDetailDto> Handle(ReorderFormFieldsCommand request, CancellationToken cancellationToken)
    {
        var form = await _formRepository.GetByIdWithDetailsAsync(request.FormId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Form), request.FormId);

        var orders = request.FieldOrders.ToDictionary(x => x.FieldId, x => x.DisplayOrder);
        form.ReorderFields(orders, request.UpdatedBy);
        _formRepository.Update(form);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return form.ToDetailDto();
    }
}
