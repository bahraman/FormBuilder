import type { SelectHTMLAttributes } from 'react'
import './ui.css'

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label?: string
  options: { value: string; label: string }[]
}

/** Swap with Vendo-designer Select when integrating. */
export function Select({ label, options, id, className = '', ...props }: SelectProps) {
  const inputId = id ?? props.name
  return (
    <label className={`fb-field ${className}`.trim()} htmlFor={inputId}>
      {label ? <span className="fb-label">{label}</span> : null}
      <select id={inputId} className="fb-control" {...props}>
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </label>
  )
}
