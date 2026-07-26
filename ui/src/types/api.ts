export type FormStatus = 'Draft' | 'Published' | 'Archived' | 0 | 1 | 2

export type FieldType =
  | 'Text'
  | 'MultilineText'
  | 'Number'
  | 'Decimal'
  | 'Date'
  | 'Time'
  | 'DateTime'
  | 'Email'
  | 'Phone'
  | 'Url'
  | 'Checkbox'
  | 'RadioButton'
  | 'Dropdown'
  | 'MultiSelect'
  | 'Password'
  | 'FileUpload'
  | 'ImageUpload'
  | number

export type ValidationRuleType =
  | 'Required'
  | 'MinLength'
  | 'MaxLength'
  | 'MinValue'
  | 'MaxValue'
  | 'Regex'
  | 'Email'
  | 'Phone'
  | 'Url'
  | 'AllowedFileTypes'
  | 'MaxFileSize'
  | number

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface FormSummary {
  id: number
  subscriberId: number
  restaurantId: number | null
  name: string
  description: string | null
  slug: string
  status: FormStatus
  version: number
  parentFormId: number | null
  publishedAtUtc: string | null
  archivedAtUtc: string | null
  createdAtUtc: string
  updatedAtUtc: string | null
  fieldCount: number
  rowVersion: string
}

export interface FieldOption {
  id: string
  label: string
  value: string
  displayOrder: number
  isDefault: boolean
}

export interface FieldValidationRule {
  id: string
  ruleType: ValidationRuleType
  value: string
  errorMessage: string | null
}

export interface FormField {
  id: number
  formId: number
  name: string
  label: string
  fieldType: FieldType
  displayOrder: number
  isRequired: boolean
  placeholder: string | null
  helpText: string | null
  defaultValue: string | null
  options: FieldOption[]
  validationRules: FieldValidationRule[]
  rowVersion: string
}

export interface FormDetail extends FormSummary {
  fields: FormField[]
}

export interface FieldOptionInput {
  label: string
  value: string
  displayOrder: number
  isDefault: boolean
}

export interface FieldValidationRuleInput {
  ruleType: ValidationRuleType
  value: string
  errorMessage?: string | null
}

export interface CreateFormPayload {
  subscriberId: number
  restaurantId?: number | null
  name: string
  description?: string | null
  slug: string
  createdBy?: string | null
}

export interface UpdateFormPayload {
  name: string
  description?: string | null
  rowVersion: string
  updatedBy?: string | null
}

export interface AddFieldPayload {
  name: string
  label: string
  fieldType: FieldType
  displayOrder: number
  isRequired?: boolean
  placeholder?: string | null
  helpText?: string | null
  defaultValue?: string | null
  options?: FieldOptionInput[] | null
  validationRules?: FieldValidationRuleInput[] | null
  createdBy?: string | null
}

export interface UpdateFieldPayload {
  label: string
  isRequired: boolean
  placeholder?: string | null
  helpText?: string | null
  defaultValue?: string | null
  rowVersion: string
  options?: FieldOptionInput[] | null
  validationRules?: FieldValidationRuleInput[] | null
  updatedBy?: string | null
}

export interface ApiProblem {
  title?: string
  detail?: string
  status?: number
  errors?: Record<string, string[]>
}
