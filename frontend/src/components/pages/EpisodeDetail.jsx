import { useEffect, useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useApp } from '../../context/AppContext';
import { episodesApi } from '../../api/episodesApi';
import { getStatusLabel, getStatusBadgeClass, getStageLabel, getStageIcon, isActionableStatus } from '../../utils/statusHelpers';
import { formatDate, formatDateTime, formatDuration } from '../../utils/formatters';
import PipelineStageIndicator from '../pipeline/PipelineStageIndicator';
import PipelineLogFeed from '../pipeline/PipelineLogFeed';
import VideoPreviewModal from '../episodes/VideoPreviewModal';
import LoadingSpinner from '../shared/LoadingSpinner';
import ErrorAlert from '../shared/ErrorAlert';

const PIPELINE_STAGES = [
  'ScriptGeneration',
  'ImageGeneration',
  'VoiceGeneration',
  'MusicGeneration',
  'VideoAssembly',
  'SeoGeneration',
];

function getStageStatus(episodeStatus) {
  const statusToStageIndex = {
    TopicQueued: -1,
    GeneratingScript: 0, ScriptGenerated: 1,
    GeneratingImages: 1, ImagesGenerated: 2,
    GeneratingVoice: 2, VoiceGenerated: 3,
    GeneratingMusic: 3, MusicGenerated: 4,
    AssemblingVideo: 4, VideoAssembled: 5,
    GeneratingSeo: 5,
    PendingReview: 6, Approved: 6, Published: 6,
  };
  return statusToStageIndex[episodeStatus] ?? -1;
}

