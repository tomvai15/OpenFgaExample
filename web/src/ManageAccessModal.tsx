import React, { useEffect, useState } from 'react'
import { Autocomplete, Avatar, Box, Button, Dialog, DialogActions, DialogContent, DialogTitle, List, ListItem, ListItemAvatar, ListItemText, MenuItem, Select, Stack, TextField, Typography, Snackbar } from '@mui/material'
import { getUsers, type TestUserModel } from './auth'
import { checkRelation, updateProjectRelations, getProjectRelations, type ProjectRelationItem } from './api'

export default function ManageAccessModal({ open, onClose, projectId, projectName }: { open: boolean; onClose: () => void; projectId: string; projectName?: string }) {
  const [users, setUsers] = useState<TestUserModel[]>([])
  const [loading, setLoading] = useState(false)
  const [members, setMembers] = useState<ProjectRelationItem[]>([])
  const [selectedUser, setSelectedUser] = useState<TestUserModel | null>(null)
  const [selectedRole, setSelectedRole] = useState<'Editor' | 'Viewer'>('Editor')
  const [snack, setSnack] = useState<{ open: boolean; msg: string }>({ open: false, msg: '' })

  useEffect(() => {
    if (!open) return
    setLoading(true)
    getUsers().then(list => setUsers(list)).catch(() => setUsers([])).finally(() => setLoading(false))
  }, [open])

  useEffect(() => {
    if (!open || !projectId) return
    const run = async () => {
      try {
        const rels = await getProjectRelations(projectId)
        setMembers(rels.relations)
      } catch {
        setMembers([])
      }
    }
    run()
  }, [open, projectId])

  async function handleAdd() {
    if (!selectedUser) return
    try {
      await updateProjectRelations(projectId, selectedRole, selectedUser.id)
      setSnack({ open: true, msg: `Added ${selectedRole.toLowerCase()} ${selectedUser.name}` })
      // refresh relations
      try {
        const rels = await getProjectRelations(projectId)
        setMembers(rels.relations)
      } catch {
        // ignore
      }
      setSelectedUser(null)
    } catch (e: any) {
      setSnack({ open: true, msg: `Failed to add: ${e?.message ?? e}` })
    }
  }

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>Manage access — {projectName}</DialogTitle>
      <DialogContent>
        <Typography variant="subtitle2" sx={{ mb: 1 }}>Members</Typography>
        <List dense>
          {members.map(m => (
            <ListItem key={m.id} divider>
              <ListItemAvatar>
                <Avatar>{m.displayName?.split(' ').map(s => s[0]).join('').slice(0,2) ?? m.id.slice(0,2)}</Avatar>
              </ListItemAvatar>
              <ListItemText primary={m.displayName} secondary={m.id} />
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Typography variant="caption" sx={{ px: 1, bgcolor: m.relationType === 'Editor' ? '#e8f0ff' : '#f1f5f9', borderRadius: 1 }}>{m.relationType}</Typography>
              </Box>
            </ListItem>
          ))}
        </List>

        <Typography variant="subtitle2" sx={{ mt: 2, mb: 1 }}>Add user</Typography>
        <Stack direction="row" spacing={1} alignItems="center">
          <Autocomplete
            sx={{ flex: 1 }}
            options={users}
            getOptionLabel={(u) => `${u.name} (${u.role})`}
            value={selectedUser}
            onChange={(_, v) => setSelectedUser(v)}
            renderInput={(params) => <TextField {...params} label="Select user" />}
            disableClearable={false}
          />
          <Select value={selectedRole} onChange={e => setSelectedRole(String(e.target.value) as any)} size="small">
            <MenuItem value="Editor">Editor</MenuItem>
            <MenuItem value="Viewer">Viewer</MenuItem>
          </Select>
          <Button variant="contained" onClick={handleAdd} disabled={!selectedUser}>Add</Button>
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>

      <Snackbar open={snack.open} autoHideDuration={2500} onClose={() => setSnack(s => ({ ...s, open: false }))} message={snack.msg} />
    </Dialog>
  )
}
