# Vendo Form Builder UI

Standalone React (Vite) UI for the Vendo-FormBuilder API.

This project is intentionally separate so it can be developed alone first, then embedded into **Vendo-designer** (`D:\Delino\Sources\Vendo-designer`).

## Stack

- React 19 + TypeScript
- Vite (not Next.js)
- React Router
- Thin local UI primitives in `src/ui/*` (Button, Input, Modal, …) ready to swap with Vendo-designer components

## Run locally

1. Start the FormBuilder API on `http://localhost:5000`
2. In this folder:

```bash
npm install
npm run dev
```

Open `http://localhost:5173`

Vite proxies `/api` → `http://localhost:5000`.

Optional env (copy `.env.example`):

```env
VITE_API_BASE_URL=
VITE_DEFAULT_SUBSCRIBER_ID=1
VITE_DEFAULT_RESTAURANT_ID=
```

## Features (v1)

- List / search / filter forms by tenant
- Create draft form
- Edit form metadata
- Add / edit / delete / reorder fields
- Options for selectable field types
- Live preview
- Publish / archive / create new version / delete

## Embed into Vendo-designer

Vendo-designer was not available in the cloud agent environment. Integration path:

1. Keep this folder as a sibling package, or copy/link `ui` into the designer monorepo.
2. Import the embed entry:

```tsx
import { FormBuilderApp } from '@vendo/form-builder-ui/embed'
// or relative: ../../FormBuilder/ui/src/embed

export function FormBuilderRoute() {
  return (
    <FormBuilderApp
      basename="/form-builder"
      subscriberId={currentSubscriberId}
      restaurantId={currentRestaurantId}
      actor={currentUserName}
    />
  )
}
```

3. Replace thin primitives under `src/ui/` with Vendo-designer components (same prop names where possible).
4. Map CSS variables in `src/styles/tokens.css` to Vendo design tokens (`[data-theme='vendo']`).

Suggested host route: `/form-builder/*`

## Folder map

```
src/
  FormBuilderApp.tsx   # embeddable app shell
  embed.ts             # public exports for host apps
  api/                 # FormBuilder REST client
  ui/                  # swappable design-system adapters
  pages/               # Forms list + editor
  features/            # create modal, field editor, preview
  context/             # tenant (subscriber/restaurant) state
```
