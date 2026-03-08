export default function SettingsSection({ title, icon, children }) {
  return (
    <div className="card shadow-sm mb-4 border-0" 
         style={{ background: 'white', border: '1px solid #e2e8f0 !important', borderRadius: '1.25rem' }}>
      {title && (
        <div className="card-header bg-transparent px-4 py-3 pb-0 border-0">
          <h6 className="mb-0 font-headings fw-bold text-slate-800 text-uppercase ls-wide" style={{ fontSize: '0.75rem', color: '#1e293b' }}>
            {icon && <i className={`bi ${icon} me-2 text-indigo-500`} style={{ color: '#6366f1' }}></i>}
            {title}
          </h6>
        </div>
      )}
      <div className="card-body p-4">
        {children}
      </div>
    </div>
  );
}
