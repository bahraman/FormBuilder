import { useTenant } from '@/context/TenantContext'
import { Input } from '@/ui/Input'

export function TenantBar() {
  const { subscriberId, restaurantId, actor, setSubscriberId, setRestaurantId, setActor } =
    useTenant()

  return (
    <div className="fb-panel fb-panel-pad" style={{ marginBottom: '1rem' }}>
      <div className="fb-toolbar" style={{ marginBottom: 0 }}>
        <Input
          label="SubscriberId"
          type="number"
          min={1}
          value={subscriberId}
          onChange={(event) => setSubscriberId(Number(event.target.value) || 1)}
        />
        <Input
          label="RestaurantId"
          type="number"
          min={0}
          hint="خالی یا ۰ = سطح مشترک (subscriber-level)"
          value={restaurantId ?? ''}
          onChange={(event) => {
            const value = event.target.value
            if (!value) {
              setRestaurantId(null)
              return
            }
            const parsed = Number(value)
            setRestaurantId(!Number.isFinite(parsed) || parsed <= 0 ? null : parsed)
          }}
        />
        <Input
          label="Actor"
          value={actor}
          onChange={(event) => setActor(event.target.value)}
        />
      </div>
    </div>
  )
}
