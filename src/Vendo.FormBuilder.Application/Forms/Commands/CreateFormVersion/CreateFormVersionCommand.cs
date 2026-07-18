using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Forms.Dtos;
using Vendo.FormBuilder.Domain.Exceptions;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Forms.Commands.CreateFormVersion;

public sealed record CreateFormVersionCommand(Guid FormId, string? CreatedBy = null) : IRequest<FormDetailDto>;

public sealed class CreateFormVersionCommandHandler : IRequestHandler<CreateFormVersionCommand, FormDetailDto>
{
    private readonly IFormRepository _formRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFormVersionCommandHandler(IFormRepository formRepository, IUnitOfWork unitOfWork)
    {
        _formRepository = formRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FormDetailDto> Handle(CreateFormVersionCommand request, CancellationToken cancellationToken)
    {
        var form = await _formRepository.GetByIdWithDetailsAsync(request.FormId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Form), request.FormId);

        var latestVersion = await _formRepository.GetLatestVersionAsync(form.Slug, cancellationToken);
        if (form.Version < latestVersion)
        {
            throw new ConflictException(
                $"A newer version ({latestVersion}) already exists for slug '{form.Slug}'. Version from the latest form instead.");
        }

        var newVersion = form.CreateNewVersion(request.CreatedBy);
        await _formRepository.AddAsync(newVersion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return newVersion.ToDetailDto();
    }
}
