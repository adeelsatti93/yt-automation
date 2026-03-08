import { useState } from 'react';

export default function ConnectionTestButton({ service, onTest }) {
  const [testing, setTesting] = useState(false);
  const [result, setResult] = useState(null);

  const handleTest = async () => {
    setTesting(true);
    setResult(null);
    try {
      const res = await onTest(service);
      setResult(res);
    } catch (err) {
      setResult({ success: false, message: err.message || 'Test failed' });
    } finally {
      setTesting(false);
    }
  };

  return (
    <div className="d-inline-flex align-items-center gap-2">
      <button
        className="btn btn-sm btn-outline-info"
        onClick={handleTest}
        disabled={testing}
      >
        {testing ? (
          <>
            <span className="spinner-border spinner-border-sm me-1"></span>
            Testing...
          </>
        ) : (
          <>
            <i className="bi bi-plug me-1"></i>Test
          </>
        )}
      </button>
      {result && (
        <span className={`badge ${result.success ? 'bg-success' : 'bg-danger'}`}>
          {result.success ? '✅ OK' : `❌ ${result.message || 'Failed'}`}
        </span>
      )}
    </div>
  );
}
