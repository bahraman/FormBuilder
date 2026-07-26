import type { ReactNode } from 'react'
import './ui.css'

type Tone = 'neutral' | 'draft' | 'published' | 'archived' | 'brand'

interface BadgeProps {
  tone?: Tone
  children: ReactNode
}

/** Swap with Vendo-designer Badge/Tag when integrating. */
export function Badge({ tone = 'neutral', children }: BadgeProps) {
  return <span className={`fb-badge fb-badge-${tone}`}>{children}</span>
}
