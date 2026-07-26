# Vendo-FormBuilder-Ui

Standalone **React + Vite** UI for the Vendo-FormBuilder API.

**Intended local path on your machine:**

```text
D:\Delino\Sources\Vendo-FormBuilder-Ui
```

Sibling projects:

| Path | Role |
|------|------|
| `D:\Delino\Sources\FormBuilder` | ASP.NET FormBuilder API |
| `D:\Delino\Sources\Vendo-designer` | Host app (embed this UI later) |
| `D:\Delino\Sources\Vendo-FormBuilder-Ui` | This UI (standalone first) |

> This cloud agent cannot write to your local `D:\` drive. Copy/clone the project there using the steps below.

## Create the folder on your PC (Windows)

### Option A — copy from FormBuilder repo (already pulled)

```powershell
New-Item -ItemType Directory -Force -Path "D:\Delino\Sources\Vendo-FormBuilder-Ui" | Out-Null
Copy-Item -Path "D:\Delino\Sources\FormBuilder\ui\*" -Destination "D:\Delino\Sources\Vendo-FormBuilder-Ui" -Recurse -Force
cd D:\Delino\Sources\Vendo-FormBuilder-Ui
npm install
npm run dev
```

If your FormBuilder clone path differs, adjust the source path (the UI currently lives in the FormBuilder repo under `ui/`).

### Option B — download the zip artifact

1. Download `Vendo-FormBuilder-Ui.zip` from the cloud agent artifacts.
2. Extract to `D:\Delino\Sources\Vendo-FormBuilder-Ui`
3. Run:

```powershell
cd D:\Delino\Sources\Vendo-FormBuilder-Ui
npm install
npm run dev
```

### Optional — make it its own Git repo

```powershell
cd D:\Delino\Sources\Vendo-FormBuilder-Ui
git init
git add .
git commit -m "chore: initial Vendo-FormBuilder-Ui"
# then create github.com/bahraman/Vendo-FormBuilder-Ui in the browser and:
# git remote add origin https://github.com/bahraman/Vendo-FormBuilder-Ui.git
# git push -u origin main
```

(The cloud GitHub token cannot create that repo for you.)

## Run

1. Start FormBuilder API on `http://localhost:5000`
2. In this project:

```bash
npm install
npm run dev
```

Open `http://localhost:5173` — Vite proxies `/api` → `localhost:5000`.

## Embed into Vendo-designer later

```tsx
import { FormBuilderApp } from 'D:/Delino/Sources/Vendo-FormBuilder-Ui/src/embed'
// or a workspace/package alias you configure

<FormBuilderApp
  basename="/form-builder"
  subscriberId={currentSubscriberId}
  restaurantId={currentRestaurantId}
  actor={currentUserName}
/>
```

Then replace thin primitives in `src/ui/*` with Vendo-designer components.

## Stack

- React 19 + TypeScript
- Vite (not Next.js)
- React Router
