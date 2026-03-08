import EpisodeCard from './EpisodeCard';
import LoadingSpinner from '../shared/LoadingSpinner';
import EmptyState from '../shared/EmptyState';

const FILTER_TABS = [
  { key: 'all', label: 'All' },
  { key: 'inProgress', label: 'In Progress' },
  { key: 'pendingReview', label: 'Pending Review' },
  { key: 'published', label: 'Published' },
  { key: 'failed', label: 'Failed' },
];

export default function EpisodeList({
  episodes,
  loading,
  onView,
  onDelete,
  onFilterChange,
  activeFilter = 'all',
}) {
  if (loading) {
    return <LoadingSpinner text="Loading episodes..." />;
  }

  return (
    <div>
      <ul className="nav nav-tabs mb-4">
        {FILTER_TABS.map((tab) => (
          <li className="nav-item" key={tab.key}>
            <button
              className={`nav-link ${activeFilter === tab.key ? 'active' : ''}`}
              onClick={() => onFilterChange(tab.key)}
            >
              {tab.label}
            </button>
          </li>
        ))}
      </ul>

      {episodes.length === 0 ? (
        <EmptyState
          icon="bi-film"
          title="No episodes found"
          message="No episodes match the current filter."
        />
      ) : (
        <div className="row row-cols-1 row-cols-md-2 row-cols-lg-3 row-cols-xl-4 g-4">
          {episodes.map((episode) => (
            <div className="col" key={episode.id}>
              <EpisodeCard
                episode={episode}
                onView={onView}
                onDelete={onDelete}
              />
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
