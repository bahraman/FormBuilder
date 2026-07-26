import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { TenantBar } from '@/components/TenantBar'

interface AppShellProps {
  children: ReactNode
  title?: string
  actions?: ReactNode
}

export function AppShell({ children, title, actions }: AppShellProps) {
  return (
    <div className="fb-app" data-theme="vendo">
      <div className="fb-shell">
        <header className="fb-topbar">
          <div className="fb-brand">
            <Link to="/">
              <strong>Vendo Form Builder</strong>
            </Link>
            <span>{title ?? 'ساخت و مدیریت فرم‌های داینامیک'}</span>
          </div>
          {actions}
        </header>
        <TenantBar />
        {children}
      </div>
    </div>
  )
}
