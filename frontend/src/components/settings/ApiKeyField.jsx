import { useState, useEffect } from 'react';

export default function ApiKeyField({ label, settingKey, value, description, helpUrl, onSave, testResult, onTest, testing, saving }) {
  const [inputValue, setInputValue] = useState(value || '');
  const [showValue, setShowValue] = useState(false);
  const [dirty, setDirty] = useState(false);

  useEffect(() => {
    setInputValue(value || '');
    setDirty(false);
  }, [value]);

  const handleChange = (e) => {
    setInputValue(e.target.value);
    setDirty(true);
  };

  const handleSave = async () => {
    await onSave(settingKey, inputValue);
    setDirty(false);
  };

  return (
    <div className="card mb-4 border-0 shadow-sm" style={{ background: 'white', border: '1px solid #e2e8f0 !important', borderRadius: '1rem' }}>
      <div className="card-body p-4">
        <div className="d-flex justify-content-between align-items-start mb-4">
          <div>
            <h6 className="mb-1 font-headings fw-bold text-slate-900" style={{ fontSize: '0.95rem' }}>{label}</h6>
            {description && <small className="text-slate-400 fw-medium">{description}</small>}
          </div>
          {testResult && (
            <span className={`badge ${testResult.success ? 'bg-success' : 'bg-danger'} px-3 py-1`} style={{ borderRadius: '8px' }}>
              {testResult.success ? '✅ Operational' : '❌ Connection Failed'}
            </span>
          )}
        </div>

        <div className="input-group mb-3" style={{ boxShadow: '0 1px 2px rgba(0,0,0,0.05)', borderRadius: '0.75rem', overflow: 'hidden', border: '1px solid #e2e8f0' }}>
          <input
            type={showValue ? 'text' : 'password'}
            className="form-control border-0 px-3"
            value={inputValue}
            onChange={handleChange}
            placeholder="Enter API key..."
            disabled={saving}
            style={{ height: '48px', background: 'white !important' }}
          />
          <button
            className="btn btn-white border-0 border-start rounded-0 px-3"
            type="button"
            onClick={() => setShowValue(!showValue)}
            title={showValue ? 'Hide' : 'Show'}
            style={{ background: '#f8fafc', color: '#64748b' }}
          >
            <i className={`bi ${showValue ? 'bi-eye-slash' : 'bi-eye'}`}></i>
          </button>
          <button
            className="btn btn-primary border-0 rounded-0 px-4"
            type="button"
            onClick={handleSave}
            disabled={saving || !dirty}
            style={{ fontWeight: '700' }}
          >
            {saving ? (
              <span className="spinner-border spinner-border-sm"></span>
            ) : (
              'SAVE'
            )}
          </button>
        </div>

        <div className="d-flex justify-content-between align-items-center">
          {helpUrl && (
            <a href={helpUrl} target="_blank" rel="noopener noreferrer" className="small fw-bold text-indigo-600 text-decoration-none d-flex align-items-center gap-1">
              <i className="bi bi-box-arrow-up-right me-1"></i>
              <span>Get API Key</span>
            </a>
          )}
          <button
            className="btn btn-sm btn-outline-primary"
            onClick={() => onTest(settingKey.split(':')[0])}
            disabled={testing || !inputValue}
            style={{ borderRadius: '8px', padding: '0.5rem 1rem' }}
          >
            {testing ? (
              <>
                <span className="spinner-border spinner-border-sm me-2"></span>
                Testing...
              </>
            ) : (
              <>
                <i className="bi bi-plug-fill me-1"></i>Test Connection
              </>
            )}
          </button>
        </div>

        {testResult && !testResult.success && testResult.message && (
          <div className="alert alert-danger py-2 px-3 mt-3 mb-0 small border-0 fw-medium" style={{ borderRadius: '8px', background: '#fef2f2', color: '#b91c1c' }}>
            <i className="bi bi-exclamation-circle me-2"></i>
            {testResult.message}
          </div>
        )}
      </div>
    </div>
  );
}
