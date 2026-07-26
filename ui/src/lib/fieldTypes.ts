import type { FieldType, FormStatus } from '@/types/api'

export const FIELD_TYPES: { value: Exclude<FieldType, number>; label: string; supportsOptions: boolean }[] = [
  { value: 'Text', label: 'متن کوتاه', supportsOptions: false },
  { value: 'MultilineText', label: 'متن چندخطی', supportsOptions: false },
  { value: 'Number', label: 'عدد', supportsOptions: false },
  { value: 'Decimal', label: 'اعشار', supportsOptions: false },
  { value: 'Date', label: 'تاریخ', supportsOptions: false },
  { value: 'Time', label: 'ساعت', supportsOptions: false },
  { value: 'DateTime', label: 'تاریخ و ساعت', supportsOptions: false },
  { value: 'Email', label: 'ایمیل', supportsOptions: false },
  { value: 'Phone', label: 'تلفن', supportsOptions: false },
  { value: 'Url', label: 'لینک', supportsOptions: false },
  { value: 'Checkbox', label: 'چک‌باکس', supportsOptions: true },
  { value: 'RadioButton', label: 'رادیو', supportsOptions: true },
  { value: 'Dropdown', label: 'کشویی', supportsOptions: true },
  { value: 'MultiSelect', label: 'چندانتخابی', supportsOptions: true },
  { value: 'Password', label: 'رمز', supportsOptions: false },
  { value: 'FileUpload', label: 'فایل', supportsOptions: false },
  { value: 'ImageUpload', label: 'تصویر', supportsOptions: false },
]

export function normalizeFieldType(type: FieldType | string): Exclude<FieldType, number> {
  if (typeof type === 'number') {
    return FIELD_TYPES[type]?.value ?? 'Text'
  }
  const match = FIELD_TYPES.find((item) => item.value === type)
  return match?.value ?? 'Text'
}

export function fieldTypeLabel(type: FieldType): string {
  const normalized = normalizeFieldType(type)
  return FIELD_TYPES.find((item) => item.value === normalized)?.label ?? String(type)
}

export function supportsOptions(type: FieldType): boolean {
  const normalized = normalizeFieldType(type)
  return FIELD_TYPES.find((item) => item.value === normalized)?.supportsOptions ?? false
}

export function normalizeStatus(status: FormStatus): 'Draft' | 'Published' | 'Archived' {
  if (status === 0 || status === 'Draft') return 'Draft'
  if (status === 1 || status === 'Published') return 'Published'
  return 'Archived'
}

export function statusLabel(status: FormStatus): string {
  const normalized = normalizeStatus(status)
  if (normalized === 'Draft') return 'پیش‌نویس'
  if (normalized === 'Published') return 'منتشر شده'
  return 'بایگانی'
}

export function slugify(value: string): string {
  return value
    .trim()
    .toLowerCase()
    .replace(/\s+/g, '-')
    .replace(/[^a-z0-9\u0600-\u06FF-]/g, '')
    .replace(/-+/g, '-')
    .replace(/^-|-$/g, '')
}
