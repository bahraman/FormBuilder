import type { InputHTMLAttributes } from 'react'
import './ui.css'

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string
  hint?: string
}

/** Swap with Vendo-designer Input when integrating. */
export function Input({ label, hint, id, className = '', ...props }: InputProps) {
  const inputId = id ?? props.name
  return (
    <label className={`fb-field ${className}`.trim()} htmlFor={inputId}>
      {label ? <span className="fb-label">{label}</span> : null}
      <input id={inputId} className="fb-control" {...props} />
      {hint ? <span className="fb-hint">{hint}</span> : null}
    </label>
  )
}
