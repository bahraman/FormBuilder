import { apiRequest, buildQuery } from '@/api/client'
import type {
  AddFieldPayload,
  CreateFormPayload,
  FormDetail,
  FormField,
  FormStatus,
  FormSummary,
  PagedResult,
  UpdateFieldPayload,
  UpdateFormPayload,
} from '@/types/api'

export interface ListFormsParams {
  subscriberId: number
  restaurantId?: number | null
  pageNumber?: number
  pageSize?: number
  search?: string
  status?: FormStatus | ''
}

export const formsApi = {
  list(params: ListFormsParams) {
    return apiRequest<PagedResult<FormSummary>>(
      `/api/forms${buildQuery({
        subscriberId: params.subscriberId,
        restaurantId: params.restaurantId,
        pageNumber: params.pageNumber ?? 1,
        pageSize: params.pageSize ?? 20,
        search: params.search,
        status: params.status || undefined,
      })}`,
    )
  },

  getById(formId: number, subscriberId: number, restaurantId?: number | null) {
    return apiRequest<FormDetail>(
      `/api/forms/${formId}${buildQuery({ subscriberId, restaurantId })}`,
    )
  },

  create(payload: CreateFormPayload) {
    return apiRequest<FormDetail>('/api/forms', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  update(
    formId: number,
    subscriberId: number,
    payload: UpdateFormPayload,
    restaurantId?: number | null,
  ) {
    return apiRequest<FormDetail>(
      `/api/forms/${formId}${buildQuery({ subscriberId, restaurantId })}`,
      {
        method: 'PUT',
        body: JSON.stringify(payload),
      },
    )
  },

  publish(formId: number, subscriberId: number, restaurantId?: number | null, actor?: string) {
    return apiRequest<FormDetail>(
      `/api/forms/${formId}/publish${buildQuery({ subscriberId, restaurantId })}`,
      {
        method: 'POST',
        body: JSON.stringify({ actor }),
      },
    )
  },

  archive(formId: number, subscriberId: number, restaurantId?: number | null, actor?: string) {
    return apiRequest<FormDetail>(
      `/api/forms/${formId}/archive${buildQuery({ subscriberId, restaurantId })}`,
      {
        method: 'POST',
        body: JSON.stringify({ actor }),
      },
    )
  },

  createVersion(formId: number, subscriberId: number, restaurantId?: number | null, actor?: string) {
    return apiRequest<FormDetail>(
      `/api/forms/${formId}/versions${buildQuery({ subscriberId, restaurantId })}`,
      {
        method: 'POST',
        body: JSON.stringify({ actor }),
      },
    )
  },

  remove(formId: number, subscriberId: number, restaurantId?: number | null, deletedBy?: string) {
    return apiRequest<void>(
      `/api/forms/${formId}${buildQuery({ subscriberId, restaurantId, deletedBy })}`,
      { method: 'DELETE' },
    )
  },

  addField(
    formId: number,
    subscriberId: number,
    payload: AddFieldPayload,
    restaurantId?: number | null,
  ) {
    return apiRequest<FormField>(
      `/api/forms/${formId}/fields${buildQuery({ subscriberId, restaurantId })}`,
      {
        method: 'POST',
        body: JSON.stringify(payload),
      },
    )
  },

  updateField(
    formId: number,
    fieldId: number,
    subscriberId: number,
    payload: UpdateFieldPayload,
    restaurantId?: number | null,
  ) {
    return apiRequest<FormField>(
      `/api/forms/${formId}/fields/${fieldId}${buildQuery({ subscriberId, restaurantId })}`,
      {
        method: 'PUT',
        body: JSON.stringify(payload),
      },
    )
  },

  deleteField(
    formId: number,
    fieldId: number,
    subscriberId: number,
    restaurantId?: number | null,
    deletedBy?: string,
  ) {
    return apiRequest<void>(
      `/api/forms/${formId}/fields/${fieldId}${buildQuery({ subscriberId, restaurantId, deletedBy })}`,
      { method: 'DELETE' },
    )
  },

  reorderFields(
    formId: number,
    subscriberId: number,
    fieldOrders: { fieldId: number; displayOrder: number }[],
    restaurantId?: number | null,
    updatedBy?: string,
  ) {
    return apiRequest<FormDetail>(
      `/api/forms/${formId}/fields/reorder${buildQuery({ subscriberId, restaurantId })}`,
      {
        method: 'PUT',
        body: JSON.stringify({ fieldOrders, updatedBy }),
      },
    )
  },
}
