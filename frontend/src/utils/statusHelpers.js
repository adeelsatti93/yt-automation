/**
 * Status and pipeline stage helpers for the Kids Cartoon Pipeline app.
 */

// ──────────────────────────────────────────────
// Episode statuses (in pipeline order)
// ──────────────────────────────────────────────

const STATUS_LABELS = {
  TopicQueued: 'Topic Queued',
  GeneratingScript: 'Generating Script',
  ScriptGenerated: 'Script Generated',
  GeneratingVoice: 'Generating Voice',
  VoiceGenerated: 'Voice Generated',
  GeneratingImages: 'Generating Images',
  ImagesGenerated: 'Images Generated',
  GeneratingMusic: 'Generating Music',
  MusicGenerated: 'Music Generated',
  AssemblingVideo: 'Assembling Video',
  VideoAssembled: 'Video Assembled',
  GeneratingSeo: 'Generating SEO',
  PendingReview: 'Pending Review',
  Approved: 'Approved',
  Published: 'Published',
  Failed: 'Failed',
  Rejected: 'Rejected',
};

const STATUS_BADGE_CLASSES = {
  TopicQueued: 'bg-secondary',
  GeneratingScript: 'bg-primary',
  ScriptGenerated: 'bg-info',
  GeneratingVoice: 'bg-primary',
  VoiceGenerated: 'bg-info',
  GeneratingImages: 'bg-primary',
  ImagesGenerated: 'bg-info',
  GeneratingMusic: 'bg-primary',
  MusicGenerated: 'bg-info',
  AssemblingVideo: 'bg-primary',
  VideoAssembled: 'bg-info',
  GeneratingSeo: 'bg-primary',
  PendingReview: 'bg-warning',
  Approved: 'bg-success',
  Published: 'bg-success',
  Failed: 'bg-danger',
  Rejected: 'bg-danger',
};

// ──────────────────────────────────────────────
// Pipeline stages
// ──────────────────────────────────────────────

const STAGE_LABELS = {
  ScriptGeneration: 'Script Generation',
  VoiceGeneration: 'Voice Generation',
  ImageGeneration: 'Image Generation',
  MusicGeneration: 'Music Generation',
  VideoAssembly: 'Video Assembly',
  SeoGeneration: 'SEO Generation',
};

const STAGE_ICONS = {
  ScriptGeneration: 'bi-file-earmark-text',
  VoiceGeneration: 'bi-mic',
  ImageGeneration: 'bi-image',
  MusicGeneration: 'bi-music-note-beamed',
  VideoAssembly: 'bi-film',
  SeoGeneration: 'bi-search',
};

// ──────────────────────────────────────────────
// Exported helpers
// ──────────────────────────────────────────────

/**
 * Return a Bootstrap badge class for the given episode status.
 * @param {string} status - Episode status enum value.
 * @returns {string} Bootstrap badge class (e.g. "bg-primary").
 */
export function getStatusBadgeClass(status) {
  return STATUS_BADGE_CLASSES[status] || 'bg-secondary';
}

/**
 * Return a human-readable label for the given episode status.
 * @param {string} status - Episode status enum value (e.g. "TopicQueued").
 * @returns {string} Human-readable label (e.g. "Topic Queued").
 */
export function getStatusLabel(status) {
  return STATUS_LABELS[status] || status || 'Unknown';
}

/**
 * Return a Bootstrap icon class for the given pipeline stage.
 * @param {string} stage - Pipeline stage enum value.
 * @returns {string} Bootstrap icon class (e.g. "bi-file-earmark-text").
 */
export function getStageIcon(stage) {
  return STAGE_ICONS[stage] || 'bi-gear';
}

/**
 * Return a human-readable label for the given pipeline stage.
 * @param {string} stage - Pipeline stage enum value (e.g. "ScriptGeneration").
 * @returns {string} Human-readable label (e.g. "Script Generation").
 */
export function getStageLabel(stage) {
  return STAGE_LABELS[stage] || stage || 'Unknown';
}

/**
 * Check whether the given status is a terminal (final) status.
 * Terminal statuses: Published, Failed, Rejected.
 * @param {string} status - Episode status enum value.
 * @returns {boolean} True if the status is terminal.
 */
export function isTerminalStatus(status) {
  return ['Published', 'Failed', 'Rejected'].includes(status);
}

/**
 * Check whether the given status is actionable (requires user action).
 * @param {string} status - Episode status enum value.
 * @returns {boolean} True if the status is actionable.
 */
export function isActionableStatus(status) {
  return status === 'PendingReview';
}

/**
 * Return an ordered array of all episode statuses representing the
 * pipeline flow. Useful for building progress indicators.
 * @returns {string[]} Array of status enum values in pipeline order.
 */
export function getStatusFlow() {
  return [
    'TopicQueued',
    'GeneratingScript',
    'ScriptGenerated',
    'GeneratingVoice',
    'VoiceGenerated',
    'GeneratingImages',
    'ImagesGenerated',
    'GeneratingMusic',
    'MusicGenerated',
    'AssemblingVideo',
    'VideoAssembled',
    'GeneratingSeo',
    'PendingReview',
    'Approved',
    'Published',
    'Failed',
    'Rejected',
  ];
}
