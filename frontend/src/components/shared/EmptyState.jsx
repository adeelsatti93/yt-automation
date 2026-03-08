export default function EmptyState({ icon = 'bi-inbox', title = 'No data available', message = '', action, actionLabel = 'Create New' }) {
  return (
    <div className="text-center py-5">
      <div className="rounded-circle d-flex align-items-center justify-content-center mx-auto mb-4" 
           style={{ background: '#f8fafc', width: '96px', height: '96px', border: '2px solid #f1f5f9' }}>
        <i className={`bi ${icon} display-4`} style={{ color: '#cbd5e1' }}></i>
      </div>
      <h4 className="mb-2 text-slate-900 font-headings fw-extrabold" style={{ fontSize: '1.25rem' }}>{title}</h4>
      {message && <p className="text-slate-400 mx-auto fw-medium small mb-4" style={{ maxWidth: '400px', lineHeight: '1.6' }}>{message}</p>}
      {action && (
        <button className="btn btn-primary px-4 py-2" onClick={action} style={{ borderRadius: '10px', fontWeight: '700' }}>
          <i className="bi bi-plus-lg me-2"></i>{actionLabel.toUpperCase()}
        </button>
      )}
    </div>
  )
}
