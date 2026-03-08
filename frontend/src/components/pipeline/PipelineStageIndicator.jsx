export default function PipelineStageIndicator({ stage, isActive, isCompleted, label, icon }) {
  let iconColor = '#6b7280';
  let iconBg = 'rgba(107, 114, 128, 0.1)';
  let borderColor = 'rgba(107, 114, 128, 0.2)';
  let dotColor = '#4b5563';
  let labelColor = '#6b7280';
  let shadow = 'none';

  if (isCompleted) {
    iconColor = '#22c55e';
    iconBg = 'rgba(34, 197, 94, 0.15)';
    borderColor = 'rgba(34, 197, 94, 0.5)';
    dotColor = '#22c55e';
    labelColor = '#4ade80';
    shadow = '0 0 12px rgba(34, 197, 94, 0.2)';
  } else if (isActive) {
    iconColor = '#818cf8';
    iconBg = 'rgba(129, 140, 248, 0.15)';
    borderColor = 'rgba(129, 140, 248, 0.5)';
    dotColor = '#818cf8';
    labelColor = '#a5b4fc';
    shadow = '0 0 16px rgba(129, 140, 248, 0.25)';
  }

  return (
    <div className="d-flex flex-column align-items-center"
         style={{ minWidth: '100px', transition: 'all 0.4s ease' }}>

      <div
        className="rounded-circle d-flex align-items-center justify-content-center mb-3 position-relative"
        style={{
          width: '48px',
          height: '48px',
          fontSize: '1.25rem',
          backgroundColor: iconBg,
          border: `2px solid ${borderColor}`,
          color: iconColor,
          zIndex: 2,
          boxShadow: shadow
        }}
      >
        {isCompleted ? (
          <i className="bi bi-check-lg animate__animated animate__zoomIn"></i>
        ) : (
          <i className={`bi ${icon || 'bi-gear'} ${isActive ? 'animate-spin-slow' : ''}`}></i>
        )}

        <div className="position-absolute bottom-0 end-0 rounded-circle" style={{ padding: '2px' }}>
           <div className="rounded-circle" style={{ width: '8px', height: '8px', backgroundColor: dotColor }}></div>
        </div>
      </div>

      <span className="text-center font-headings fw-bold text-uppercase"
            style={{ fontSize: '0.625rem', color: labelColor, letterSpacing: '0.05em' }}>
        {label || stage}
      </span>
    </div>
  );
}
