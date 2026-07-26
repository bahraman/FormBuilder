import type { ApiProblem } from '@/types/api'

export class ApiError extends Error {
  readonly status: number
  readonly problem?: ApiProblem

  constructor(message: string, status: number, problem?: ApiProblem) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.problem = problem
  }
}

function getBaseUrl() {
  const configured = import.meta.env.VITE_API_BASE_URL as string | undefined
  return configured?.replace(/\/$/, '') ?? ''
}

function toCamelCaseKey(key: string): string {
  if (!key) return key
  return key.charAt(0).toLowerCase() + key.slice(1)
}

function normalizeKeys(value: unknown): unknown {
  if (Array.isArray(value)) {
    return value.map(normalizeKeys)
  }

  if (value && typeof value === 'object') {
    return Object.fromEntries(
      Object.entries(value as Record<string, unknown>).map(([key, nested]) => [
        toCamelCaseKey(key),
        normalizeKeys(nested),
      ]),
    )
  }

  return value
}

export async function apiRequest<T>(
  path: string,
  init: RequestInit = {},
): Promise<T> {
  const headers = new Headers(init.headers)
  if (init.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  const response = await fetch(`${getBaseUrl()}${path}`, {
    ...init,
    headers,
  })

  if (response.status === 204) {
    return undefined as T
  }

  const text = await response.text()
  const payload = text ? normalizeKeys(JSON.parse(text)) : undefined

  if (!response.ok) {
    const problem = payload as ApiProblem | undefined
    throw new ApiError(
      problem?.detail || problem?.title || `Request failed (${response.status})`,
      response.status,
      problem,
    )
  }

  return payload as T
}

export function buildQuery(params: Record<string, string | number | boolean | null | undefined>) {
  const search = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null || value === '') continue
    search.set(key, String(value))
  }
  const query = search.toString()
  return query ? `?${query}` : ''
}
