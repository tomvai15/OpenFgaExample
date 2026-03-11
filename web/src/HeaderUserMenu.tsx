import React, { useEffect, useState } from 'react'
import { getUsers, generateToken, getToken, clearToken, type TestUserModel } from './auth'
import { Avatar, IconButton, Menu, MenuItem, ListItemText, CircularProgress, Typography, Divider } from '@mui/material'
import ArrowDropDownIcon from '@mui/icons-material/ArrowDropDown'

export default function HeaderUserMenu() {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const open = Boolean(anchorEl)
  const [users, setUsers] = useState<TestUserModel[]>([])
  const [loading, setLoading] = useState(false)
  const token = getToken()

  useEffect(() => {
    setLoading(true)
    getUsers().then(list => setUsers(list)).catch(() => setUsers([])).finally(() => setLoading(false))
  }, [])

  const currentPayload = token ? (() => {
    try {
      const payload = token.split('.')[1]
      return JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')))
    } catch { return null }
  })() : null
  const currentUserId = currentPayload?.sub ?? null
  const currentUser = users.find(u => u.id === currentUserId) ?? null

  function handleOpen(e: React.MouseEvent<HTMLElement>) { setAnchorEl(e.currentTarget) }
  function handleClose() { setAnchorEl(null) }

  async function handleSelectUser(id: string) {
    try {
      await generateToken(id)
      // refresh to apply new token app-wide
      window.location.reload()
    } catch {
      // on failure, just close menu
      handleClose()
    }
  }

  function handleLogout() {
    clearToken()
    window.location.reload()
  }

  return (
    <>
      <IconButton size="small" onClick={handleOpen} aria-controls={open ? 'user-menu' : undefined} aria-haspopup="true">
        <Avatar sx={{ width: 32, height: 32 }}>{currentUser?.name?.split(' ').map(s => s[0]).join('').slice(0,2) ?? 'U'}</Avatar>
        <ArrowDropDownIcon />
      </IconButton>

      <Menu id="user-menu" anchorEl={anchorEl} open={open} onClose={handleClose} anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }} transformOrigin={{ vertical: 'top', horizontal: 'right' }}>
        <MenuItem disabled>
          <ListItemText primary={currentUser ? currentUser.name : 'Not signed in'} secondary={currentUser ? currentUser.role : ''} />
        </MenuItem>
        <Divider />
        {loading && (
          <MenuItem>
            <CircularProgress size={18} />
            <Typography sx={{ ml: 1 }}>Loading users…</Typography>
          </MenuItem>
        )}
        {!loading && users.map(u => (
          <MenuItem key={u.id} onClick={() => handleSelectUser(u.id)} selected={u.id === currentUserId}>
            <ListItemText primary={u.name} secondary={u.role} />
          </MenuItem>
        ))}
        <Divider />
        <MenuItem onClick={handleLogout}>
          <ListItemText primary="Logout" />
        </MenuItem>
      </Menu>
    </>
  )
}
