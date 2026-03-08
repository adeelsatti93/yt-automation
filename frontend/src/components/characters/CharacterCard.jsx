import { useState } from 'react';

export default function CharacterCard({ character, onEdit, onDelete, onTestVoice }) {
  const [testingVoice, setTestingVoice] = useState(false);

  const handleTestVoice = async () => {
    setTestingVoice(true);
    try {
      await onTestVoice(character);
    } finally {
      setTestingVoice(false);
    }
  };

  return (
    <div className="card h-100 shadow-sm">
      <div className="card-body">
        <div className="d-flex align-items-start mb-3">
          <div
            className="rounded-circle bg-primary bg-opacity-10 d-flex align-items-center justify-content-center me-3 flex-shrink-0"
            style={{ width: '48px', height: '48px' }}
          >
            <i className="bi bi-person-fill text-primary" style={{ fontSize: '1.5rem' }}></i>
          </div>
          <div className="flex-grow-1 min-width-0">
            <h6 className="card-title mb-1">{character.name}</h6>
            {character.voiceId && (
              <small className="text-muted">
                <i className="bi bi-mic me-1"></i>
                {character.voiceName || character.voiceId}
              </small>
            )}
          </div>
        </div>

        <p className="card-text text-muted small mb-0" style={{
          display: '-webkit-box',
          WebkitLineClamp: 3,
          WebkitBoxOrient: 'vertical',
          overflow: 'hidden',
        }}>
          {character.description || 'No description provided.'}
        </p>
      </div>

      <div className="card-footer bg-transparent border-top-0 d-flex gap-2">
        {character.voiceId && (
          <button
            className="btn btn-sm btn-outline-info"
            onClick={handleTestVoice}
            disabled={testingVoice}
          >
            {testingVoice ? (
              <>
                <span className="spinner-border spinner-border-sm me-1"></span>
                Testing...
              </>
            ) : (
              <>
                <i className="bi bi-volume-up me-1"></i>Test
              </>
            )}
          </button>
        )}
        <button
          className="btn btn-sm btn-outline-primary flex-grow-1"
          onClick={() => onEdit(character)}
        >
          <i className="bi bi-pencil me-1"></i>Edit
        </button>
        <button
          className="btn btn-sm btn-outline-danger"
          onClick={() => onDelete(character)}
        >
          <i className="bi bi-trash"></i>
        </button>
      </div>
    </div>
  );
}
