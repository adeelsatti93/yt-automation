import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useApp } from '../../context/AppContext';
import { useEpisodes } from '../../hooks/useEpisodes';
import EpisodeCard from '../episodes/EpisodeCard';
import LoadingSpinner from '../shared/LoadingSpinner';
import ErrorAlert from '../shared/ErrorAlert';
import EmptyState from '../shared/EmptyState';
import ConfirmModal from '../shared/ConfirmModal';

const STATUS_OPTIONS = [
  { value: '', label: 'All Statuses' },
  { value: 'TopicQueued', label: 'Queued' },
  { value: 'GeneratingScript', label: 'Processing' },
  { value: 'PendingReview', label: 'Pending Review' },
  { value: 'Published', label: 'Published' },
  { value: 'Failed', label: 'Failed' },
  { value: 'Rejected', label: 'Rejected' },
];

export default function Episodes() {
  const { setPageTitle } = useApp();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  const initialStatus = searchParams.get('status') || '';
  const {
    episodes,
    totalCount,
    totalPages,
    loading,
    error,
    filters,
    fetchEpisodes,
    deleteEpisode,
    setStatusFilter,
    setPage,
  } = useEpisodes({ status: initialStatus || null, pageSize: 12 });

  const [deleteTarget, setDeleteTarget] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    setPageTitle('Episodes');
  }, [setPageTitle]);

  const handleStatusChange = (e) => {
    const status = e.target.value;
    setStatusFilter(status || null);
    if (status) {
      setSearchParams({ status });
    } else {
      setSearchParams({});
    }
  };

  const handleView = (episode) => {
    navigate(`/episodes/${episode.id}`);
  };

  const handleDeleteConfirm = async () => {
    if (deleteTarget) {
      try {
        await deleteEpisode(deleteTarget.id);
      } catch {
        // Error handled in hook
      }
      setDeleteTarget(null);
    }
  };

  const filtered = searchTerm
    ? episodes.filter((ep) =>
        (ep.title || '').toLowerCase().includes(searchTerm.toLowerCase())
      )
    : episodes;

  if (loading && episodes.length === 0) {
    return <LoadingSpinner text="Loading episodes..." />;
  }

  return (
    <div className="container-fluid">
      {error && <ErrorAlert message={error} onRetry={fetchEpisodes} />}

      {/* Filter Bar */}
      <div className="glass-card mb-4 p-4">
        <div className="row g-3 align-items-center">
          <div className="col-md-4">
            <div className="input-group">
              <span className="input-group-text">
                <i className="bi bi-search text-muted"></i>
              </span>
              <input
                type="text"
                className="form-control"
                placeholder="Search episodes..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
          </div>
          <div className="col-md-3">
            <select
              className="form-select"
              value={filters.status || ''}
              onChange={handleStatusChange}
            >
              {STATUS_OPTIONS.map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.label}
                </option>
              ))}
            </select>
          </div>
          <div className="col-md-5 text-end">
            <span className="text-muted me-3 small">
              {totalCount} total episodes
            </span>
            <button
              className="btn btn-primary"
              onClick={() => navigate('/topics')}
            >
              <i className="bi bi-plus-lg"></i> New Episode
            </button>
          </div>
        </div>
      </div>

      {/* Episode Grid */}
      {filtered.length === 0 ? (
        <EmptyState
          icon="bi-film"
          title="No episodes found"
          message="Add some topics and trigger the pipeline to generate episodes."
          action={() => navigate('/topics')}
          actionLabel="Add Topics"
        />
      ) : (
        <>
          <div className="row row-cols-1 row-cols-md-2 row-cols-lg-3 row-cols-xl-4 g-3">
            {filtered.map((episode) => (
              <div className="col" key={episode.id}>
                <EpisodeCard
                  episode={episode}
                  onView={handleView}
                  onDelete={setDeleteTarget}
                />
              </div>
            ))}
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <nav className="mt-4 d-flex justify-content-center">
              <ul className="pagination">
                <li className={`page-item ${filters.page <= 1 ? 'disabled' : ''}`}>
                  <button className="page-link" onClick={() => setPage(filters.page - 1)}>
                    Previous
                  </button>
                </li>
                {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
                  <li key={p} className={`page-item ${p === filters.page ? 'active' : ''}`}>
                    <button className="page-link" onClick={() => setPage(p)}>
                      {p}
                    </button>
                  </li>
                ))}
                <li className={`page-item ${filters.page >= totalPages ? 'disabled' : ''}`}>
                  <button className="page-link" onClick={() => setPage(filters.page + 1)}>
                    Next
                  </button>
                </li>
              </ul>
            </nav>
          )}
        </>
      )}

      {/* Delete Confirmation */}
      <ConfirmModal
        show={!!deleteTarget}
        title="Delete Episode"
        message={`Are you sure you want to delete "${deleteTarget?.title || 'Untitled'}"? This action cannot be undone.`}
        confirmText="Delete"
        variant="danger"
        onConfirm={handleDeleteConfirm}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  );
}
