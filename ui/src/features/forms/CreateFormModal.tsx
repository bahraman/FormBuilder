import { useEffect, useState } from 'react'
import { formsApi } from '@/api/formsApi'
import { ApiError } from '@/api/client'
import { useTenant } from '@/context/TenantContext'
import { slugify } from '@/lib/fieldTypes'
import { Button } from '@/ui/Button'
import { Input } from '@/ui/Input'
import { Modal } from '@/ui/Modal'
import { TextArea } from '@/ui/TextArea'
import type { FormDetail } from '@/types/api'

interface CreateFormModalProps {
  open: boolean
  onClose: () => void
  onCreated: (form: FormDetail) => void
}

export function CreateFormModal({ open, onClose, onCreated }: CreateFormModalProps) {
  const { subscriberId, restaurantId, actor } = useTenant()
  const [name, setName] = useState('')
  const [slug, setSlug] = useState('')
  const [description, setDescription] = useState('')
  const [slugTouched, setSlugTouched] = useState(false)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!open) return
    setName('')
    setSlug('')
    setDescription('')
    setSlugTouched(false)
    setError(null)
    setSaving(false)
  }, [open])

  useEffect(() => {
    if (!slugTouched) {
      setSlug(slugify(name))
    }
  }, [name, slugTouched])

  async function handleSubmit() {
    setSaving(true)
    setError(null)
    try {
      const form = await formsApi.create({
        subscriberId,
        restaurantId,
        name: name.trim(),
        slug: slug.trim() || slugify(name),
        description: description.trim() || null,
        createdBy: actor,
      })
      onCreated(form)
      onClose()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'ایجاد فرم ناموفق بود.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal
      open={open}
      title="فرم جدید"
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose} disabled={saving}>
            انصراف
          </Button>
          <Button variant="primary" onClick={handleSubmit} disabled={saving || !name.trim()}>
            {saving ? 'در حال ایجاد…' : 'ایجاد فرم'}
          </Button>
        </>
      }
    >
      <div className="fb-stack">
        {error ? <div className="fb-error">{error}</div> : null}
        <Input
          label="نام فرم"
          value={name}
          onChange={(event) => setName(event.target.value)}
          placeholder="مثلاً نظرسنجی منو"
          autoFocus
        />
        <Input
          label="Slug"
          value={slug}
          onChange={(event) => {
            setSlugTouched(true)
            setSlug(event.target.value)
          }}
          hint="شناسه یکتا در محدوده subscriber/restaurant"
          dir="ltr"
          style={{ fontFamily: 'var(--fb-font-latin)' }}
        />
        <TextArea
          label="توضیحات"
          value={description}
          onChange={(event) => setDescription(event.target.value)}
          placeholder="اختیاری"
        />
      </div>
    </Modal>
  )
}
