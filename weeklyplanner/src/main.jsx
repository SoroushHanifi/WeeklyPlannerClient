import React from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import App from './App.jsx'
import { AuthProvider } from './contexts/AuthContext.jsx'
import './styles/global.css'

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <BrowserRouter>
      <AuthProvider>
        <App />
        <Toaster
          position="top-center"
          toastOptions={{
            style: {
              background: '#1a1d2b',
              color: '#e8eaf0',
              border: '1px solid rgba(255,255,255,0.07)',
              borderRadius: '10px',
              fontFamily: 'Vazirmatn, sans-serif',
              fontSize: '14px',
              direction: 'rtl',
            },
            success: {
              iconTheme: { primary: '#4ade80', secondary: '#1a1d2b' },
            },
            error: {
              iconTheme: { primary: '#f87171', secondary: '#1a1d2b' },
            },
          }}
        />
      </AuthProvider>
    </BrowserRouter>
  </React.StrictMode>
)
