import axios from 'axios'
import { toast } from 'react-toastify'

const api = axios.create({
  baseURL: '/api',
  timeout: 120000,
  headers: { 'Content-Type': 'application/json' }
})

api.interceptors.response.use(
  response => response,
  error => {
    const message = error.response?.data?.detail || error.message || 'Something went wrong'
    toast.error(message)
    return Promise.reject(error)
  }
)

export default api
