/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL?: string
  readonly VITE_DEFAULT_SUBSCRIBER_ID?: string
  readonly VITE_DEFAULT_RESTAURANT_ID?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
