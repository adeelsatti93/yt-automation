import React from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap/dist/js/bootstrap.bundle.min.js'
import 'bootstrap-icons/font/bootstrap-icons.css'
import 'react-toastify/dist/ReactToastify.css'
import './styles/main.scss'
import { ToastContainer } from 'react-toastify'
import App from './App'
import { AppProvider } from './context/AppContext'
import { SettingsProvider } from './context/SettingsContext'
import { PipelineProvider } from './context/PipelineContext'

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <BrowserRouter>
      <AppProvider>
        <SettingsProvider>
          <PipelineProvider>
            <App />
            <ToastContainer position="top-right" autoClose={4000} />
          </PipelineProvider>
        </SettingsProvider>
      </AppProvider>
    </BrowserRouter>
  </React.StrictMode>
)