export default function EpisodeDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { setPageTitle } = useApp();

  const [episode, setEpisode] = useState(null);
  const [jobs, setJobs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [showVideo, setShowVideo] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);
  const [rejectReason, setRejectReason] = useState('');
  const [showRejectInput, setShowRejectInput] = useState(false);

  const fetchEpisode = useCallback(async () => {
    setError(null);
    try {
      const [epRes, jobRes] = await Promise.all([
        episodesApi.getById(id),
        episodesApi.getPipelineJobs(id),
      ]);
      setEpisode(epRes.data);
      setJobs(Array.isArray(jobRes.data) ? jobRes.data : jobRes.data.jobs || []);
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to load episode');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchEpisode();
  }, [fetchEpisode]);

  useEffect(() => {
    setPageTitle(episode?.title || 'Episode Detail');
  }, [episode, setPageTitle]);

  // Auto-poll for active pipeline status
  useEffect(() => {
    if (!episode) return;
    const terminalStates = ['PendingReview', 'Approved', 'Published', 'Failed', 'Rejected'];
    const isProcessing = !terminalStates.includes(episode.status);

    // Reset actionLoading when pipeline reaches a terminal state
    if (!isProcessing) {
      setActionLoading(false);
      return;
    }

    const interval = setInterval(fetchEpisode, 4000);
    return () => clearInterval(interval);
  }, [episode, fetchEpisode]);

  const handleApprove = async () => {
    setActionLoading(true);
    try {
      const res = await episodesApi.approve(id);
      setEpisode(res.data);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to approve');
    } finally {
      setActionLoading(false);
    }
  };

  const handleReject = async () => {
    setActionLoading(true);
    try {
      const res = await episodesApi.reject(id, { reason: rejectReason });
      setEpisode(res.data);
      setShowRejectInput(false);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to reject');
    } finally {
      setActionLoading(false);
    }
  };

  const handleRegenerate = async (stage) => {
    setActionLoading(true);
    try {
      await episodesApi.regenerate(id, stage);
      // Optimistically mark as processing so buttons disable and auto-poll kicks in
      setEpisode(prev => prev ? { ...prev, status: 'GeneratingScript' } : prev);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to regenerate');
      setActionLoading(false);
    }
  };

  const handleResume = async (stage) => {
    setActionLoading(true);
    setError(null);
    try {
      await episodesApi.resume(id, stage);
      // Optimistically mark as processing so resume card hides and auto-poll kicks in
      setEpisode(prev => prev ? { ...prev, status: 'GeneratingScript' } : prev);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to resume pipeline');
      setActionLoading(false);
    }
  };

  const getFailedStage = () => {
    const failedJob = [...jobs].reverse().find(j => j.status === 'Failed');
    return failedJob?.stage || null;
  };

  if (loading) return <LoadingSpinner text="Loading episode..." />;
  if (error && !episode) return <ErrorAlert message={error} onRetry={fetchEpisode} />;
  if (!episode) return <ErrorAlert message="Episode not found" />;

  // For failed episodes, compute stage index from jobs data
  const stageIndex = episode.status === 'Failed'
    ? PIPELINE_STAGES.filter(stage => jobs.some(j => j.stage === stage && j.status === 'Completed')).length
    : getStageStatus(episode.status);

  return (
    <div className="container-fluid">
      {error && <ErrorAlert message={error} onRetry={fetchEpisode} />}

      {/* Header with Back button */}
      <div className="d-flex align-items-center mb-5 mt-2">
        <button className="btn btn-outline-primary" onClick={() => navigate('/episodes')}>
          <i className="bi bi-arrow-left"></i> Back to Episodes
        </button>
        <div className="ms-auto">
          <span className={`badge ${getStatusBadgeClass(episode.status)} fs-6 px-3 py-2`}>
            {getStatusLabel(episode.status)}
          </span>
        </div>
      </div>

      <div className="row g-4">
        {/* Left Column: Video + Pipeline */}
        <div className="col-lg-8">
          {/* Video Preview */}
          {episode.videoUrl && (
            <div className="card shadow-sm mb-4 overflow-hidden">
              <div
                className="position-relative d-flex align-items-center justify-content-center"
                style={{
                  minHeight: '280px',
                  background: episode.thumbnailUrl
                    ? `url(${episode.thumbnailUrl}) center/cover no-repeat`
                    : 'linear-gradient(135deg, #0f172a 0%, #1e293b 100%)',
                  cursor: 'pointer',
                }}
                onClick={() => setShowVideo(true)}
              >
                <div className="position-absolute top-0 start-0 w-100 h-100" style={{ background: 'rgba(0,0,0,0.35)' }} />
                <div className="position-relative text-center text-white z-1">
                  <div
                    className="d-inline-flex align-items-center justify-content-center rounded-circle mb-3"
                    style={{ width: '72px', height: '72px', background: 'rgba(79,70,229,0.9)', boxShadow: '0 8px 30px rgba(79,70,229,0.5)' }}
                  >
                    <i className="bi bi-play-fill" style={{ fontSize: '2rem', marginLeft: '4px' }}></i>
                  </div>
                  <div className="fw-bold fs-5">{episode.title || 'Watch Episode'}</div>
                  <small className="opacity-75">Click to preview before approving</small>
                </div>
              </div>
            </div>
          )}

          <div className="card shadow-sm mb-4 border-top border-primary border-opacity-50">
            <div className="card-header py-3">
              <h6 className="mb-0 font-headings text-uppercase small ls-wide"><i className="bi bi-cpu me-2 text-primary"></i>AI Engine Intelligence Flow</h6>
            </div>
            <div className="card-body">
              <div className="d-flex justify-content-between overflow-auto pb-2">
                {PIPELINE_STAGES.map((stage, i) => (
                  <PipelineStageIndicator
                    key={stage}
                    stage={stage}
                    label={getStageLabel(stage)}
                    icon={getStageIcon(stage)}
                    isCompleted={i < stageIndex}
                    isActive={i === stageIndex}
                  />
                ))}
              </div>
            </div>
          </div>

          {/* Pipeline Running Banner */}
          {!['PendingReview', 'Approved', 'Published', 'Failed', 'Rejected', 'TopicQueued'].includes(episode.status) && (
            <div className="card shadow-sm mb-4 border-info">
              <div className="card-body d-flex align-items-center gap-3">
                <div className="spinner-border spinner-border-sm text-info" role="status"></div>
                <div>
                  <h6 className="mb-0 text-info">Pipeline Running</h6>
                  <small className="text-muted">
                    Currently: {getStatusLabel(episode.status)} — Auto-refreshing every 4s
                  </small>
                </div>
              </div>
            </div>
          )}

          {/* Resume Pipeline for Failed episodes */}
          {episode.status === 'Failed' && getFailedStage() && (
            <div className="card shadow-sm mb-4 border-danger">
              <div className="card-body">
                <h6 className="mb-3 text-danger"><i className="bi bi-exclamation-triangle me-2"></i>Pipeline Failed</h6>
                {episode.currentStageError && (
                  <div className="alert alert-danger py-2 small mb-3">{episode.currentStageError}</div>
                )}
                <div className="d-flex gap-2 flex-wrap">
                  <button
                    className="btn btn-primary"
                    onClick={() => handleResume(getFailedStage())}
                    disabled={actionLoading}
                  >
                    <i className="bi bi-play-fill me-1"></i>
                    Resume from {getStageLabel(getFailedStage())}
                  </button>
                  <div className="dropdown">
                    <button className="btn btn-outline-secondary dropdown-toggle" data-bs-toggle="dropdown" disabled={actionLoading}>
                      Resume from...
                    </button>
                    <ul className="dropdown-menu">
                      {PIPELINE_STAGES.map(stage => (
                        <li key={stage}>
                          <button className="dropdown-item" onClick={() => handleResume(stage)}>
                            <i className={`bi ${getStageIcon(stage)} me-2`}></i>{getStageLabel(stage)}
                          </button>
                        </li>
                      ))}
                    </ul>
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* Actions */}
          {isActionableStatus(episode.status) && (
            <div className="card shadow-sm mb-4 border-warning">
              <div className="card-body">
                <h6 className="mb-3"><i className="bi bi-clipboard-check me-2"></i>Review Actions</h6>
                <div className="d-flex gap-2 flex-wrap">
                  <button
                    className="btn btn-success"
                    onClick={handleApprove}
                    disabled={actionLoading}
                  >
                    <i className="bi bi-check-lg me-1"></i>Approve & Schedule
                  </button>
                  {!showRejectInput ? (
                    <button
                      className="btn btn-outline-danger"
                      onClick={() => setShowRejectInput(true)}
                    >
                      <i className="bi bi-x-lg me-1"></i>Reject
                    </button>
                  ) : (
                    <div className="input-group" style={{ maxWidth: '400px' }}>
                      <input
                        type="text"
                        className="form-control"
                        placeholder="Rejection reason..."
                        value={rejectReason}
                        onChange={(e) => setRejectReason(e.target.value)}
                      />
                      <button className="btn btn-danger" onClick={handleReject} disabled={actionLoading}>
                        Reject
                      </button>
                      <button className="btn btn-outline-secondary" onClick={() => setShowRejectInput(false)}>
                        Cancel
                      </button>
                    </div>
                  )}
                </div>
                <div className="mt-3">
                  <small className="text-muted me-2">Regenerate:</small>
                  {PIPELINE_STAGES.map((stage) => (
                    <button
                      key={stage}
                      className="btn btn-sm btn-outline-secondary me-1 mb-1"
                      onClick={() => handleRegenerate(stage)}
                      disabled={actionLoading}
                    >
                      <i className={`bi ${getStageIcon(stage)} me-1`}></i>
                      {getStageLabel(stage)}
                    </button>
                  ))}
                </div>
              </div>
            </div>
          )}

          {/* Scene Gallery Preview */}
          {episode.scenes?.length > 0 && episode.scenes.some(s => s.imageUrl) && (
            <div className="card shadow-sm mb-4">
              <div className="card-header py-3">
                <h6 className="mb-0 font-headings text-uppercase small ls-wide">
                  <i className="bi bi-images me-2 text-primary"></i>Scene Preview Gallery
                </h6>
              </div>
              <div className="card-body">
                <div className="row g-3">
                  {episode.scenes.filter(s => s.imageUrl).map((scene) => (
                    <div className="col-md-6 col-lg-4" key={scene.id}>
                      <div className="card h-100 border">
                        <img
                          src={scene.imageUrl}
                          alt={`Scene ${scene.sceneNumber}`}
                          className="card-img-top"
                          style={{ height: '180px', objectFit: 'cover' }}
                        />
                        <div className="card-body p-2">
                          <div className="d-flex align-items-center gap-2 mb-1">
                            <span className="badge bg-primary bg-opacity-10 text-primary">Scene {scene.sceneNumber}</span>
                            {scene.durationSeconds > 0 && (
                              <small className="text-muted">{scene.durationSeconds}s</small>
                            )}
                          </div>
                          {scene.actionDescription && (
                            <small className="text-slate-600 d-block" style={{ fontSize: '0.75rem' }}>
                              {scene.actionDescription.length > 80
                                ? scene.actionDescription.substring(0, 80) + '...'
                                : scene.actionDescription}
                            </small>
                          )}
                          {scene.dialogueLines?.length > 0 && (
                            <div className="mt-1">
                              {scene.dialogueLines.slice(0, 2).map((dl) => (
                                <div key={dl.id} className="d-flex align-items-center gap-1" style={{ fontSize: '0.7rem' }}>
                                  <strong className="text-primary">{dl.characterName}:</strong>
                                  <span className="text-slate-500 text-truncate">{dl.text}</span>
                                  {dl.audioUrl && (
                                    <button
                                      className="btn btn-link btn-sm p-0 ms-auto"
                                      onClick={() => new Audio(dl.audioUrl).play()}
                                      title="Play audio"
                                    >
                                      <i className="bi bi-play-circle text-primary"></i>
                                    </button>
                                  )}
                                </div>
                              ))}
                              {scene.dialogueLines.length > 2 && (
                                <small className="text-muted" style={{ fontSize: '0.65rem' }}>
                                  +{scene.dialogueLines.length - 2} more lines
                                </small>
                              )}
                            </div>
                          )}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          )}

          {/* Pipeline Jobs Log */}
          <PipelineLogFeed logs={jobs.map((j) => ({
            level: j.status === 'Failed' ? 'error' : j.status === 'Completed' ? 'success' : 'info',
            message: `${getStageLabel(j.stage)} — ${j.status}${j.errorMessage ? ': ' + j.errorMessage : ''}`,
            stage: j.stage,
            timestamp: j.completedAt || j.startedAt,
          }))} />
        </div>

        {/* Right Column: Metadata */}
        <div className="col-lg-4">
          {/* Episode Info */}
          <div className="card shadow-sm mb-4">
            <div className="card-header"><h6 className="mb-0">Episode Info</h6></div>
            <div className="card-body">
              <dl className="detail-list mb-0">
                <dt>Summary</dt>
                <dd>{episode.summary || '—'}</dd>
                <dt>Moral</dt>
                <dd>{episode.moral || '—'}</dd>
                <dt>Scenes</dt>
                <dd className="fw-bold text-primary">{episode.scenes?.length || 0} Cinematic Scenes</dd>
                <dt>Created</dt>
                <dd>{formatDateTime(episode.createdAt)}</dd>
                {episode.scheduledPublishAt && (
                  <>
                    <dt>Scheduled Publish</dt>
                    <dd className="text-accent fw-bold">{formatDateTime(episode.scheduledPublishAt)}</dd>
                  </>
                )}
                {episode.youtubeUrl && (
                  <>
                    <dt>YouTube Connectivity</dt>
                    <dd>
                      <a href={episode.youtubeUrl} target="_blank" rel="noopener noreferrer" className="btn btn-sm btn-outline-danger w-100 mt-2">
                        <i className="bi bi-youtube me-2"></i>Watch on YouTube
                      </a>
                    </dd>
                  </>
                )}
              </dl>
            </div>
          </div>

          {/* SEO Info */}
          {episode.seoTitle && (
            <div className="card shadow-sm mb-4">
              <div className="card-header"><h6 className="mb-0">SEO Metadata</h6></div>
              <div className="card-body">
                <dl className="detail-list mb-0">
                  <dt>SEO Title</dt>
                  <dd className="small text-slate-700">{episode.seoTitle}</dd>
                  <dt>Description Meta</dt>
                  <dd className="small text-slate-700" style={{ maxHeight: '120px', overflowY: 'auto' }}>
                    {episode.seoDescription || '—'}
                  </dd>
                  {episode.seoTags && (
                    <>
                      <dt>Keywords / Tags</dt>
                      <dd className="d-flex flex-wrap gap-1 mt-2">
                        {(() => {
                          let tags = [];
                          if (Array.isArray(episode.seoTags)) {
                            tags = episode.seoTags;
                          } else if (typeof episode.seoTags === 'string') {
                            try {
                              tags = JSON.parse(episode.seoTags);
                            } catch {
                              tags = [];
                            }
                          }
                          return tags.map((tag, i) => (
                            <span key={i} className="badge bg-primary bg-opacity-10 text-primary border border-primary border-opacity-10 px-2 py-1">{tag}</span>
                          ));
                        })()}
                      </dd>
                    </>
                  )}
                </dl>
              </div>
            </div>
          )}

          {/* Characters */}
          {episode.characters?.length > 0 && (
            <div className="card shadow-sm">
              <div className="card-header d-flex align-items-center py-3">
                <h6 className="mb-0 font-headings text-uppercase small ls-wide">
                  <i className="bi bi-people me-2 text-primary"></i>
                  Featured Elements
                </h6>
              </div>
              <div className="card-body p-0">
                <div className="list-group list-group-flush border-0">
                  {episode.characters.map((char) => (
                    <div key={char.id} className="list-group-item d-flex align-items-center py-3 px-4 bg-transparent border-bottom border-white border-opacity-5">
                      <div
                        className="rounded-circle bg-primary bg-opacity-10 d-flex align-items-center justify-content-center me-3"
                        style={{ width: '40px', height: '40px', border: '1px solid rgba(99, 102, 241, 0.2)' }}
                      >
                        <i className="bi bi-person-fill text-primary"></i>
                      </div>
                      <div className="flex-grow-1">
                        <div className="fw-bold text-slate-900 small">{char.name}</div>
                        <div className="text-slate-500 smaller">Voice Profile Active</div>
                      </div>
                      <i className="bi bi-chevron-right text-white text-opacity-20 fs-7"></i>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Video Modal */}
      <VideoPreviewModal
        show={showVideo}
        episode={episode}
        onClose={() => setShowVideo(false)}
      />
    </div>
  );
}
