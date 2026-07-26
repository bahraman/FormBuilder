import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { FormBuilderApp } from '@/FormBuilderApp'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <FormBuilderApp />
  </StrictMode>,
)
