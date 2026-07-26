import './ui.css'

export function Spinner({ label = 'در حال بارگذاری…' }: { label?: string }) {
  return (
    <div className="fb-spinner" role="status" aria-live="polite">
      <span className="fb-spinner-dot" />
      <span>{label}</span>
    </div>
  )
}
