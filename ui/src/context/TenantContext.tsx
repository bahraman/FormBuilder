import {
  createContext,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react'

export interface TenantState {
  subscriberId: number
  restaurantId: number | null
  actor: string
  setSubscriberId: (value: number) => void
  setRestaurantId: (value: number | null) => void
  setActor: (value: string) => void
}

const TenantContext = createContext<TenantState | null>(null)

function readNumberEnv(name: string, fallback: number): number {
  const raw = import.meta.env[name] as string | undefined
  const parsed = Number(raw)
  return Number.isFinite(parsed) && parsed > 0 ? parsed : fallback
}

function readOptionalNumberEnv(name: string): number | null {
  const raw = import.meta.env[name] as string | undefined
  if (!raw) return null
  const parsed = Number(raw)
  if (!Number.isFinite(parsed) || parsed <= 0) return null
  return parsed
}

interface TenantProviderProps {
  children: ReactNode
  initialSubscriberId?: number
  initialRestaurantId?: number | null
  initialActor?: string
}

export function TenantProvider({
  children,
  initialSubscriberId,
  initialRestaurantId,
  initialActor = 'vendo-ui',
}: TenantProviderProps) {
  const [subscriberId, setSubscriberId] = useState(
    initialSubscriberId ?? readNumberEnv('VITE_DEFAULT_SUBSCRIBER_ID', 1),
  )
  const [restaurantId, setRestaurantId] = useState<number | null>(
    initialRestaurantId ?? readOptionalNumberEnv('VITE_DEFAULT_RESTAURANT_ID'),
  )
  const [actor, setActor] = useState(initialActor)

  const value = useMemo(
    () => ({
      subscriberId,
      restaurantId,
      actor,
      setSubscriberId,
      setRestaurantId,
      setActor,
    }),
    [subscriberId, restaurantId, actor],
  )

  return <TenantContext.Provider value={value}>{children}</TenantContext.Provider>
}

export function useTenant() {
  const ctx = useContext(TenantContext)
  if (!ctx) {
    throw new Error('useTenant must be used within TenantProvider')
  }
  return ctx
}
