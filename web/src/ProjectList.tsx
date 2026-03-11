import React, { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { type Project, getProjects, getProject, createProject, deleteProject, checkRelation } from './api'
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  List,
  ListItem,
  ListItemText,
  Stack,
  TextField,
  Tooltip,
  Typography,
  
} from '@mui/material'
import CloseIcon from '@mui/icons-material/Close'
import EditIcon from '@mui/icons-material/Edit'
import DeleteIcon from '@mui/icons-material/Delete'
import VisibilityIcon from '@mui/icons-material/Visibility'
import ManageAccessModal from './ManageAccessModal'

export default function ProjectList() {
  const [projects, setProjects] = useState<Project[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [newName, setNewName] = useState('')
  const [newDesc, setNewDesc] = useState('')
  const [createAllowed, setCreateAllowed] = useState<boolean | null>(null)
  const [perms, setPerms] = useState<Record<string, { canEdit: boolean; canDelete: boolean }>>({})
  const [selected, setSelected] = useState<Project | null>(null)
  const [manageOpen, setManageOpen] = useState(false)

  async function load() {
    setLoading(true)
    setError(null)
    // clear previous items so UI doesn't show stale data while reloading
    setProjects([])
    setPerms({})
    try {
      const items = await getProjects()
      setProjects(items)
      // fetch per-project permissions in parallel
      const permPromises = items.map(async (it) => {
        const [canEdit, canDelete] = await Promise.all([
          checkRelation('Project', it.id, 'CanEdit').catch(() => false),
          checkRelation('Project', it.id, 'CanDelete').catch(() => false),
        ])
        return { id: it.id, canEdit, canDelete }
      })
      const permResults = await Promise.all(permPromises)
      const map: Record<string, { canEdit: boolean; canDelete: boolean }> = {}
      permResults.forEach(r => { map[r.id] = { canEdit: r.canEdit, canDelete: r.canDelete } })
      setPerms(map)
      // check create permission (best-effort). Adjust resourceId if your org id differs.
      try {
        const createOk = await checkRelation('Organization', 'default', 'CanCreate')
        setCreateAllowed(createOk)
      } catch {
        setCreateAllowed(null)
      }
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
        setSelected(p)
    } catch (err: any) {
      alert('View failed: ' + (err?.message ?? err))
    }
  }

  function closeModal() {
    setSelected(null)
  }

  const navigate = useNavigate()

  function handleEdit(id: string) {
    navigate(`/projects/${id}/edit`)
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
        <Box component="form" onSubmit={handleCreate} sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
          <TextField size="small" placeholder="Name" value={newName} onChange={e => setNewName(e.target.value)} />
          <TextField size="small" placeholder="Description" value={newDesc} onChange={e => setNewDesc(e.target.value)} />
          <Button type="submit" variant="contained" disabled={createAllowed === false} title={createAllowed === false ? 'You do not have permission to create projects' : ''}>Create</Button>
        </Box>

        {loading && <div>Loading...</div>}
        {error && <div style={{ color: 'red' }}>{error}</div>}

        {!loading && projects.length === 0 && <div className="muted">No projects</div>}

        <List>
          {projects.map(p => (
            <ListItem
              key={p.id}
              divider
              secondaryAction={(
                <Stack direction="row" spacing={1}>
                  <Tooltip title="View">
                    <IconButton edge="end" onClick={() => handleView(p.id)}>
                      <VisibilityIcon />
                    </IconButton>
                  </Tooltip>

                  <Tooltip title={perms[p.id]?.canEdit === false ? 'You do not have permission to edit this project' : 'Edit'}>
                    <span>
                      <IconButton edge="end" onClick={() => handleEdit(p.id)} disabled={perms[p.id]?.canEdit === false}>
                        <EditIcon />
                      </IconButton>
                    </span>
                  </Tooltip>

                  <Tooltip title={perms[p.id]?.canDelete === false ? 'You do not have permission to delete this project' : 'Delete'}>
                    <span>
                      <IconButton edge="end" onClick={() => handleDelete(p.id)} disabled={perms[p.id]?.canDelete === false}>
                        <DeleteIcon />
                      </IconButton>
                    </span>
                  </Tooltip>
                </Stack>
              )}
            >
              <ListItemText primary={p.name} secondary={p.description} />
            </ListItem>
          ))}
        </List>

        <Box sx={{ marginTop: 2 }}>
          <Button variant="outlined" onClick={load}>Refresh</Button>
        </Box>
      </div>
      {/* Dialog for project details */}
      <Dialog open={!!selected} onClose={closeModal} fullWidth maxWidth="md">
        <DialogTitle sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          {selected?.name}
          <IconButton onClick={closeModal}><CloseIcon /></IconButton>
        </DialogTitle>
        <DialogContent>
          <Box sx={{ mb: 1 }}><strong>ID:</strong> <span className="muted">{selected?.id}</span></Box>
          {selected?.description && <Box sx={{ mb: 1 }}><strong>Description:</strong> <div>{selected.description}</div></Box>}
          <pre style={{ background: '#f1f5f9', padding: 8, borderRadius: 6 }}>{JSON.stringify(selected, null, 2)}</pre>
          <Box sx={{ mt: 2 }}>
            <Button variant="outlined" onClick={() => setManageOpen(true)}>Manage access</Button>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={closeModal}>Close</Button>
        </DialogActions>
      </Dialog>
      <ManageAccessModal open={manageOpen} onClose={() => setManageOpen(false)} projectId={selected?.id ?? ''} projectName={selected?.name} />
    </div>
  )
}
