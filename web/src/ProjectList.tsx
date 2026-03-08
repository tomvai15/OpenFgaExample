import React, { useEffect, useState } from 'react'
import { type Project, getProjects, getProject, createProject, updateProject, deleteProject } from './api'

export default function ProjectList() {
  const [projects, setProjects] = useState<Project[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [newName, setNewName] = useState('')
  const [newDesc, setNewDesc] = useState('')

  async function load() {
    setLoading(true)
    setError(null)
    // clear previous items so UI doesn't show stale data while reloading
    setProjects([])
    try {
      const items = await getProjects()
      setProjects(items)
    } catch (e: any) {
      setError(e?.message || 'Failed to load')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

  async function handleCreate(e?: React.FormEvent) {
    e?.preventDefault()
    if (!newName.trim()) return
    try {
      const created = await createProject({ name: newName.trim(), description: newDesc.trim() || undefined })
      setNewName('')
      setNewDesc('')
      setProjects((p) => [created, ...p])
    } catch (err: any) {
      alert('Create failed: ' + (err?.message ?? err))
    }
  }

  async function handleView(id: string) {
    try {
      const p = await getProject(id)
      alert(JSON.stringify(p, null, 2))
    } catch (err: any) {
      alert('View failed: ' + (err?.message ?? err))
    }
  }

  async function handleEdit(id: string) {
    const newName = prompt('New name?')
    if (!newName) return
    try {
      const updated = await updateProject(id, { name: newName })
      setProjects((p) => p.map(x => x.id === id ? updated : x))
    } catch (err: any) {
      alert('Update failed: ' + (err?.message ?? err))
    }
  }

  async function handleDelete(id: string) {
    if (!confirm('Delete this project?')) return
    try {
      await deleteProject(id)
      setProjects((p) => p.filter(x => x.id !== id))
    } catch (err: any) {
      alert('Delete failed: ' + (err?.message ?? err))
    }
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h2 style={{ margin: 0 }}>Projects</h2>
        <div className="muted">{projects.length} total</div>
      </div>

      <div className="panel" style={{ marginTop: 12 }}>
        <form onSubmit={handleCreate} className="project-form">
          <input type="text" placeholder="Name" value={newName} onChange={e => setNewName(e.target.value)} />
          <input type="text" placeholder="Description" value={newDesc} onChange={e => setNewDesc(e.target.value)} />
          <button className="btn" type="submit">Create</button>
        </form>

        {loading && <div>Loading...</div>}
        {error && <div style={{ color: 'red' }}>{error}</div>}

        {!loading && projects.length === 0 && <div className="muted">No projects</div>}

        <ul className="project-list">
          {projects.map(p => (
            <li key={p.id} className="project-item">
              <div className="project-meta">
                <div className="project-name">{p.name}</div>
                {p.description && <div className="project-desc">{p.description}</div>}
              </div>

              <div className="project-actions">
                <button className="btn secondary" onClick={() => handleView(p.id)}>View</button>
                <button className="btn ghost" onClick={() => handleEdit(p.id)}>Edit</button>
                <button className="btn secondary" onClick={() => handleDelete(p.id)}>Delete</button>
              </div>
            </li>
          ))}
        </ul>

        <div style={{ marginTop: 12 }}>
          <button className="btn secondary" onClick={load}>Refresh</button>
        </div>
      </div>
    </div>
  )
}
