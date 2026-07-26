import { fieldTypeLabel } from '@/lib/fieldTypes'
import type { FormField } from '@/types/api'
import { Button } from '@/ui/Button'

interface FieldListProps {
  fields: FormField[]
  selectedFieldId: number | null
  canEdit: boolean
  onSelect: (fieldId: number) => void
  onMove: (fieldId: number, direction: -1 | 1) => void
}

export function FieldList({
  fields,
  selectedFieldId,
  canEdit,
  onSelect,
  onMove,
}: FieldListProps) {
  const ordered = [...fields].sort((a, b) => a.displayOrder - b.displayOrder)

  if (ordered.length === 0) {
    return <div className="fb-empty">هنوز فیلدی ندارید.</div>
  }

  return (
    <div className="fb-stack">
      {ordered.map((field, index) => (
        <div key={field.id} className="fb-row" style={{ alignItems: 'stretch' }}>
          <button
            type="button"
            className="fb-field-item"
            data-active={selectedFieldId === field.id}
            onClick={() => onSelect(field.id)}
          >
            <span className="fb-chip">{index + 1}</span>
            <span>
              <h4>
                {field.label}
                {field.isRequired ? ' *' : ''}
              </h4>
              <small>
                {fieldTypeLabel(field.fieldType)} · <span dir="ltr">{field.name}</span>
              </small>
            </span>
            <span className="fb-chip">{field.options.length} opt</span>
          </button>
          {canEdit ? (
            <div className="fb-stack" style={{ width: 42 }}>
              <Button
                variant="ghost"
                disabled={index === 0}
                onClick={() => onMove(field.id, -1)}
                aria-label="Move up"
              >
                ↑
              </Button>
              <Button
                variant="ghost"
                disabled={index === ordered.length - 1}
                onClick={() => onMove(field.id, 1)}
                aria-label="Move down"
              >
                ↓
              </Button>
            </div>
          ) : null}
        </div>
      ))}
    </div>
  )
}
