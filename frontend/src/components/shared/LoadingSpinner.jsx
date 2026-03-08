export default function LoadingSpinner({ text = 'Preparing Environment...', size = '' }) {
  const isSmall = size === 'sm';
  
  return (
    <div className={`d-flex flex-column align-items-center justify-content-center ${isSmall ? 'py-2' : 'py-5'}`} style={{ minHeight: isSmall ? 'auto' : '200px' }}>
      <div 
        className={`spinner-border ${isSmall ? 'spinner-border-sm' : ''}`} 
        role="status"
        style={{ color: '#4f46e5', width: isSmall ? '1rem' : '3rem', height: isSmall ? '1rem' : '3rem', borderWidth: isSmall ? '2px' : '4px' }}
      >
        <span className="visually-hidden">{text}</span>
      </div>
      {!isSmall && (
        <span className="mt-4 text-slate-500 font-headings fw-bold animate__animated animate__pulse animate__infinite" 
              style={{ fontSize: '0.8125rem', letterSpacing: '0.1em', opacity: 0.7 }}>
          {text.toUpperCase()}
        </span>
      )}
    </div>
  )
}
