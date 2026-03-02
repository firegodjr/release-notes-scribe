import type {
  ConfigStatusResponse,
  WorkItemQueryRequest,
  WorkItemQueryResponse,
  GenerateRequest,
  GenerateResponse
} from '../types'

const BASE = '/api'

async function fetchJson<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(url, {
    headers: { 'Content-Type': 'application/json' },
    ...options
  })
  if (!res.ok) {
    const text = await res.text()
    throw new Error(text || `HTTP ${res.status}`)
  }
  return res.json()
}

export const api = {
  getConfigStatus(): Promise<ConfigStatusResponse> {
    return fetchJson(`${BASE}/config/status`)
  },

  queryWorkItems(request: WorkItemQueryRequest): Promise<WorkItemQueryResponse> {
    return fetchJson(`${BASE}/workitems/query`, {
      method: 'POST',
      body: JSON.stringify(request)
    })
  },

  generate(request: GenerateRequest): Promise<GenerateResponse> {
    return fetchJson(`${BASE}/generate`, {
      method: 'POST',
      body: JSON.stringify(request)
    })
  }
}
