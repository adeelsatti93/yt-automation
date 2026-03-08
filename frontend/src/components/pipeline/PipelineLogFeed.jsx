import { useEffect, useRef } from 'react';
import { formatDateTime } from '../../utils/formatters';

const LEVEL_COLORS = {
  info: { text: '#4f46e5', bg: '#eef2ff', icon: 'bi-info-circle' },
  warning: { text: '#d97706', bg: '#fffbeb', icon: 'bi-exclamation-triangle' },
  error: { text: '#dc2626', bg: '#fef2f2', icon: 'bi-x-circle' },
  success: { text: '#059669', bg: '#ecfdf5', icon: 'bi-check-circle' },
};

export default function PipelineLogFeed({ logs = [] }) {
  const feedRef = useRef(null);

  useEffect(() => {
    if (feedRef.current) {
      feedRef.current.scrollTop = feedRef.current.scrollHeight;
    }
  }, [logs]);

  if (logs.length === 0) {
    return (
      <div className="card border-0 shadow-sm" style={{ background: '#f8fafc', border: '1px solid #e2e8f0 !important' }}>
        <div className="card-body text-center py-5">
          <i className="bi bi-journal-text d-block mb-3 display-4 text-slate-200" style={{ color: '#e2e8f0' }}></i>
          <span className="text-slate-400 fw-bold small text-uppercase ls-wide">Wait for system activity...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="card shadow-sm border-0" style={{ background: 'white', border: '1px solid #e2e8f0 !important' }}>
      <div className="card-header bg-transparent border-bottom border-slate-100 px-4 py-3 d-flex justify-content-between align-items-center">
        <h6 className="mb-0 font-headings text-uppercase small ls-wide fw-extrabold text-slate-500">
          <i className="bi bi-terminal me-2 text-indigo-500" style={{ color: '#4f46e5' }}></i>
          System Intelligence Logs
        </h6>
        <span className="badge bg-slate-50 text-slate-500 border border-slate-200">{logs.length} entries</span>
      </div>
      <div
        ref={feedRef}
        className="card-body p-0"
        style={{ maxHeight: '420px', overflowY: 'auto' }}
      >
        <div className="list-group list-group-flush">
          {logs.map((log, index) => {
            const config = LEVEL_COLORS[log.level] || LEVEL_COLORS.info;
            return (
              <div
                key={index}
                className="list-group-item border-bottom border-slate-50 px-4 py-3"
                style={{ fontSize: '0.875rem', backgroundColor: index % 2 === 0 ? 'white' : '#fbfcff' }}
              >
                <div className="d-flex align-items-start gap-3">
                  <div className="mt-1 rounded-circle p-1 d-flex align-items-center justify-content-center" 
                       style={{ background: config.bg, color: config.text, fontSize: '1rem', width: '28px', height: '28px' }}>
                    <i className={`bi ${config.icon}`}></i>
                  </div>
                  <div className="flex-grow-1">
                    <div className="mb-1 d-flex justify-content-between align-items-center">
                      <span className="fw-semibold text-slate-700">{log.message}</span>
                      {log.stage && (
                        <span className="badge bg-slate-100 text-slate-500 border-0 fw-bold" style={{ fontSize: '0.6rem' }}>
                          {log.stage}
                        </span>
                      )}
                    </div>
                    <div className="text-slate-400 smaller fw-medium">{formatDateTime(log.timestamp)}</div>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}
