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

function App() {
  const [name, setName] = useState('')
  const [tickets, setTickets] = useState(1)
  const [ticketType, setTicketType] = useState('Stag')
  const [bookings, setBookings] = useState([])
  const [searchNumber, setSearchNumber] = useState('')
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' })
  const [deleteDialog, setDeleteDialog] = useState({ open: false, bookingNumber: null })

  const apiBase = import.meta.env.VITE_API_BASE || 'http://localhost:5021'

  useEffect(() => { fetchBookings() }, [])

  async function fetchBookings() {
    try {
      const res = await fetch(`${apiBase}/api/bookings`)
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

  async function submit(e) {
    e.preventDefault()
    if (!name || tickets <= 0) { showSnackbar('Please enter valid details', 'warning'); return }
    try {
      const res = await fetch(`${apiBase}/api/bookings`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, numberOfTickets: parseInt(tickets), ticketType })
      })
      if (res.ok) {
        const newBooking = await res.json(); setBookings([newBooking, ...bookings]);
        setName(''); setTickets(1); setTicketType('Stag'); showSnackbar('Ticket booked successfully!')
      } else { const errorText = await res.text(); showSnackbar(`Error: ${errorText}`, 'error') }
    } catch (err) { console.error(err); showSnackbar('Server not reachable', 'error') }
  }

  const openDeleteDialog = (bookingNumber) => setDeleteDialog({ open: true, bookingNumber })
  const closeDeleteDialog = () => setDeleteDialog({ open: false, bookingNumber: null })

  const confirmDelete = async () => {
    try {
      const res = await fetch(`${apiBase}/api/bookings/by-number/${deleteDialog.bookingNumber}`, { method: 'DELETE' })
      if (res.ok || res.status === 204) { setBookings(bookings.filter(b => b.bookingNumber !== deleteDialog.bookingNumber)); showSnackbar('Booking deleted successfully!') }
      else { const errorText = await res.text(); showSnackbar(`Error: ${errorText}`, 'error') }
    } catch (err) { console.error(err); showSnackbar('Server not reachable', 'error') } finally { closeDeleteDialog() }
  }

  const searchBooking = async () => {
    if (!searchNumber.trim()) { fetchBookings(); return }
    try {
      const res = await fetch(`${apiBase}/api/bookings/by-number/${searchNumber}`)
      if (res.ok) { const booking = await res.json(); setBookings([booking]); showSnackbar('Booking found!') }
      else if (res.status === 404) { showSnackbar('Booking not found', 'warning'); setBookings([]) }
      else showSnackbar('Error searching booking', 'error')
    } catch (err) { console.error(err); showSnackbar('Server not reachable', 'error') }
  }

  const downloadExcel = async () => {
    try {
      const res = await fetch(`${apiBase}/api/bookings/export/excel`)
      if (!res.ok) throw new Error('Failed to download')
      const blob = await res.blob()
      const disposition = res.headers.get('content-disposition') || ''
      let filename = `bookings_${new Date().toISOString().slice(0,19).replace(/[:T]/g,'')}.xlsx`
      if (disposition) {
        const fnMatch = disposition.match(/filename\*=UTF-8''([^;]+)/i) || disposition.match(/filename=\"?([^\";]+)\"?/i)
        if (fnMatch && fnMatch[1]) { try { filename = decodeURIComponent(fnMatch[1]) } catch { filename = fnMatch[1].replace(/['"]/g,'') } }
      }
      const url = URL.createObjectURL(blob); const a = document.createElement('a'); a.href = url; a.download = filename; document.body.appendChild(a); a.click(); a.remove(); URL.revokeObjectURL(url); showSnackbar('Excel downloaded successfully!')
    } catch (err) { console.error(err); showSnackbar('Failed to download Excel', 'error') }
  }

  return (
    <Container maxWidth="sm" sx={{ mt: 6 }}>
      <Paper elevation={4} sx={{ p: 4, borderRadius: 3 }}>
        <Box display="flex" alignItems="center" justifyContent="center" mb={2}>
          <EventSeatIcon color="primary" fontSize="large" sx={{ mr: 1 }} />
          <Typography variant="h5" fontWeight="bold">Ticket Booking</Typography>
        </Box>

        <Box component="form" onSubmit={submit} mb={3}>
          <TextField label="Name" variant="outlined" fullWidth value={name} onChange={(e)=>setName(e.target.value)} sx={{ mb: 2 }} />
          <TextField label="Number of Tickets" variant="outlined" type="number" fullWidth inputProps={{ min: 1 }} value={tickets} onChange={(e)=>setTickets(e.target.value)} sx={{ mb: 2 }} />
          <TextField select label="Ticket Type" fullWidth value={ticketType} onChange={(e)=>setTicketType(e.target.value)} sx={{ mb: 2 }}>
            <MenuItem value="Stag">Stag</MenuItem>
            <MenuItem value="Silver">Silver</MenuItem>
            <MenuItem value="Gold">Gold</MenuItem>
            <MenuItem value="Platinum">Platinum</MenuItem>
          </TextField>
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
                  <ListItemText primary={`${b.name} (${b.ticketType}) booked ${b.numberOfTickets} ticket(s)`} secondary={`Booking #: ${b.bookingNumber} | ${new Date(b.bookingDate).toLocaleString()}`} />
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
