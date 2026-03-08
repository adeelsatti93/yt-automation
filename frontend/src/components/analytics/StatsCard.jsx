import { formatNumber } from '../../utils/formatters';

export default function StatsCard({ title, value, icon = 'bi-bar-chart', variant = 'primary' }) {
  const variantMap = {
    primary: { color: '#4f46e5', bg: '#eef2ff', shadow: 'rgba(79, 70, 229, 0.1)' },
    success: { color: '#10b981', bg: '#ecfdf5', shadow: 'rgba(16, 185, 129, 0.1)' },
    warning: { color: '#f59e0b', bg: '#fffbeb', shadow: 'rgba(245, 158, 11, 0.1)' },
    info: { color: '#0ea5e9', bg: '#f0f9ff', shadow: 'rgba(14, 165, 233, 0.1)' },
    danger: { color: '#ef4444', bg: '#fef2f2', shadow: 'rgba(239, 68, 68, 0.1)' },
  };
  
  const v = variantMap[variant] || variantMap.primary;

  return (
    <div className="card h-100 border-0 overflow-hidden stats-card-white" 
         style={{ 
           background: '#ffffff', 
           border: '1px solid #e2e8f0 !important',
           boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.05), 0 2px 4px -2px rgba(0, 0, 0, 0.05)',
           transition: 'all 0.3s ease',
           cursor: 'default'
         }}
         onMouseOver={e => e.currentTarget.style.boxShadow = '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -4px rgba(0, 0, 0, 0.05)'}
         onMouseOut={e => e.currentTarget.style.boxShadow = '0 4px 6px -1px rgba(0, 0, 0, 0.05), 0 2px 4px -2px rgba(0, 0, 0, 0.05)'}
    >
      <div className="card-body p-4 position-relative overflow-hidden">
        {/* Extremely Subdued Watermark - Strictly Bottom Corner */}
        <div className="position-absolute end-0 bottom-0" 
             style={{ 
               transform: 'translate(10%, 10%)', 
               color: v.color, 
               opacity: 0.03, 
               zIndex: 0,
               pointerEvents: 'none'
             }}>
          <i className={`bi ${icon}`} style={{ fontSize: '3rem' }}></i>
        </div>

        <div className="d-flex align-items-center mb-1 position-relative" style={{ zIndex: 1 }}>
          <div className="rounded-circle d-flex align-items-center justify-content-center me-3" 
               style={{ backgroundColor: v.bg, color: v.color, width: '48px', height: '48px', fontSize: '1.4rem' }}>
            <i className={`bi ${icon}`}></i>
          </div>
          <div className="flex-grow-1">
            <h6 className="text-uppercase mb-0 ls-wide fw-extrabold opacity-40" style={{ fontSize: '0.6rem', color: '#1e293b' }}>{title}</h6>
            <div className="d-flex align-items-baseline gap-2">
              <h2 className="mb-0 font-headings fw-extrabold" style={{ fontSize: '2.25rem', color: '#0f172a', letterSpacing: '-0.04em' }}>
                {formatNumber(value)}
              </h2>
            </div>
          </div>
        </div>
        
        {/* Visual rhythm indicator */}
        <div className="mt-3 bg-slate-100 rounded-pill" style={{ height: '4px', backgroundColor: '#f1f5f9', overflow: 'hidden' }}>
          <div className="h-100 rounded-pill" style={{ width: '45%', background: v.color }}></div>
        </div>
      </div>
    </div>
  );
}
