import React, { useEffect, useState } from 'react'
import { getUsers, generateToken, getToken, clearToken, subscribeTokenChange, type TestUserModel } from './auth'

function parseJwt(token: string | null) {
  if (!token) return null
  try {
    const payload = token.split('.')[1]
    const decoded = JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')))
    return decoded
  } catch {
    return null
  }
}

export default function UserSwitcher() {
  const [users, setUsers] = useState<TestUserModel[]>([])
  const [loading, setLoading] = useState(false)
  const [token, setToken] = useState<string | null>(getToken())
  const [selectedId, setSelectedId] = useState<string | null>(null)

  useEffect(() => {
    setLoading(true)
    getUsers().then(list => {
      setUsers(list)
      const cur = parseJwt(getToken())?.sub ?? null
      setSelectedId(cur ?? (list[0]?.id ?? null))
    }).catch(() => setUsers([])).finally(() => setLoading(false))

    const unsub = subscribeTokenChange(() => setToken(getToken()))
    return unsub
  }, [])

  const currentPayload = parseJwt(token)
  const currentUserId = currentPayload?.sub ?? null
  const currentUser = users.find(u => u.id === currentUserId) ?? null

  async function handleSelectChange(ev: React.ChangeEvent<HTMLSelectElement>) {
    const id = ev.target.value
    setSelectedId(id)
    try {
      await generateToken(id)
      setToken(getToken())
    } catch (err: any) {
      alert('Failed to switch: ' + (err?.message ?? err))
    }
  }

  function logout() {
    clearToken()
    setToken(null)
  }

  return (
    <div className="panel user-switcher">
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h3 style={{ margin: 0 }}>Auth</h3>
        <div className="muted">{token ? 'Token present' : 'No token'}</div>
      </div>

      <div style={{ marginTop: 8, display: 'flex', gap: 8, alignItems: 'center' }}>
        <div style={{ flex: 1 }}>
          <div className="muted">Current user</div>
          <div>
            {currentUser ? (
              <strong>{currentUser.name} <span className="muted">({currentUser.role})</span></strong>
            ) : (
              <span className="muted">None</span>
            )}
          </div>
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', gap: 6 }}>
          <select value={selectedId ?? ''} onChange={handleSelectChange} style={{ padding: 8, borderRadius: 6, border: '1px solid #e6edf3' }}>
            {users.map(u => (
              <option key={u.id} value={u.id}>{u.name} ({u.role})</option>
            ))}
          </select>
          <div style={{ display: 'flex', gap: 8 }}>
            <button className="btn ghost" onClick={logout}>Logout</button>
          </div>
        </div>
      </div>

      {loading && <div style={{ marginTop: 8 }}>Loading users...</div>}
    </div>
  )
}
