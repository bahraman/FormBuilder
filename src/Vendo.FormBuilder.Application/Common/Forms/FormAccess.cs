using Vendo.FormBuilder.Domain.Common;
using Vendo.FormBuilder.Domain.Entities;
using Vendo.FormBuilder.Domain.Exceptions;
using Vendo.FormBuilder.Domain.Interfaces;

namespace Vendo.FormBuilder.Application.Common.Forms;

internal static class FormAccess
{
    public static async Task<Form> GetAccessibleFormAsync(
        IFormRepository formRepository,
        Guid formId,
        int subscriberId,
        int? restaurantId,
        bool withDetails,
        CancellationToken cancellationToken)
    {
        var scope = TenantScope.ForSubscriber(subscriberId, restaurantId);

        var form = withDetails
            ? await formRepository.GetByIdWithDetailsAsync(formId, cancellationToken)
            : await formRepository.GetByIdAsync(formId, cancellationToken);

        if (form is null)
        {
            throw new NotFoundException(nameof(Form), formId);
        }

        form.EnsureAccessibleTo(scope);
        return form;
    }
}
