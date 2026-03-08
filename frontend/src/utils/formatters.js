/**
 * Formatting utility functions for the Kids Cartoon Pipeline app.
 */

/**
 * Format a date string into "Mar 8, 2026" format.
 * @param {string} dateString - ISO date string or any Date-parseable string.
 * @returns {string} Formatted date.
 */
export function formatDate(dateString) {
  if (!dateString) return '';
  const date = new Date(dateString);
  if (isNaN(date.getTime())) return '';
  return date.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
}

/**
 * Format a date string into "Mar 8, 2026 2:30 PM" format.
 * @param {string} dateString - ISO date string or any Date-parseable string.
 * @returns {string} Formatted date and time.
 */
export function formatDateTime(dateString) {
  if (!dateString) return '';
  const date = new Date(dateString);
  if (isNaN(date.getTime())) return '';
  return date.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
    hour12: true,
  });
}

/**
 * Format a duration in seconds into a human-readable string.
 * Returns "2m 30s" for durations under an hour, "1h 5m" for longer durations.
 * @param {number} seconds - Duration in seconds.
 * @returns {string} Formatted duration.
 */
export function formatDuration(seconds) {
  if (seconds == null || isNaN(seconds) || seconds < 0) return '0s';

  const totalSeconds = Math.floor(seconds);

  if (totalSeconds < 60) {
    return `${totalSeconds}s`;
  }

  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const remainingSeconds = totalSeconds % 60;

  if (hours > 0) {
    return minutes > 0 ? `${hours}h ${minutes}m` : `${hours}h`;
  }

  return remainingSeconds > 0
    ? `${minutes}m ${remainingSeconds}s`
    : `${minutes}m`;
}

/**
 * Format a file size in bytes into a human-readable string.
 * @param {number} bytes - File size in bytes.
 * @returns {string} Formatted file size (e.g. "1.5 MB").
 */
export function formatFileSize(bytes) {
  if (bytes == null || isNaN(bytes) || bytes < 0) return '0 B';

  if (bytes === 0) return '0 B';

  const units = ['B', 'KB', 'MB', 'GB', 'TB'];
  const base = 1024;
  const unitIndex = Math.floor(Math.log(bytes) / Math.log(base));
  const clampedIndex = Math.min(unitIndex, units.length - 1);
  const value = bytes / Math.pow(base, clampedIndex);

  // Show decimals only for values >= KB
  if (clampedIndex === 0) {
    return `${Math.round(value)} B`;
  }

  return `${value % 1 === 0 ? value.toFixed(0) : value.toFixed(1)} ${units[clampedIndex]}`;
}

/**
 * Truncate text to a maximum length, appending "..." if truncated.
 * @param {string} text - The text to truncate.
 * @param {number} [maxLength=100] - Maximum allowed length.
 * @returns {string} Truncated text.
 */
export function truncateText(text, maxLength = 100) {
  if (!text) return '';
  if (text.length <= maxLength) return text;
  return text.slice(0, maxLength) + '...';
}

/**
 * Format a number with comma separators (e.g. 1234 -> "1,234").
 * @param {number} num - The number to format.
 * @returns {string} Formatted number.
 */
export function formatNumber(num) {
  if (num == null || isNaN(num)) return '0';
  return Number(num).toLocaleString('en-US');
}

/**
 * Format a value as a percentage string.
 * @param {number} value - The numeric value (e.g. 45.5 for 45.5%).
 * @param {number} [decimals=1] - Number of decimal places.
 * @returns {string} Formatted percentage (e.g. "45.5%").
 */
export function formatPercentage(value, decimals = 1) {
  if (value == null || isNaN(value)) return '0%';
  return `${Number(value).toFixed(decimals)}%`;
}
