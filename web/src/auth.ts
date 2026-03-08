export type TestUserModel = {
  id: string
  name: string
  role: string
}

type TokenResponse = {
  token: string
}

const BASE = import.meta.env.VITE_API_BASE ?? ''
const TOKEN_KEY = 'auth_token'

let listeners: Array<() => void> = []

export function getUsers(): Promise<TestUserModel[]> {
  return fetch(`${BASE}/auth/users`).then(async (res) => {
    if (!res.ok) throw new Error(await res.text() || res.statusText)
    return res.json()
  })
}

export async function generateToken(userId: string): Promise<string> {
  const res = await fetch(`${BASE}/auth/token/${encodeURIComponent(userId)}`, { method: 'POST' })
  if (!res.ok) throw new Error(await res.text() || res.statusText)
  const body = (await res.json()) as TokenResponse
  setToken(body.token)
  return body.token
}

export function setToken(token: string | null) {
  if (token) localStorage.setItem(TOKEN_KEY, token)
  else localStorage.removeItem(TOKEN_KEY)
  listeners.forEach(l => l())
}

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function clearToken() {
  setToken(null)
}

export function getAuthHeader(): Record<string, string> {
  const t = getToken()
  return t ? { Authorization: `Bearer ${t}` } : {}
}

export function subscribeTokenChange(cb: () => void) {
  listeners.push(cb)
  return () => {
    listeners = listeners.filter(x => x !== cb)
  }
}
