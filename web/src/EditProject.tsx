import React, { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { Box, Button, CircularProgress, TextField, Typography } from '@mui/material'
import { getProject, updateProject } from './api'

export default function EditProject() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')

  useEffect(() => {
    if (!id) return
    setLoading(true)
    getProject(id).then(p => {
      setName(p.name)
      setDescription(p.description ?? '')
    }).catch(e => setError(e?.message ?? String(e))).finally(() => setLoading(false))
  }, [id])

  async function handleSave() {
    if (!id) return
    setSaving(true)
    try {
      await updateProject(id, { name: name.trim(), description: description.trim() || undefined })
      navigate('/')
    } catch (e: any) {
      setError(e?.message ?? String(e))
    } finally {
      setSaving(false)
    }
  }

  if (loading) return <Box sx={{ p: 2 }}><CircularProgress /></Box>

  return (
    <Box sx={{ p: 2 }}>
      <Typography variant="h6" sx={{ mb: 2 }}>Edit project</Typography>
      {error && <Typography color="error" sx={{ mb: 1 }}>{error}</Typography>}
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        <TextField label="Name" value={name} onChange={e => setName(e.target.value)} />
        <TextField label="Description" value={description} onChange={e => setDescription(e.target.value)} multiline rows={3} />
        <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
          <Button onClick={() => navigate('/')}>Cancel</Button>
          <Button variant="contained" onClick={handleSave} disabled={saving}>{saving ? 'Saving…' : 'Save'}</Button>
        </Box>
      </Box>
    </Box>
  )
}
