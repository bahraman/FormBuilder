using Vendo.FormBuilder.Application.Common.Forms;
using Vendo.FormBuilder.Application.Common.Mappings;
using Vendo.FormBuilder.Application.Forms.Dtos;
using Vendo.FormBuilder.Domain.Exceptions;
using Vendo.FormBuilder.Domain.Interfaces;
using MediatR;

namespace Vendo.FormBuilder.Application.Forms.Commands.CreateFormVersion;

public sealed record CreateFormVersionCommand(
    long FormId,
    int SubscriberId,
    int? RestaurantId = null,
    string? CreatedBy = null) : IRequest<FormDetailDto>;

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
        var form = await FormAccess.GetAccessibleFormAsync(
            _formRepository,
            request.FormId,
            request.SubscriberId,
            request.RestaurantId,
            withDetails: true,
            cancellationToken);

        var latestVersion = await _formRepository.GetLatestVersionAsync(
            form.SubscriberId,
            form.RestaurantId,
            form.Slug,
            cancellationToken);

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
