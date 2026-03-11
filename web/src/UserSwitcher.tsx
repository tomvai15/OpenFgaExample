import React, { useEffect, useState } from 'react'
import { getUsers, generateToken, getToken, clearToken, subscribeTokenChange, type TestUserModel } from './auth'
import {
  Avatar,
  Box,
  Button,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  Snackbar,
  Typography,
} from '@mui/material'
import LogoutIcon from '@mui/icons-material/Logout'
import CloseIcon from '@mui/icons-material/Close'

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
  const [snackOpen, setSnackOpen] = useState(false)
  const [snackMsg, setSnackMsg] = useState('')
  const [snackSeverity, setSnackSeverity] = useState<'success' | 'error'>('success')
  const [confirmLogout, setConfirmLogout] = useState(false)

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

  async function handleSelectChange(ev: any) {
    const id = ev.target.value
    setSelectedId(id)
    try {
      await generateToken(id)
      setToken(getToken())
      const user = users.find(u => u.id === id)
      setSnackMsg(`Switched to ${user?.name ?? id}`)
      setSnackSeverity('success')
      setSnackOpen(true)
    } catch (err: any) {
      setSnackMsg('Failed to switch user')
      setSnackSeverity('error')
      setSnackOpen(true)
    }
  }

  function logout() {
    clearToken()
    setToken(null)
    setSnackMsg('Logged out')
    setSnackSeverity('success')
    setSnackOpen(true)
  }

  return (
    <Box className="panel user-switcher" sx={{ p: 2 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 2 }}>
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
          <Avatar>{currentUser?.name?.split(' ').map(s => s[0]).join('').slice(0,2) ?? 'U'}</Avatar>
          <Box>
            <Typography variant="subtitle1">{currentUser ? currentUser.name : 'No user'}</Typography>
            <Chip label={currentUser ? currentUser.role : 'guest'} size="small" sx={{ mt: 0.5 }} />
          </Box>
        </Box>

        <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
          <FormControl size="small" sx={{ minWidth: 200 }}>
            <InputLabel id="user-select-label">Switch user</InputLabel>
            <Select
              labelId="user-select-label"
              value={selectedId ?? ''}
              label="Switch user"
              onChange={handleSelectChange}
              disabled={loading || users.length === 0}
            >
              {users.map(u => (
                <MenuItem key={u.id} value={u.id}>{u.name} ({u.role})</MenuItem>
              ))}
            </Select>
          </FormControl>

          <Button variant="outlined" color="inherit" startIcon={<LogoutIcon />} onClick={() => setConfirmLogout(true)}>Logout</Button>
        </Box>
      </Box>

      {loading && <Box sx={{ mt: 1 }}><CircularProgress size={20} /></Box>}

      <Dialog open={confirmLogout} onClose={() => setConfirmLogout(false)}>
        <DialogTitle>Confirm logout</DialogTitle>
        <DialogContent>
          <DialogContentText>Are you sure you want to logout?</DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmLogout(false)}>Cancel</Button>
          <Button color="error" onClick={() => { setConfirmLogout(false); logout(); }}>Logout</Button>
        </DialogActions>
      </Dialog>

      <Snackbar open={snackOpen} autoHideDuration={2500} onClose={() => setSnackOpen(false)} message={snackMsg} action={(
        <IconButton size="small" aria-label="close" color="inherit" onClick={() => setSnackOpen(false)}>
          <CloseIcon fontSize="small" />
        </IconButton>
      )} />
    </Box>
  )
}
