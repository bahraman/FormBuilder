using Vendo.FormBuilder.Application.Common.Forms;
using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Responses.Dtos;
using Vendo.FormBuilder.Application.Responses.Services;
using Vendo.FormBuilder.Domain.Exceptions;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Responses.Commands.UpdateFormResponse;

public sealed record UpdateFormResponseCommand(
    Guid ResponseId,
    int SubscriberId,
    IReadOnlyList<FormResponseValueInputDto> Values,
    string? UpdatedBy = null) : IRequest<FormResponseDto>;

public sealed class UpdateFormResponseCommandHandler
    : IRequestHandler<UpdateFormResponseCommand, FormResponseDto>
{
    private readonly IFormRepository _formRepository;
    private readonly IFormResponseRepository _responseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFormResponseCommandHandler(
        IFormRepository formRepository,
        IFormResponseRepository responseRepository,
        IUnitOfWork unitOfWork)
    {
        _formRepository = formRepository;
        _responseRepository = responseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FormResponseDto> Handle(
        UpdateFormResponseCommand request,
        CancellationToken cancellationToken)
    {
        var response = await _responseRepository.GetByIdForUpdateAsync(request.ResponseId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.FormResponse), request.ResponseId);

        var form = await FormAccess.GetAccessibleFormAsync(
            _formRepository,
            response.FormId,
            request.SubscriberId,
            withDetails: true,
            cancellationToken);

        FormResponseValidator.Validate(form, request.Values);

        var fields = form.Fields.Where(f => !f.IsDeleted).ToDictionary(f => f.Id);
        var nextValues = request.Values
            .Where(value => fields.ContainsKey(value.FieldId))
            .Select(value =>
            {
                var field = fields[value.FieldId];
                return (field.Id, field.Name, value.Value);
            })
            .ToList();

        response.ReplaceValues(nextValues, request.UpdatedBy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response.ToDto();
    }
}
