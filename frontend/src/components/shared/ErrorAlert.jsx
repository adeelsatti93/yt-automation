export default function ErrorAlert({ message, onRetry }) {
  if (!message) return null

  return (
    <div className="alert border-0 shadow-sm d-flex align-items-center p-3 mb-4" 
         role="alert" 
         style={{ background: '#fef2f2', border: '1px solid #fee2e2 !important', borderRadius: '12px', color: '#b91c1c' }}>
      <div className="rounded-circle d-flex align-items-center justify-content-center me-3" 
           style={{ background: 'rgba(185, 28, 28, 0.1)', width: '32px', height: '32px', flexShrink: 0 }}>
        <i className="bi bi-exclamation-triangle-fill"></i>
      </div>
      <div className="flex-grow-1 fw-semibold small">{message}</div>
      {onRetry && (
        <button className="btn btn-sm px-3 ms-3" 
                onClick={onRetry}
                style={{ background: 'white', color: '#b91c1c', border: '1px solid #fee2e2', borderRadius: '8px', fontWeight: '700', fontSize: '0.75rem' }}>
          <i className="bi bi-arrow-clockwise me-1"></i>RETRY
        </button>
      )}
    </div>
  )
}
