using Vendo.FormBuilder.Application.Common.Forms;
using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Forms.Dtos;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Forms.Commands.ReorderFormFields;

public sealed record ReorderFormFieldsCommand(
    long FormId,
    int SubscriberId,
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
        var form = await FormAccess.GetAccessibleFormAsync(
            _formRepository,
            request.FormId,
            request.SubscriberId,
            withDetails: true,
            cancellationToken);

        var orders = request.FieldOrders.ToDictionary(x => x.FieldId, x => x.DisplayOrder);
        form.ReorderFields(orders, request.UpdatedBy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return form.ToDetailDto();
    }
}
