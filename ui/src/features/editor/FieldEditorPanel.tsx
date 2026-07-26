import { useEffect, useState } from 'react'
import { formsApi } from '@/api/formsApi'
import { ApiError } from '@/api/client'
import { useTenant } from '@/context/TenantContext'
import { FIELD_TYPES, normalizeFieldType, supportsOptions } from '@/lib/fieldTypes'
import { Button } from '@/ui/Button'
import { Input } from '@/ui/Input'
import { Select } from '@/ui/Select'
import { TextArea } from '@/ui/TextArea'
import type { FieldOptionInput, FieldType, FormDetail, FormField } from '@/types/api'

interface FieldEditorPanelProps {
  form: FormDetail
  selectedField: FormField | null
  onChanged: (form: FormDetail) => void
  onSelectField: (fieldId: number | null) => void
}

interface DraftOption {
  label: string
  value: string
  isDefault: boolean
}

export function FieldEditorPanel({
  form,
  selectedField,
  onChanged,
  onSelectField,
}: FieldEditorPanelProps) {
  const { subscriberId, restaurantId, actor } = useTenant()
  const [mode, setMode] = useState<'idle' | 'create' | 'edit'>('idle')
  const [name, setName] = useState('')
  const [label, setLabel] = useState('')
  const [fieldType, setFieldType] = useState<Exclude<FieldType, number>>('Text')
  const [isRequired, setIsRequired] = useState(false)
  const [placeholder, setPlaceholder] = useState('')
  const [helpText, setHelpText] = useState('')
  const [defaultValue, setDefaultValue] = useState('')
  const [options, setOptions] = useState<DraftOption[]>([])
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!selectedField) {
      return
    }

    setMode('edit')
    setName(selectedField.name)
    setLabel(selectedField.label)
    setFieldType(normalizeFieldType(selectedField.fieldType))
    setIsRequired(selectedField.isRequired)
    setPlaceholder(selectedField.placeholder ?? '')
    setHelpText(selectedField.helpText ?? '')
    setDefaultValue(selectedField.defaultValue ?? '')
    setOptions(
      selectedField.options.map((option) => ({
        label: option.label,
        value: option.value,
        isDefault: option.isDefault,
      })),
    )
    setError(null)
  }, [selectedField])

  function startCreate() {
    onSelectField(null)
    setMode('create')
    setName('')
    setLabel('')
    setFieldType('Text')
    setIsRequired(false)
    setPlaceholder('')
    setHelpText('')
    setDefaultValue('')
    setOptions([])
    setError(null)
  }

  async function reloadForm() {
    const fresh = await formsApi.getById(form.id, subscriberId, restaurantId)
    onChanged(fresh)
    return fresh
  }

  async function handleSave() {
    setSaving(true)
    setError(null)
    try {
      const optionPayload: FieldOptionInput[] | null = supportsOptions(fieldType)
        ? options
            .filter((option) => option.label.trim() && option.value.trim())
            .map((option, index) => ({
              label: option.label.trim(),
              value: option.value.trim(),
              displayOrder: index,
              isDefault: option.isDefault,
            }))
        : null

      if (mode === 'create') {
        const created = await formsApi.addField(
          form.id,
          subscriberId,
          {
            name: name.trim(),
            label: label.trim(),
            fieldType,
            displayOrder: form.fields.length,
            isRequired,
            placeholder: placeholder.trim() || null,
            helpText: helpText.trim() || null,
            defaultValue: defaultValue.trim() || null,
            options: optionPayload,
            createdBy: actor,
          },
          restaurantId,
        )
        const fresh = await reloadForm()
        onSelectField(created.id)
        const exists = fresh.fields.find((field) => field.id === created.id)
        if (exists) onSelectField(exists.id)
      } else if (selectedField) {
        await formsApi.updateField(
          form.id,
          selectedField.id,
          subscriberId,
          {
            label: label.trim(),
            isRequired,
            placeholder: placeholder.trim() || null,
            helpText: helpText.trim() || null,
            defaultValue: defaultValue.trim() || null,
            rowVersion: selectedField.rowVersion,
            options: optionPayload,
            updatedBy: actor,
          },
          restaurantId,
        )
        await reloadForm()
      }
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'ذخیره فیلد ناموفق بود.')
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete() {
    if (!selectedField) return
    if (!window.confirm('این فیلد حذف شود؟')) return
    setSaving(true)
    setError(null)
    try {
      await formsApi.deleteField(form.id, selectedField.id, subscriberId, restaurantId, actor)
      onSelectField(null)
      setMode('idle')
      await reloadForm()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'حذف فیلد ناموفق بود.')
    } finally {
      setSaving(false)
    }
  }

  if (mode === 'idle') {
    return (
      <aside className="fb-panel fb-panel-pad">
        <h3 className="fb-section-title">فیلدها</h3>
        <p className="fb-muted">یک فیلد را انتخاب کنید یا فیلد جدید بسازید.</p>
        <Button variant="teal" onClick={startCreate}>
          + فیلد جدید
        </Button>
      </aside>
    )
  }

  return (
    <aside className="fb-panel fb-panel-pad">
      <div className="fb-row-between">
        <h3 className="fb-section-title" style={{ marginBottom: 0 }}>
          {mode === 'create' ? 'فیلد جدید' : 'ویرایش فیلد'}
        </h3>
        <Button variant="ghost" onClick={startCreate}>
          +
        </Button>
      </div>

      <div className="fb-stack" style={{ marginTop: '0.85rem' }}>
        {error ? <div className="fb-error">{error}</div> : null}

        {mode === 'create' ? (
          <Input
            label="Name (سیستمی)"
            value={name}
            onChange={(event) => setName(event.target.value)}
            dir="ltr"
            style={{ fontFamily: 'var(--fb-font-latin)' }}
            placeholder="rating"
          />
        ) : (
          <Input label="Name" value={name} disabled dir="ltr" />
        )}

        <Input
          label="برچسب"
          value={label}
          onChange={(event) => setLabel(event.target.value)}
          placeholder="امتیاز"
        />

        <Select
          label="نوع فیلد"
          value={fieldType}
          disabled={mode === 'edit'}
          onChange={(event) => setFieldType(normalizeFieldType(event.target.value))}
          options={FIELD_TYPES.map((item) => ({ value: item.value, label: item.label }))}
        />

        <label className="fb-row" style={{ gap: '0.45rem' }}>
          <input
            type="checkbox"
            checked={isRequired}
            onChange={(event) => setIsRequired(event.target.checked)}
          />
          <span>اجباری</span>
        </label>

        <Input
          label="Placeholder"
          value={placeholder}
          onChange={(event) => setPlaceholder(event.target.value)}
        />
        <TextArea
          label="راهنما"
          value={helpText}
          onChange={(event) => setHelpText(event.target.value)}
        />
        <Input
          label="مقدار پیش‌فرض"
          value={defaultValue}
          onChange={(event) => setDefaultValue(event.target.value)}
        />

        {supportsOptions(fieldType) ? (
          <div className="fb-stack">
            <div className="fb-row-between">
              <strong>گزینه‌ها</strong>
              <Button
                variant="secondary"
                onClick={() =>
                  setOptions((current) => [
                    ...current,
                    { label: '', value: '', isDefault: false },
                  ])
                }
              >
                + گزینه
              </Button>
            </div>
            {options.map((option, index) => (
              <div key={index} className="fb-stack" style={{ padding: '0.65rem', background: 'rgba(23,33,43,0.03)', borderRadius: 12 }}>
                <Input
                  label="عنوان"
                  value={option.label}
                  onChange={(event) =>
                    setOptions((current) =>
                      current.map((item, i) =>
                        i === index ? { ...item, label: event.target.value } : item,
                      ),
                    )
                  }
                />
                <Input
                  label="Value"
                  value={option.value}
                  dir="ltr"
                  onChange={(event) =>
                    setOptions((current) =>
                      current.map((item, i) =>
                        i === index ? { ...item, value: event.target.value } : item,
                      ),
                    )
                  }
                />
                <div className="fb-row-between">
                  <label className="fb-row" style={{ gap: '0.4rem' }}>
                    <input
                      type="checkbox"
                      checked={option.isDefault}
                      onChange={(event) =>
                        setOptions((current) =>
                          current.map((item, i) =>
                            i === index
                              ? { ...item, isDefault: event.target.checked }
                              : item,
                          ),
                        )
                      }
                    />
                    پیش‌فرض
                  </label>
                  <Button
                    variant="danger"
                    onClick={() =>
                      setOptions((current) => current.filter((_, i) => i !== index))
                    }
                  >
                    حذف
                  </Button>
                </div>
              </div>
            ))}
          </div>
        ) : null}

        <div className="fb-row">
          <Button
            variant="primary"
            disabled={saving || !label.trim() || (mode === 'create' && !name.trim())}
            onClick={() => void handleSave()}
          >
            {saving ? 'در حال ذخیره…' : 'ذخیره'}
          </Button>
          {mode === 'edit' ? (
            <Button variant="danger" disabled={saving} onClick={() => void handleDelete()}>
              حذف فیلد
            </Button>
          ) : null}
        </div>
      </div>
    </aside>
  )
}
