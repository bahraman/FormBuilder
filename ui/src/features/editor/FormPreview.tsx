import { fieldTypeLabel, normalizeFieldType, supportsOptions } from '@/lib/fieldTypes'
import type { FormDetail, FormField } from '@/types/api'

function PreviewControl({ field }: { field: FormField }) {
  const type = normalizeFieldType(field.fieldType)

  if (type === 'MultilineText') {
    return <textarea className="fb-control fb-textarea" placeholder={field.placeholder ?? ''} disabled />
  }

  if (supportsOptions(field.fieldType)) {
    if (type === 'Dropdown' || type === 'MultiSelect') {
      return (
        <select className="fb-control" disabled multiple={type === 'MultiSelect'}>
          {field.options.map((option) => (
            <option key={option.id} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      )
    }

    return (
      <div className="fb-stack">
        {field.options.map((option) => (
          <label key={option.id} className="fb-row" style={{ gap: '0.4rem' }}>
            <input type={type === 'Checkbox' ? 'checkbox' : 'radio'} disabled />
            <span>{option.label}</span>
          </label>
        ))}
      </div>
    )
  }

  if (type === 'FileUpload' || type === 'ImageUpload') {
    return <input className="fb-control" type="file" disabled />
  }

  const inputType =
    type === 'Number' || type === 'Decimal'
      ? 'number'
      : type === 'Date'
        ? 'date'
        : type === 'Time'
          ? 'time'
          : type === 'DateTime'
            ? 'datetime-local'
            : type === 'Email'
              ? 'email'
              : type === 'Phone'
                ? 'tel'
                : type === 'Url'
                  ? 'url'
                  : type === 'Password'
                    ? 'password'
                    : 'text'

  return (
    <input
      className="fb-control"
      type={inputType}
      placeholder={field.placeholder ?? ''}
      defaultValue={field.defaultValue ?? ''}
      disabled
    />
  )
}

export function FormPreview({ form }: { form: FormDetail }) {
  const fields = [...form.fields].sort((a, b) => a.displayOrder - b.displayOrder)

  return (
    <aside className="fb-panel fb-panel-pad">
      <h3 className="fb-section-title">پیش‌نمایش</h3>
      <p className="fb-muted" style={{ marginTop: 0 }}>
        {form.name}
      </p>
      {fields.length === 0 ? (
        <div className="fb-empty">هنوز فیلدی اضافه نشده.</div>
      ) : (
        fields.map((field) => (
          <div key={field.id} className="fb-preview-field">
            <label>
              {field.label}
              {field.isRequired ? ' *' : ''}
            </label>
            <PreviewControl field={field} />
            <div className="hint">
              {fieldTypeLabel(field.fieldType)}
              {field.helpText ? ` — ${field.helpText}` : ''}
            </div>
          </div>
        ))
      )}
    </aside>
  )
}
