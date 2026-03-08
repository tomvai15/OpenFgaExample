import { getAuthHeader } from './auth'

export type Project = {
  id: string
  name: string
  description?: string
}

const BASE = import.meta.env.VITE_API_BASE ?? ''

async function handleRes<T>(res: Response): Promise<T> {
  if (!res.ok) {
    const txt = await res.text()
    throw new Error(txt || res.statusText)
  }
  return (await res.json()) as T
}

function authHeaders() {
  return getAuthHeader()
}

export async function getProjects(): Promise<Project[]> {
  const res = await fetch(`${BASE}/api/projects`, { headers: { ...authHeaders() } })
  return handleRes<Project[]>(res)
}

export async function getProject(id: string): Promise<Project> {
  const res = await fetch(`${BASE}/api/projects/${id}`, { headers: { ...authHeaders() } })
  return handleRes<Project>(res)
}

export async function createProject(payload: { name: string; description?: string }): Promise<Project> {
  const res = await fetch(`${BASE}/api/projects`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify(payload),
  })
  return handleRes<Project>(res)
}

export async function updateProject(id: string, payload: { name?: string; description?: string }): Promise<Project> {
  const res = await fetch(`${BASE}/api/projects/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify(payload),
  })
  return handleRes<Project>(res)
}

export async function deleteProject(id: string): Promise<void> {
  const res = await fetch(`${BASE}/api/projects/${id}`, { method: 'DELETE', headers: { ...authHeaders() } })
  if (!res.ok) {
    const txt = await res.text()
    throw new Error(txt || res.statusText)
  }
}
