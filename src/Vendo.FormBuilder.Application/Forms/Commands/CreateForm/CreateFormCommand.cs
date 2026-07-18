using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Forms.Dtos;
using Vendo.FormBuilder.Domain.Entities;
using Vendo.FormBuilder.Domain.Exceptions;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Forms.Commands.CreateForm;

public sealed record CreateFormCommand(
    string Name,
    string? Description,
    string Slug,
    string? CreatedBy = null) : IRequest<FormDetailDto>;

public sealed class CreateFormCommandHandler : IRequestHandler<CreateFormCommand, FormDetailDto>
{
    private readonly IFormRepository _formRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFormCommandHandler(IFormRepository formRepository, IUnitOfWork unitOfWork)
    {
        _formRepository = formRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FormDetailDto> Handle(CreateFormCommand request, CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();

        if (await _formRepository.SlugExistsAsync(slug, cancellationToken: cancellationToken))
        {
            throw new ConflictException($"A form with slug '{slug}' already exists.");
        }

        var form = Form.Create(request.Name, request.Description, slug, request.CreatedBy);
        await _formRepository.AddAsync(form, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return form.ToDetailDto();
    }
}
