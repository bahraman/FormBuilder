import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { TenantProvider } from '@/context/TenantContext'
import { FormEditorPage } from '@/pages/FormEditorPage'
import { FormsPage } from '@/pages/FormsPage'
import '@/styles/global.css'

export interface FormBuilderAppProps {
  /** Base path when embedding inside Vendo-designer (e.g. "/form-builder"). */
  basename?: string
  subscriberId?: number
  restaurantId?: number | null
  actor?: string
}

/**
 * Embeddable Form Builder shell.
 * Import this from Vendo-designer once you are ready to host the UI there.
 */
export function FormBuilderApp({
  basename,
  subscriberId,
  restaurantId,
  actor,
}: FormBuilderAppProps) {
  return (
    <TenantProvider
      initialSubscriberId={subscriberId}
      initialRestaurantId={restaurantId}
      initialActor={actor}
    >
      <BrowserRouter basename={basename}>
        <Routes>
          <Route path="/" element={<FormsPage />} />
          <Route path="/forms/:formId" element={<FormEditorPage />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </TenantProvider>
  )
}
