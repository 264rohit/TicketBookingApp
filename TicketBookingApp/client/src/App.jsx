import React, { useState, useEffect } from 'react'
import {
  Container,
  TextField,
  Button,
  Typography,
  Box,
  Paper,
  List,
  ListItem,
  ListItemText,
  Divider,
  Snackbar,
  Alert,
  MenuItem,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Stack,
} from '@mui/material'
import EventSeatIcon from '@mui/icons-material/EventSeat'
import DeleteIcon from '@mui/icons-material/Delete'
import SearchIcon from '@mui/icons-material/Search'
import DownloadIcon from '@mui/icons-material/Download'
import LogoutIcon from '@mui/icons-material/Logout'

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false)
  const [loginEmail, setLoginEmail] = useState('')
  const [userEmail, setUserEmail] = useState(null)

  const [name, setName] = useState('')
  const [tickets, setTickets] = useState(1)
  const [ticketType, setTicketType] = useState('standardStag')
  const [phone, setPhone] = useState('')
  const [extraPerson, setExtraPerson] = useState(0)
  const [bookings, setBookings] = useState([])
  const [searchNumber, setSearchNumber] = useState('')
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' })
  const [deleteDialog, setDeleteDialog] = useState({ open: false, bookingNumber: null })

  const apiBase = import.meta.env.VITE_API_BASE || ''

  useEffect(() => {
    const token = localStorage.getItem('token')
    if (token) {
      verifyToken(token)
    }
  }, [])

  async function verifyToken(token) {
    try {
      const res = await fetch(`${apiBase}/api/auth/verify`, {
        headers: { 'Authorization': `Bearer ${token}` }
      })
      if (res.ok) {
        const data = await res.json()
        setUserEmail(data.email)
        setIsLoggedIn(true)
        fetchBookings(token)
      } else {
        localStorage.removeItem('token')
        setIsLoggedIn(false)
      }
    } catch (err) {
      console.error(err)
      localStorage.removeItem('token')
    }
  }

  async function handleLogin(e) {
    e.preventDefault()
    if (!loginEmail) {
      showSnackbar('Please enter your email', 'warning')
      return
    }

    try {
      const res = await fetch(`${apiBase}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: loginEmail })
      })

      if (res.ok) {
        const data = await res.json()
        localStorage.setItem('token', data.token)
        setUserEmail(data.email)
        setIsLoggedIn(true)
        setLoginEmail('')
        showSnackbar(`Welcome, ${data.email}!`)
        fetchBookings(data.token)
      } else {
        const error = await res.json()
        showSnackbar(error.message || 'Login failed', 'error')
      }
    } catch (err) {
      console.error(err)
      showSnackbar('Server not reachable', 'error')
    }
  }

  async function handleLogout() {
    localStorage.removeItem('token')
    setIsLoggedIn(false)
    setUserEmail(null)
    setBookings([])
    showSnackbar('Logged out successfully')
  }

  function getToken() {
    return localStorage.getItem('token')
  }

  async function fetchBookings(token = null) {
    token = token || getToken()
    if (!token) return

    try {
      const res = await fetch(`${apiBase}/api/bookings`, {
        headers: { 'Authorization': `Bearer ${token}` }
      })
      if (!res.ok) throw new Error('Failed to fetch')
      const data = await res.json()
      setBookings(data)
    } catch (err) {
      console.error(err)
      showSnackbar('Failed to load bookings', 'error')
    }
  }

  function showSnackbar(message, severity = 'success') {
    setSnackbar({ open: true, message, severity })
  }

  function ticketTypeLabel(value) {
    return (
      {
        standardStag: 'Standard Stag',
        premiumPlatinum: 'Premium Platinum',
        titaniumTable: 'Titanium Table',
        titaniumTable10: 'Titanium Table 10',
        premiumTitaniumTable: 'Premium Titanium Table'
      }[String(value)] || value
    )
  }

  async function submit(e) {
    e.preventDefault()
    const token = getToken()
    if (!token) {
      showSnackbar('You must be logged in', 'warning')
      return
    }

    if (!name || tickets <= 0) {
      showSnackbar('Please enter valid details', 'warning')
      return
    }

    try {
      const res = await fetch(`${apiBase}/api/bookings`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
        body: JSON.stringify({ name, numberOfTickets: parseInt(tickets), ticketType, phoneNumber: phone, extraPerson: parseInt(extraPerson) })
      })
      if (res.ok) {
        const newBooking = await res.json()
        setBookings([newBooking, ...bookings])
        setName('')
        setTickets(1)
        setTicketType('standardStag')
        setPhone('')
        setExtraPerson(0)
        showSnackbar('Ticket booked successfully!')
      } else {
        const errorText = await res.text()
        showSnackbar(`Error: ${errorText}`, 'error')
      }
    } catch (err) {
      console.error(err)
      showSnackbar('Server not reachable', 'error')
    }
  }

  const openDeleteDialog = (bookingNumber) => setDeleteDialog({ open: true, bookingNumber })
  const closeDeleteDialog = () => setDeleteDialog({ open: false, bookingNumber: null })

  const confirmDelete = async () => {
    const token = getToken()
    try {
      const res = await fetch(`${apiBase}/api/bookings/by-number/${deleteDialog.bookingNumber}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${token}` }
      })
      if (res.ok || res.status === 204) {
        setBookings(bookings.filter(b => b.bookingNumber !== deleteDialog.bookingNumber))
        showSnackbar('Booking deleted successfully!')
      } else {
        const errorText = await res.text()
        showSnackbar(`Error: ${errorText}`, 'error')
      }
    } catch (err) {
      console.error(err)
      showSnackbar('Server not reachable', 'error')
    } finally {
      closeDeleteDialog()
    }
  }

  const searchBooking = async () => {
    const token = getToken()
    if (!searchNumber.trim()) {
      fetchBookings(token)
      return
    }

    try {
      const res = await fetch(`${apiBase}/api/bookings/by-number/${searchNumber}`, {
        headers: { 'Authorization': `Bearer ${token}` }
      })
      if (res.ok) {
        const booking = await res.json()
        setBookings([booking])
        showSnackbar('Booking found!')
      } else if (res.status === 404) {
        showSnackbar('Booking not found', 'warning')
        setBookings([])
      } else {
        showSnackbar('Error searching booking', 'error')
      }
    } catch (err) {
      console.error(err)
      showSnackbar('Server not reachable', 'error')
    }
  }

  const downloadExcel = async () => {
    const token = getToken()
    try {
      const res = await fetch(`${apiBase}/api/bookings/export/excel`, {
        headers: { 'Authorization': `Bearer ${token}` }
      })
      if (!res.ok) throw new Error('Failed to download')
      const blob = await res.blob()
      const disposition = res.headers.get('content-disposition') || ''
      let filename = `bookings_${new Date().toISOString().slice(0,19).replace(/[:T]/g,'')}.xlsx`
      if (disposition) {
        const fnMatch = disposition.match(/filename\*=UTF-8''([^;]+)/i) || disposition.match(/filename=\"?([^\";]+)\"?/i)
        if (fnMatch && fnMatch[1]) {
          try {
            filename = decodeURIComponent(fnMatch[1])
          } catch {
            filename = fnMatch[1].replace(/['\"]/g,'')
          }
        }
      }
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = filename
      document.body.appendChild(a)
      a.click()
      a.remove()
      URL.revokeObjectURL(url)
      showSnackbar('Excel downloaded successfully!')
    } catch (err) {
      console.error(err)
      showSnackbar('Failed to download Excel', 'error')
    }
  }

  // Login Screen
  if (!isLoggedIn) {
    return (
      <Container maxWidth="sm" sx={{ mt: 10 }}>
        <Paper elevation={4} sx={{ p: 4, borderRadius: 3, textAlign: 'center' }}>
          <EventSeatIcon color="primary" fontSize="large" sx={{ mb: 2 }} />
          <Typography variant="h4" fontWeight="bold" sx={{ mb: 3 }}>
            Ticket Booking
          </Typography>
          <Typography variant="body2" color="textSecondary" sx={{ mb: 3 }}>
            Login with your authorized email to access the app
          </Typography>

          <Box component="form" onSubmit={handleLogin}>
            <TextField
              label="Email Address"
              variant="outlined"
              type="email"
              fullWidth
              value={loginEmail}
              onChange={(e) => setLoginEmail(e.target.value)}
              sx={{ mb: 2 }}
              placeholder="imrohitchaudhari55@gmail.com"
            />
            <Button variant="contained" color="primary" type="submit" fullWidth>
              Login
            </Button>
          </Box>

          <Typography variant="caption" color="textSecondary" sx={{ mt: 2, display: 'block' }}>
            Authorized emails: imrohitchaudhari55@gmail.com, rakhpasaremangesh33@gmail.com
          </Typography>
        </Paper>
      </Container>
    )
  }

  // Main App Screen
  return (
    <Container maxWidth="sm" sx={{ mt: 6, mb: 4 }}>
      <Paper elevation={4} sx={{ p: 4, borderRadius: 3 }}>
        <Box display="flex" alignItems="center" justifyContent="space-between" mb={2}>
          <Box display="flex" alignItems="center">
            <EventSeatIcon color="primary" fontSize="large" sx={{ mr: 1 }} />
            <Typography variant="h5" fontWeight="bold">Ticket Booking</Typography>
          </Box>
          <Box display="flex" alignItems="center" gap={1}>
            <Typography variant="body2">{userEmail}</Typography>
            <IconButton size="small" color="error" onClick={handleLogout} title="Logout">
              <LogoutIcon />
            </IconButton>
          </Box>
        </Box>

        <Box component="form" onSubmit={submit} mb={3}>
          <TextField label="Name" variant="outlined" fullWidth value={name} onChange={(e)=>setName(e.target.value)} sx={{ mb: 2 }} />
          <TextField label="Phone Number" variant="outlined" fullWidth type="tel" value={phone} onChange={(e)=>setPhone(e.target.value)} sx={{ mb: 2 }} />
          <TextField label="Number of Tickets" variant="outlined" type="number" fullWidth inputProps={{ min: 1 }} value={tickets} onChange={(e)=>setTickets(e.target.value)} sx={{ mb: 2 }} />
          <TextField select label="Ticket Type" fullWidth value={ticketType} onChange={(e)=>setTicketType(e.target.value)} sx={{ mb: 2 }}>
            <MenuItem value="standardStag">Standard Stag</MenuItem>
            <MenuItem value="premiumPlatinum">Premium Platinum</MenuItem>
            <MenuItem value="titaniumTable">Titanium Table</MenuItem>
            <MenuItem value="titaniumTable10">Titanium Table 10</MenuItem>
            <MenuItem value="premiumTitaniumTable">Premium Titanium Table</MenuItem>
          </TextField>
          <TextField label="Extra Person" variant="outlined" type="number" fullWidth inputProps={{ min: 0 }} value={extraPerson} onChange={(e)=>setExtraPerson(e.target.value)} sx={{ mb: 2 }} />
          <Button variant="contained" color="primary" type="submit" fullWidth>Book Ticket</Button>
        </Box>

        <Typography variant="h6" gutterBottom>Manage Bookings</Typography>
        <Stack direction="row" spacing={2} mb={3}>
          <TextField label="Enter Booking Number" variant="outlined" fullWidth value={searchNumber} onChange={(e)=>setSearchNumber(e.target.value)} />
          <Button variant="contained" color="secondary" startIcon={<SearchIcon />} onClick={searchBooking}>Search</Button>
          <Button variant="contained" color="success" startIcon={<DownloadIcon />} onClick={downloadExcel}>Excel</Button>
        </Stack>

        <Typography variant="h6" gutterBottom>All Bookings</Typography>
        <Paper variant="outlined" sx={{ maxHeight: 300, overflow: 'auto' }}>
          <List>
            {bookings.length === 0 && <ListItem><ListItemText primary="No bookings yet" /></ListItem>}
            {bookings.map((b, idx) => (
              <React.Fragment key={b.id || idx}>
                <ListItem secondaryAction={<IconButton edge="end" color="error" onClick={()=>openDeleteDialog(b.bookingNumber)}><DeleteIcon /></IconButton>}>
                  <ListItemText primary={`${b.name} (${ticketTypeLabel(b.ticketType)}) booked ${b.numberOfTickets} ticket(s)${b.extraPerson > 0 ? ` + ${b.extraPerson} extra` : ''}`} secondary={`Booking #: ${b.bookingNumber} | ${b.phoneNumber ? 'Phone: ' + b.phoneNumber + ' | ' : ''}${new Date(b.bookingDate).toLocaleString()}`} />
                </ListItem>
                <Divider />
              </React.Fragment>
            ))}
          </List>
        </Paper>
      </Paper>

      <Dialog open={deleteDialog.open} onClose={closeDeleteDialog}>
        <DialogTitle>Confirm Delete</DialogTitle>
        <DialogContent>
          <DialogContentText>Are you sure you want to delete booking #{deleteDialog.bookingNumber}?</DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={closeDeleteDialog}>Cancel</Button>
          <Button color="error" onClick={confirmDelete}>Delete</Button>
        </DialogActions>
      </Dialog>

      <Snackbar open={snackbar.open} autoHideDuration={4000} onClose={()=>setSnackbar({...snackbar, open:false})}>
        <Alert onClose={()=>setSnackbar({...snackbar, open:false})} severity={snackbar.severity} sx={{ width: '100%' }}>{snackbar.message}</Alert>
      </Snackbar>
    </Container>
  )
}

export default App
