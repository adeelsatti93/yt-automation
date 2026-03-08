import { useEffect, useState, useCallback } from 'react';
import { useApp } from '../../context/AppContext';
import { analyticsApi } from '../../api/analyticsApi';
import { useEpisodes } from '../../hooks/useEpisodes';
import StatsCard from '../analytics/StatsCard';
import PerformanceChart from '../analytics/PerformanceChart';
import LoadingSpinner from '../shared/LoadingSpinner';
import ErrorAlert from '../shared/ErrorAlert';
import { formatNumber, formatDate } from '../../utils/formatters';

export default function Analytics() {
  const { setPageTitle } = useApp();
  const { episodes, loading: episodesLoading } = useEpisodes({ status: 'Published', pageSize: 50 });

  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);
  const [syncing, setSyncing] = useState(false);
  const [error, setError] = useState(null);

  const fetchSummary = useCallback(async () => {
    setError(null);
    try {
      const res = await analyticsApi.getSummary();
      setSummary(res.data);
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to load analytics');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    setPageTitle('Analytics');
  }, [setPageTitle]);

  useEffect(() => {
    fetchSummary();
  }, [fetchSummary]);

  const handleSync = async () => {
    setSyncing(true);
    try {
      await analyticsApi.sync();
      await fetchSummary();
    } catch (err) {
      setError(err.response?.data?.message || 'Sync failed');
    } finally {
      setSyncing(false);
    }
  };

  if (loading && !summary) return <LoadingSpinner text="Loading analytics..." />;

  const stats = summary || {};
  const chartData = episodes
    .filter((ep) => ep.youtubeViews != null)
    .map((ep) => ({
      title: ep.title?.substring(0, 20) + (ep.title?.length > 20 ? '…' : '') || 'Untitled',
      views: ep.youtubeViews || 0,
    }))
    .slice(0, 15);

  return (
    <div className="container-fluid">
      {error && <ErrorAlert message={error} onRetry={fetchSummary} />}

      {/* Header */}
      <div className="d-flex justify-content-between align-items-center mb-4">
        <span className="text-muted">YouTube channel performance overview</span>
        <button
          className="btn btn-outline-primary"
          onClick={handleSync}
          disabled={syncing}
        >
          {syncing ? (
            <>
              <span className="spinner-border spinner-border-sm me-1"></span>
              Syncing...
            </>
          ) : (
            <>
              <i className="bi bi-arrow-clockwise me-1"></i>Sync from YouTube
            </>
          )}
        </button>
      </div>

      {/* Stats Row */}
      <div className="row g-3 mb-4">
        <div className="col-6 col-md-3">
          <StatsCard
            title="Total Views"
            value={stats.totalViews || 0}
            icon="bi-eye"
            variant="primary"
          />
        </div>
        <div className="col-6 col-md-3">
          <StatsCard
            title="Watch Hours"
            value={stats.totalWatchHours || 0}
            icon="bi-clock"
            variant="success"
          />
        </div>
        <div className="col-6 col-md-3">
          <StatsCard
            title="Subscribers"
            value={stats.subscribers || 0}
            icon="bi-people"
            variant="info"
          />
        </div>
        <div className="col-6 col-md-3">
          <StatsCard
            title="Avg Views"
            value={stats.averageViews || 0}
            icon="bi-graph-up"
            variant="warning"
          />
        </div>
      </div>

      <div className="row g-4">
        {/* Performance Chart */}
        <div className="col-lg-8">
          <div className="card shadow-sm">
            <div className="card-header">
              <h6 className="mb-0">
                <i className="bi bi-bar-chart me-2"></i>
                Views per Episode
              </h6>
            </div>
            <div className="card-body">
              <PerformanceChart data={chartData} dataKey="views" labelKey="title" />
            </div>
          </div>
        </div>

        {/* Top Episodes Table */}
        <div className="col-lg-4">
          <div className="card shadow-sm">
            <div className="card-header">
              <h6 className="mb-0">
                <i className="bi bi-trophy me-2"></i>
                Top Episodes
              </h6>
            </div>
            <div className="card-body p-0">
              <div className="table-responsive">
                <table className="table table-sm table-hover mb-0">
                  <thead className="table-light">
                    <tr>
                      <th>Episode</th>
                      <th className="text-end">Views</th>
                    </tr>
                  </thead>
                  <tbody>
                    {episodes
                      .filter((ep) => ep.youtubeViews != null)
                      .sort((a, b) => (b.youtubeViews || 0) - (a.youtubeViews || 0))
                      .slice(0, 10)
                      .map((ep) => (
                        <tr key={ep.id}>
                          <td className="text-truncate" style={{ maxWidth: '200px' }}>
                            {ep.title || 'Untitled'}
                          </td>
                          <td className="text-end">{formatNumber(ep.youtubeViews || 0)}</td>
                        </tr>
                      ))}
                    {episodes.filter((ep) => ep.youtubeViews != null).length === 0 && (
                      <tr>
                        <td colSpan="2" className="text-center text-muted py-3">
                          No published episodes with analytics data yet.
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Published Episodes Table */}
      <div className="card shadow-sm mt-4">
        <div className="card-header">
          <h6 className="mb-0">
            <i className="bi bi-table me-2"></i>
            Published Episodes
          </h6>
        </div>
        <div className="card-body p-0">
          <div className="table-responsive">
            <table className="table table-hover mb-0">
              <thead className="table-light">
                <tr>
                  <th>Title</th>
                  <th>Published</th>
                  <th className="text-end">Views</th>
                  <th className="text-end">Likes</th>
                  <th className="text-end">Comments</th>
                  <th>YouTube</th>
                </tr>
              </thead>
              <tbody>
                {episodes.map((ep) => (
                  <tr key={ep.id}>
                    <td className="text-truncate" style={{ maxWidth: '250px' }}>
                      {ep.title || 'Untitled'}
                    </td>
                    <td>{formatDate(ep.publishedAt || ep.createdAt)}</td>
                    <td className="text-end">{formatNumber(ep.youtubeViews || 0)}</td>
                    <td className="text-end">{formatNumber(ep.youtubeLikes || 0)}</td>
                    <td className="text-end">{formatNumber(ep.youtubeComments || 0)}</td>
                    <td>
                      {ep.youtubeUrl ? (
                        <a href={ep.youtubeUrl} target="_blank" rel="noopener noreferrer" className="text-danger">
                          <i className="bi bi-youtube"></i>
                        </a>
                      ) : (
                        <span className="text-muted">—</span>
                      )}
                    </td>
                  </tr>
                ))}
                {episodes.length === 0 && (
                  <tr>
                    <td colSpan="6" className="text-center text-muted py-4">
                      No published episodes yet.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}
