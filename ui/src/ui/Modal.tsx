import type { ReactNode } from 'react'
import { useEffect } from 'react'
import { Button } from '@/ui/Button'
import './ui.css'

interface ModalProps {
  open: boolean
  title: string
  onClose: () => void
  children: ReactNode
  footer?: ReactNode
}

/** Swap with Vendo-designer Modal/Drawer when integrating. */
export function Modal({ open, title, onClose, children, footer }: ModalProps) {
  useEffect(() => {
    if (!open) return
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') onClose()
    }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [open, onClose])

  if (!open) return null

  return (
    <div className="fb-modal-root" role="presentation" onClick={onClose}>
      <div
        className="fb-modal"
        role="dialog"
        aria-modal="true"
        aria-label={title}
        onClick={(event) => event.stopPropagation()}
      >
        <div className="fb-modal-header">
          <h2>{title}</h2>
          <Button variant="ghost" onClick={onClose} aria-label="Close">
            ✕
          </Button>
        </div>
        <div className="fb-modal-body">{children}</div>
        {footer ? <div className="fb-modal-footer">{footer}</div> : null}
      </div>
    </div>
  )
}
