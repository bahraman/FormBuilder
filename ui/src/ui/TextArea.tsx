import type { TextareaHTMLAttributes } from 'react'
import './ui.css'

interface TextAreaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label?: string
}

/** Swap with Vendo-designer TextArea when integrating. */
export function TextArea({ label, id, className = '', ...props }: TextAreaProps) {
  const inputId = id ?? props.name
  return (
    <label className={`fb-field ${className}`.trim()} htmlFor={inputId}>
      {label ? <span className="fb-label">{label}</span> : null}
      <textarea id={inputId} className="fb-control fb-textarea" {...props} />
    </label>
  )
}
