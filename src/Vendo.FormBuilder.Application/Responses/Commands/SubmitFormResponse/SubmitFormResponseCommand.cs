using Vendo.FormBuilder.Application.Common.Forms;
using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Responses.Dtos;
using Vendo.FormBuilder.Application.Responses.Services;
using Vendo.FormBuilder.Domain.Entities;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Responses.Commands.SubmitFormResponse;

public sealed record SubmitFormResponseCommand(
    long FormId,
    int SubscriberId,
    int? RestaurantId,
    IReadOnlyList<FormResponseValueInputDto> Values,
    string? SubmittedBy = null,
    string? IpAddress = null,
    string? UserAgent = null) : IRequest<FormResponseDto>;

public sealed class SubmitFormResponseCommandHandler : IRequestHandler<SubmitFormResponseCommand, FormResponseDto>
{
    private readonly IFormRepository _formRepository;
    private readonly IFormResponseRepository _responseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitFormResponseCommandHandler(
        IFormRepository formRepository,
        IFormResponseRepository responseRepository,
        IUnitOfWork unitOfWork)
    {
        _formRepository = formRepository;
        _responseRepository = responseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FormResponseDto> Handle(SubmitFormResponseCommand request, CancellationToken cancellationToken)
    {
        var form = await FormAccess.GetAccessibleFormAsync(
            _formRepository,
            request.FormId,
            request.SubscriberId,
            request.RestaurantId,
            withDetails: true,
            cancellationToken);

        form.EnsureAcceptsSubmissions();
        FormResponseValidator.Validate(form, request.Values);

        var response = FormResponse.Create(
            form.Id,
            request.SubmittedBy,
            request.IpAddress,
            request.UserAgent);

        var fields = form.Fields.Where(f => !f.IsDeleted).ToDictionary(f => f.Id);
        foreach (var value in request.Values)
        {
            if (!fields.TryGetValue(value.FieldId, out var field))
            {
                continue;
            }

            response.AddValue(field.Id, field.Name, value.Value);
        }

        await _responseRepository.AddAsync(response, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response.ToDto();
    }
}
