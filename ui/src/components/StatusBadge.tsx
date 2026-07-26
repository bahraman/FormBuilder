import { Badge } from '@/ui/Badge'
import { normalizeStatus, statusLabel } from '@/lib/fieldTypes'
import type { FormStatus } from '@/types/api'

export function StatusBadge({ status }: { status: FormStatus }) {
  const normalized = normalizeStatus(status)
  const tone =
    normalized === 'Draft' ? 'draft' : normalized === 'Published' ? 'published' : 'archived'

  return <Badge tone={tone}>{statusLabel(status)}</Badge>
}
