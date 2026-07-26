import type { ButtonHTMLAttributes, ReactNode } from 'react'
import './ui.css'

type Variant = 'primary' | 'secondary' | 'ghost' | 'danger' | 'teal'

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant
  children: ReactNode
}

/**
 * Thin button primitive.
 * Swap this implementation with Vendo-designer Button when integrating.
 */
export function Button({ variant = 'primary', className = '', children, ...props }: ButtonProps) {
  return (
    <button type="button" className={`fb-btn fb-btn-${variant} ${className}`.trim()} {...props}>
      {children}
    </button>
  )
}
