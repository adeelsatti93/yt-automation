import { getStatusBadgeClass, getStatusLabel } from '../../utils/statusHelpers';
import { formatDate } from '../../utils/formatters';

export default function EpisodeCard({ episode, onView, onDelete }) {
  const hasThumbnail = episode.thumbnailGenerated || episode.thumbnail;

  return (
    <div className="card h-100 overflow-hidden border-0" 
         style={{ 
           background: '#ffffff', 
           border: '1px solid #e2e8f0 !important',
           boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)',
           transition: 'all 0.3s ease',
           cursor: 'pointer'
         }}
         onMouseOver={e => {
            e.currentTarget.style.transform = 'translateY(-4px)';
            e.currentTarget.style.boxShadow = '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)';
         }}
         onMouseOut={e => {
            e.currentTarget.style.transform = 'translateY(0)';
            e.currentTarget.style.boxShadow = '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)';
         }}
    >
      {/* Thumbnail Area */}
      <div className="position-relative" style={{ height: '140px', background: '#f8fafc' }}>
        {hasThumbnail ? (
          <img
            src={`/api/assets/thumbnails/${episode.id}`}
            alt={episode.title}
            className="w-100 h-100 object-fit-cover"
            style={{ opacity: 0.95 }}
          />
        ) : (
          <div className="w-100 h-100 d-flex align-items-center justify-content-center text-slate-300">
            <i className="bi bi-film display-5 opacity-25" style={{ color: '#94a3b8' }}></i>
          </div>
        )}
        
        {/* Status Badge Over Image */}
        <div className="position-absolute top-0 end-0 m-3">
          <span className={`badge ${getStatusBadgeClass(episode.status)} px-2 py-1`} style={{ fontSize: '0.65rem', boxShadow: '0 2px 4px rgba(0,0,0,0.05)' }}>
            {getStatusLabel(episode.status)}
          </span>
        </div>
      </div>

      <div className="card-body d-flex flex-column p-4">
        <h6 className="card-title font-headings fw-bold mb-3 text-slate-900 text-truncate" 
            style={{ fontSize: '0.95rem' }}
            title={episode.title || 'Untitled Episode'}>
          {episode.title || 'Untitled Episode'}
        </h6>
        
        <div className="d-flex flex-column gap-2 mb-4">
          <div className="d-flex align-items-center gap-2 smaller text-slate-600 fw-medium">
            <i className="bi bi-person text-indigo-500" style={{ color: '#6366f1' }}></i>
            {episode.characterName || 'Multiple Characters'}
          </div>
          <div className="d-flex align-items-center gap-2 smaller text-slate-400">
            <i className="bi bi-calendar3"></i>
            {formatDate(episode.createdAt)}
          </div>
        </div>

        <div className="mt-auto d-flex gap-2">
          <button
            className="btn btn-outline-primary btn-sm flex-grow-1"
            onClick={(e) => { e.stopPropagation(); onView(episode); }}
            style={{ padding: '0.5rem 0.75rem' }}
          >
            <i className="bi bi-eye"></i> Details
          </button>
          <button
            className="btn btn-outline-danger btn-sm px-2"
            onClick={(e) => { e.stopPropagation(); onDelete(episode); }}
            title="Delete"
            style={{ padding: '0.5rem 0.75rem' }}
          >
            <i className="bi bi-trash"></i>
          </button>
        </div>
      </div>
    </div>
  );
}
