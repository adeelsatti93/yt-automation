import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useApp } from '../../context/AppContext';
import { useEpisodes } from '../../hooks/useEpisodes';
import { usePipeline } from '../../hooks/usePipeline';
import StatsCard from '../analytics/StatsCard';
import PipelineStatusBar from '../pipeline/PipelineStatusBar';
import EpisodeCard from '../episodes/EpisodeCard';
import LoadingSpinner from '../shared/LoadingSpinner';
import ErrorAlert from '../shared/ErrorAlert';
import EmptyState from '../shared/EmptyState';

export default function Dashboard() {
  const { setPageTitle } = useApp();
  const navigate = useNavigate();

  const {
    episodes,
    totalCount,
    loading: episodesLoading,
    error: episodesError,
    fetchEpisodes,
    deleteEpisode,
  } = useEpisodes({ pageSize: 5 });

  const {
    status: pipelineStatus,
    loading: pipelineLoading,
    error: pipelineError,
    trigger,
    getStatus,
  } = usePipeline();

  const [triggerLoading, setTriggerLoading] = useState(false);

  useEffect(() => {
    setPageTitle('Dashboard');
  }, [setPageTitle]);

  // Compute summary stats from episodes and pipeline status
  const stats = {
    total: totalCount || 0,
    published: episodes.filter((ep) => ep.status === 'Published').length,
    pendingReview: episodes.filter((ep) => ep.status === 'PendingReview').length,
    inPipeline: episodes.filter(
      (ep) =>
        !['Published', 'Failed', 'Rejected', 'PendingReview', 'Approved'].includes(ep.status)
    ).length,
  };

  const handleTriggerPipeline = async () => {
    setTriggerLoading(true);
    try {
      await trigger();
    } catch {
      // Error is set in the hook
    } finally {
      setTriggerLoading(false);
    }
  };

  const handleViewEpisode = (episode) => {
    navigate(`/episodes/${episode.id}`);
  };

  const handleDeleteEpisode = async (episode) => {
    if (window.confirm(`Delete episode "${episode.title || 'Untitled'}"?`)) {
      try {
        await deleteEpisode(episode.id);
      } catch {
        // Error is set in the hook
      }
    }
  };

  const loading = episodesLoading || pipelineLoading;
  const error = episodesError || pipelineError;

  if (loading && episodes.length === 0) {
    return <LoadingSpinner text="Loading dashboard..." />;
  }

  return (
    <div className="container-fluid">
      {error && (
        <ErrorAlert
          message={error}
          onRetry={() => {
            fetchEpisodes();
            getStatus();
          }}
        />
      )}

      {/* Summary Stats */}
      <div className="row g-3 mb-4">
        <div className="col-6 col-md-3">
          <StatsCard
            title="Total Episodes"
            value={stats.total}
            icon="bi-film"
            variant="primary"
          />
        </div>
        <div className="col-6 col-md-3">
          <StatsCard
            title="Published"
            value={stats.published}
            icon="bi-check-circle"
            variant="success"
          />
        </div>
        <div className="col-6 col-md-3">
          <StatsCard
            title="Pending Review"
            value={stats.pendingReview}
            icon="bi-hourglass-split"
            variant="warning"
          />
        </div>
        <div className="col-6 col-md-3">
          <StatsCard
            title="In Pipeline"
            value={stats.inPipeline}
            icon="bi-gear-wide-connected"
            variant="info"
          />
        </div>
      </div>

      {/* Pipeline Status */}
      <div className="mb-4">
        <PipelineStatusBar status={pipelineStatus} />
      </div>

      {/* Quick Actions */}
      <div className="d-flex gap-2 mb-4">
        <button
          className="btn btn-primary"
          onClick={() => navigate('/topics')}
        >
          <i className="bi bi-plus-lg me-1"></i>
          New Topic
        </button>
        <button
          className="btn btn-outline-primary"
          onClick={handleTriggerPipeline}
          disabled={triggerLoading}
        >
          {triggerLoading ? (
            <>
              <span className="spinner-border spinner-border-sm me-1" role="status"></span>
              Triggering...
            </>
          ) : (
            <>
              <i className="bi bi-play-circle me-1"></i>
              Trigger Pipeline
            </>
          )}
        </button>
      </div>

      {/* Recent Episodes */}
      <div className="glass-card">
        <div className="card-header border-0 bg-transparent py-4 px-4 d-flex justify-content-between align-items-center">
          <h4 className="mb-0 font-headings">
            <i className="bi bi-clock-history me-2 text-primary"></i>
            Recent Episodes
          </h4>
          <button
            className="btn btn-outline-primary"
            onClick={() => navigate('/episodes')}
          >
            View All
          </button>
        </div>
        <div className="p-4 pt-0">
          {episodes.length === 0 ? (
            <EmptyState
              icon="bi-film"
              title="No episodes yet"
              message="Create a topic and trigger the pipeline to generate your first episode."
              action={() => navigate('/topics')}
              actionLabel="Create Topic"
            />
          ) : (
            <div className="row g-4">
              {episodes.slice(0, 5).map((episode) => (
                <div className="col-md-6 col-lg-4 col-xl-2-4" key={episode.id}>
                  <EpisodeCard
                    episode={episode}
                    onView={handleViewEpisode}
                    onDelete={handleDeleteEpisode}
                  />
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
