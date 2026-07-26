import { useCallback, useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { formsApi } from '@/api/formsApi'
import { ApiError } from '@/api/client'
import { AppShell } from '@/components/AppShell'
import { StatusBadge } from '@/components/StatusBadge'
import { useTenant } from '@/context/TenantContext'
import { FieldEditorPanel } from '@/features/editor/FieldEditorPanel'
import { FieldList } from '@/features/editor/FieldList'
import { FormPreview } from '@/features/editor/FormPreview'
import { normalizeStatus } from '@/lib/fieldTypes'
import { Button } from '@/ui/Button'
import { Input } from '@/ui/Input'
import { Spinner } from '@/ui/Spinner'
import { TextArea } from '@/ui/TextArea'
import type { FormDetail } from '@/types/api'

export function FormEditorPage() {
  const { formId } = useParams()
  const navigate = useNavigate()
  const { subscriberId, restaurantId, actor } = useTenant()
  const [form, setForm] = useState<FormDetail | null>(null)
  const [selectedFieldId, setSelectedFieldId] = useState<number | null>(null)
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [loading, setLoading] = useState(true)
  const [savingMeta, setSavingMeta] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const id = Number(formId)

  const load = useCallback(async () => {
    if (!Number.isFinite(id)) return
    setLoading(true)
    setError(null)
    try {
      const detail = await formsApi.getById(id, subscriberId, restaurantId)
      setForm(detail)
      setName(detail.name)
      setDescription(detail.description ?? '')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'بارگذاری فرم ناموفق بود.')
      setForm(null)
    } finally {
      setLoading(false)
    }
  }, [id, subscriberId, restaurantId])

  useEffect(() => {
    void load()
  }, [load])

  const selectedField =
    form?.fields.find((field) => field.id === selectedFieldId) ?? null
  const status = form ? normalizeStatus(form.status) : 'Draft'
  const canEdit = status === 'Draft'

  async function saveMeta() {
    if (!form || !canEdit) return
    setSavingMeta(true)
    setError(null)
    try {
      const updated = await formsApi.update(
        form.id,
        subscriberId,
        {
          name: name.trim(),
          description: description.trim() || null,
          rowVersion: form.rowVersion,
          updatedBy: actor,
        },
        restaurantId,
      )
      setForm(updated)
      setName(updated.name)
      setDescription(updated.description ?? '')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'ذخیره مشخصات ناموفق بود.')
    } finally {
      setSavingMeta(false)
    }
  }

  async function runLifecycle(
    action: 'publish' | 'archive' | 'version' | 'delete',
  ) {
    if (!form) return
    setError(null)
    try {
      if (action === 'publish') {
        const updated = await formsApi.publish(form.id, subscriberId, restaurantId, actor)
        setForm(updated)
      } else if (action === 'archive') {
        const updated = await formsApi.archive(form.id, subscriberId, restaurantId, actor)
        setForm(updated)
      } else if (action === 'version') {
        const created = await formsApi.createVersion(form.id, subscriberId, restaurantId, actor)
        navigate(`/forms/${created.id}`)
      } else if (action === 'delete') {
        if (!window.confirm('فرم حذف شود؟')) return
        await formsApi.remove(form.id, subscriberId, restaurantId, actor)
        navigate('/')
      }
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'عملیات ناموفق بود.')
    }
  }

  async function moveField(fieldId: number, direction: -1 | 1) {
    if (!form || !canEdit) return
    const ordered = [...form.fields].sort((a, b) => a.displayOrder - b.displayOrder)
    const index = ordered.findIndex((field) => field.id === fieldId)
    const target = index + direction
    if (index < 0 || target < 0 || target >= ordered.length) return

    const swapped = [...ordered]
    ;[swapped[index], swapped[target]] = [swapped[target], swapped[index]]

    try {
      const updated = await formsApi.reorderFields(
        form.id,
        subscriberId,
        swapped.map((field, displayOrder) => ({ fieldId: field.id, displayOrder })),
        restaurantId,
        actor,
      )
      setForm(updated)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'جابه‌جایی فیلد ناموفق بود.')
    }
  }

  if (loading) {
    return (
      <AppShell title="ویرایشگر فرم">
        <Spinner />
      </AppShell>
    )
  }

  if (!form) {
    return (
      <AppShell title="ویرایشگر فرم">
        <div className="fb-error">{error ?? 'فرم پیدا نشد.'}</div>
        <Link to="/">
          <Button variant="secondary">بازگشت</Button>
        </Link>
      </AppShell>
    )
  }

  return (
    <AppShell
      title={`ویرایش: ${form.name}`}
      actions={
        <div className="fb-row">
          <Link to="/">
            <Button variant="ghost">لیست فرم‌ها</Button>
          </Link>
          {canEdit ? (
            <Button variant="teal" onClick={() => void runLifecycle('publish')}>
              انتشار
            </Button>
          ) : null}
          {status === 'Published' ? (
            <Button variant="secondary" onClick={() => void runLifecycle('archive')}>
              بایگانی
            </Button>
          ) : null}
          {status !== 'Draft' ? (
            <Button variant="primary" onClick={() => void runLifecycle('version')}>
              نسخه جدید
            </Button>
          ) : null}
          <Button variant="danger" onClick={() => void runLifecycle('delete')}>
            حذف
          </Button>
        </div>
      }
    >
      {error ? <div className="fb-error">{error}</div> : null}

      <div className="fb-panel fb-panel-pad" style={{ marginBottom: '1rem' }}>
        <div className="fb-row" style={{ marginBottom: '0.85rem' }}>
          <StatusBadge status={form.status} />
          <span className="fb-chip">v{form.version}</span>
          <span className="fb-chip" dir="ltr">
            #{form.id}
          </span>
          <span className="fb-chip" dir="ltr">
            {form.slug}
          </span>
          {!canEdit ? (
            <span className="fb-muted">فقط پیش‌نویس قابل ویرایش است.</span>
          ) : null}
        </div>
        <div className="fb-toolbar">
          <Input
            label="نام"
            value={name}
            disabled={!canEdit}
            onChange={(event) => setName(event.target.value)}
          />
          <TextArea
            label="توضیحات"
            value={description}
            disabled={!canEdit}
            onChange={(event) => setDescription(event.target.value)}
          />
          {canEdit ? (
            <Button variant="secondary" disabled={savingMeta} onClick={() => void saveMeta()}>
              {savingMeta ? '…' : 'ذخیره مشخصات'}
            </Button>
          ) : null}
        </div>
      </div>

      <div className="fb-editor">
        <FieldEditorPanel
          form={form}
          selectedField={selectedField}
          onChanged={setForm}
          onSelectField={setSelectedFieldId}
        />

        <section className="fb-panel fb-panel-pad fb-editor-main">
          <div className="fb-row-between">
            <h3 className="fb-section-title" style={{ marginBottom: 0 }}>
              ساختار فرم
            </h3>
            <span className="fb-muted">{form.fields.length} فیلد</span>
          </div>
          <FieldList
            fields={form.fields}
            selectedFieldId={selectedFieldId}
            canEdit={canEdit}
            onSelect={setSelectedFieldId}
            onMove={(fieldId, direction) => void moveField(fieldId, direction)}
          />
        </section>

        <FormPreview form={form} />
      </div>
    </AppShell>
  )
}
