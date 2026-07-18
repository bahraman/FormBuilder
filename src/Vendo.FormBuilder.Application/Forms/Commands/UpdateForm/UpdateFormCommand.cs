using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Forms.Dtos;
using Vendo.FormBuilder.Domain.Exceptions;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Forms.Commands.UpdateForm;

public sealed record UpdateFormCommand(
    Guid FormId,
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
        var form = await _formRepository.GetByIdWithDetailsAsync(request.FormId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Form), request.FormId);

        form.Update(request.Name, request.Description, request.UpdatedBy);
        form.RowVersion = Convert.FromBase64String(request.RowVersion);
        _formRepository.Update(form);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return form.ToDetailDto();
    }
}
