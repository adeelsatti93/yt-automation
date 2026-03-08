import { getStatusBadgeClass, getStatusLabel } from '../../utils/statusHelpers';

export default function EpisodeStatusBadge({ status, size = 'md' }) {
  const sizeClass = size === 'sm' ? 'fs-8' : '';

  return (
    <span className={`badge ${getStatusBadgeClass(status)} ${sizeClass}`}>
      {getStatusLabel(status)}
    </span>
  );
}
