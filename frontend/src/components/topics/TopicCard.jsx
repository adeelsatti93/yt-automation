import { useState } from 'react';
import { formatDate } from '../../utils/formatters';

export default function TopicCard({ topic, onEdit, onDelete, onTrigger }) {
  const [triggering, setTriggering] = useState(false);
  const [triggered, setTriggered] = useState(false);

  const handleTrigger = async () => {
    setTriggering(true);
    try {
      await onTrigger(topic);
      setTriggered(true); // Keep button disabled after successful trigger
    } catch {
      setTriggering(false);
    }
  };

  return (
    <div className="card shadow-sm mb-2">
      <div className="card-body py-3">
        <div className="d-flex align-items-start">
          <div className="flex-grow-1">
            <div className="d-flex align-items-center gap-2 mb-1">
              <h6 className="mb-0">{topic.title}</h6>
              {topic.priority > 0 && (
                <span className="badge bg-info bg-opacity-10 text-info">
                  Priority: {topic.priority}
                </span>
              )}
              {topic.isUsed && (
                <span className="badge bg-secondary">Used</span>
              )}
            </div>
            {topic.targetMoral && (
              <small className="text-muted d-block">
                <i className="bi bi-heart me-1"></i>
                {topic.targetMoral}
              </small>
            )}
            {topic.description && (
              <small className="text-muted d-block mt-1">{topic.description}</small>
            )}
            <small className="text-muted d-block mt-1">
              <i className="bi bi-calendar me-1"></i>
              {formatDate(topic.createdAt)}
            </small>
          </div>
          <div className="d-flex gap-1 ms-2 flex-shrink-0">
            {!topic.isUsed && (
              <button
                className={`btn btn-sm ${triggered ? 'btn-success' : 'btn-outline-success'}`}
                onClick={handleTrigger}
                disabled={triggering || triggered}
                title={triggered ? 'Pipeline started' : 'Produce Now'}
              >
                {triggering ? (
                  <><span className="spinner-border spinner-border-sm me-1"></span>Starting...</>
                ) : triggered ? (
                  <><i className="bi bi-check-lg me-1"></i>Pipeline Started</>
                ) : (
                  <><i className="bi bi-play-fill me-1"></i>Produce</>
                )}
              </button>
            )}
            <button
              className="btn btn-sm btn-outline-primary"
              onClick={() => onEdit(topic)}
              title="Edit"
            >
              <i className="bi bi-pencil"></i>
            </button>
            <button
              className="btn btn-sm btn-outline-danger"
              onClick={() => onDelete(topic)}
              title="Delete"
            >
              <i className="bi bi-trash"></i>
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
