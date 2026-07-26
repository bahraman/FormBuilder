import { useCallback, useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { formsApi } from '@/api/formsApi'
import { ApiError } from '@/api/client'
import { AppShell } from '@/components/AppShell'
import { StatusBadge } from '@/components/StatusBadge'
import { useTenant } from '@/context/TenantContext'
import { CreateFormModal } from '@/features/forms/CreateFormModal'
import { Button } from '@/ui/Button'
import { Input } from '@/ui/Input'
import { Select } from '@/ui/Select'
import { Spinner } from '@/ui/Spinner'
import type { FormStatus, FormSummary } from '@/types/api'

export function FormsPage() {
  const navigate = useNavigate()
  const { subscriberId, restaurantId } = useTenant()
  const [items, setItems] = useState<FormSummary[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [pageNumber, setPageNumber] = useState(1)
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState<'' | FormStatus>('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [createOpen, setCreateOpen] = useState(false)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const result = await formsApi.list({
        subscriberId,
        restaurantId,
        pageNumber,
        pageSize: 20,
        search: search.trim() || undefined,
        status: status === '' ? undefined : status,
      })
      setItems(result.items ?? [])
      setTotalCount(result.totalCount ?? 0)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'بارگذاری فرم‌ها ناموفق بود.')
      setItems([])
    } finally {
      setLoading(false)
    }
  }, [subscriberId, restaurantId, pageNumber, search, status])

  useEffect(() => {
    void load()
  }, [load])

  return (
    <AppShell
      title="لیست فرم‌ها"
      actions={
        <Button variant="primary" onClick={() => setCreateOpen(true)}>
          + فرم جدید
        </Button>
      }
    >
      <div className="fb-panel fb-panel-pad">
        <div className="fb-toolbar">
          <Input
            label="جستجو"
            placeholder="نام یا slug"
            value={search}
            onChange={(event) => {
              setPageNumber(1)
              setSearch(event.target.value)
            }}
          />
          <Select
            label="وضعیت"
            value={status}
            onChange={(event) => {
              setPageNumber(1)
              setStatus(event.target.value as '' | FormStatus)
            }}
            options={[
              { value: '', label: 'همه' },
              { value: 'Draft', label: 'پیش‌نویس' },
              { value: 'Published', label: 'منتشر شده' },
              { value: 'Archived', label: 'بایگانی' },
            ]}
          />
          <Button variant="secondary" onClick={() => void load()}>
            بروزرسانی
          </Button>
        </div>

        {error ? <div className="fb-error">{error}</div> : null}

        {loading ? (
          <Spinner />
        ) : items.length === 0 ? (
          <div className="fb-empty">
            هنوز فرمی نیست. اولین فرم را بسازید.
            <div style={{ marginTop: '1rem' }}>
              <Button variant="teal" onClick={() => setCreateOpen(true)}>
                ایجاد فرم
              </Button>
            </div>
          </div>
        ) : (
          <div className="fb-grid-forms">
            {items.map((form) => (
              <Link key={form.id} to={`/forms/${form.id}`} className="fb-form-row">
                <div>
                  <h3>{form.name}</h3>
                  <p>{form.description || form.slug}</p>
                  <div className="fb-meta">
                    <StatusBadge status={form.status} />
                    <span className="fb-chip">v{form.version}</span>
                    <span className="fb-chip">{form.fieldCount} فیلد</span>
                    <span className="fb-chip" dir="ltr">
                      {form.slug}
                    </span>
                    {form.restaurantId ? (
                      <span className="fb-chip">رستوران {form.restaurantId}</span>
                    ) : (
                      <span className="fb-chip">مشترک</span>
                    )}
                  </div>
                </div>
                <Button variant="secondary">ویرایش</Button>
              </Link>
            ))}
          </div>
        )}

        <div className="fb-row-between" style={{ marginTop: '1rem' }}>
          <span className="fb-muted">{totalCount} فرم</span>
          <div className="fb-row">
            <Button
              variant="ghost"
              disabled={pageNumber <= 1}
              onClick={() => setPageNumber((page) => Math.max(1, page - 1))}
            >
              قبلی
            </Button>
            <span className="fb-muted">صفحه {pageNumber}</span>
            <Button
              variant="ghost"
              disabled={items.length < 20}
              onClick={() => setPageNumber((page) => page + 1)}
            >
              بعدی
            </Button>
          </div>
        </div>
      </div>

      <CreateFormModal
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        onCreated={(form) => navigate(`/forms/${form.id}`)}
      />
    </AppShell>
  )
}
