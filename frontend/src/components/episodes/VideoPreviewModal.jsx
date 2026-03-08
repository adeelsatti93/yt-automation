import EpisodeStatusBadge from './EpisodeStatusBadge';

export default function VideoPreviewModal({ show, episode, onClose }) {
  if (!show || !episode) return null;

  return (
    <div
      className="modal d-block"
      style={{ backgroundColor: 'rgba(0,0,0,0.7)' }}
      onClick={onClose}
    >
      <div
        className="modal-dialog modal-lg modal-dialog-centered"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="modal-content">
          <div className="modal-header">
            <div>
              <h5 className="modal-title mb-1">{episode.title || 'Video Preview'}</h5>
              <EpisodeStatusBadge status={episode.status} size="sm" />
            </div>
            <button
              type="button"
              className="btn-close"
              onClick={onClose}
            ></button>
          </div>

          <div className="modal-body p-0">
            <video
              className="w-100"
              controls
              autoPlay
              src={episode.videoUrl}
              style={{ maxHeight: '480px', backgroundColor: '#000' }}
            >
              Your browser does not support video playback.
            </video>
          </div>

          {episode.scriptText && (
            <div className="modal-footer d-block">
              <h6 className="mb-2">Script</h6>
              <div
                className="bg-light rounded p-3"
                style={{ maxHeight: '200px', overflowY: 'auto', whiteSpace: 'pre-wrap' }}
              >
                <small>{episode.scriptText}</small>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
