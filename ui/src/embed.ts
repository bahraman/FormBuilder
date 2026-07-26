/**
 * Public entry for host apps (Vendo-designer).
 *
 * Example:
 *   import { FormBuilderApp } from '../FormBuilder/ui/src/embed'
 *   <FormBuilderApp basename="/form-builder" subscriberId={subscriber.id} />
 */
export { FormBuilderApp } from '@/FormBuilderApp'
export type { FormBuilderAppProps } from '@/FormBuilderApp'
export { TenantProvider, useTenant } from '@/context/TenantContext'
export { formsApi } from '@/api/formsApi'
