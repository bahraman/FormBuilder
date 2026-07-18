using FormBuilder.Application.Common.Mappings;
using FormBuilder.Application.Forms.Dtos;
using FormBuilder.Domain.Entities;
using FormBuilder.Domain.Exceptions;
using FormBuilder.Domain.Interfaces;
using MediatR;

namespace FormBuilder.Application.Forms.Commands.CreateForm;

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
