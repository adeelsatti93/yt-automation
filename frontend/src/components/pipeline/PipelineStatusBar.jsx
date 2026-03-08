import { getStageIcon, getStageLabel } from '../../utils/statusHelpers';
import PipelineStageIndicator from './PipelineStageIndicator';

const PIPELINE_STAGES = [
  'ScriptGeneration',
  'ImageGeneration',
  'VoiceGeneration',
  'MusicGeneration',
  'VideoAssembly',
  'SeoGeneration',
];

export default function PipelineStatusBar({ status }) {
  if (!status) return null;

  const { currentStage, progress = 0, isRunning, isPaused } = status;
  const currentIndex = PIPELINE_STAGES.indexOf(currentStage);

  return (
    <div className="card h-100 border-0 overflow-hidden pipeline-surface-white shadow-sm"
         style={{ 
           background: '#ffffff', 
           border: '1px solid #e2e8f0 !important',
           borderRadius: '1.25rem'
         }}>
      {/* Header with high visibility status */}
      <div className="card-header border-0 bg-transparent px-4 py-4 d-flex justify-content-between align-items-center mb-0">
        <div className="d-flex align-items-center gap-3">
          <div className="rounded-circle d-flex align-items-center justify-content-center" 
               style={{ width: '42px', height: '42px', background: '#eef2ff', color: '#4f46e5', fontSize: '1.2rem', border: '1px solid rgba(79, 70, 229, 0.1)' }}>
            <i className="bi bi-gear-wide-connected"></i>
          </div>
          <div>
            <h5 className="mb-1 font-headings fw-bold text-slate-900 text-uppercase" style={{ fontSize: '0.8125rem', letterSpacing: '0.1em' }}>
              Strategic Intelligence Flow
            </h5>
            <small className="text-slate-500 smaller fw-bold opacity-75">Operational System Pipeline</small>
          </div>
        </div>

        <div className="status-badge-container">
          {isRunning && !isPaused && (
            <div className="badge px-3 py-2 bg-primary bg-opacity-10 text-primary border border-primary border-opacity-15 fw-bold" 
                 style={{ borderRadius: '12px', letterSpacing: '0.05em' }}>
              <span className="spinner-grow spinner-grow-sm me-2" style={{ width: '6px', height: '6px' }}></span>
              PROCESSING
            </div>
          )}
          {isPaused && <div className="badge px-3 py-2 bg-amber-50 text-amber-600 border border-amber-200 fw-bold" style={{ borderRadius: '12px' }}>PAUSED</div>}
          {!isRunning && !isPaused && <div className="badge px-3 py-2 bg-slate-50 text-slate-500 border border-slate-200 fw-bold" style={{ borderRadius: '12px' }}>STANDBY</div>}
        </div>
      </div>

      <div className="card-body px-4 pb-5 pt-0">
        <div className="d-flex justify-content-between align-items-center position-relative mb-5" 
             style={{ padding: '0 1rem' }}>
          
          {/* Subtle Connection Rail */}
          <div className="position-absolute h-px bg-slate-100" style={{ left: '10%', right: '10%', top: '24px', zIndex: 0, backgroundColor: '#f1f5f9' }} />
          
          {PIPELINE_STAGES.map((stage, index) => {
            const isCompleted = currentIndex > index;
            const isActive = currentIndex === index && isRunning;
            return (
              <div key={stage} className="position-relative z-1">
                <PipelineStageIndicator
                  stage={stage}
                  isActive={isActive}
                  isCompleted={isCompleted}
                  label={getStageLabel(stage)}
                  icon={getStageIcon(stage)}
                />
              </div>
            );
          })}
        </div>

        {/* Unified Progress Tracking Section */}
        <div className="progress-section p-4 border border-slate-100 rounded-lg bg-slate-50" style={{ backgroundColor: '#fbfcfd' }}>
           <div className="d-flex justify-content-between align-items-center mb-3">
              <div className="d-flex align-items-center gap-2">
                 <i className="bi bi-activity text-indigo-500 fw-bold" style={{ color: '#6366f1' }}></i>
                 <span className="smaller text-slate-500 text-uppercase fw-extrabold ls-wide">Execution Momentum</span>
              </div>
              <span className="font-headings fw-extrabold text-indigo-600" style={{ fontSize: '1.1rem', color: '#4f46e5' }}>{Math.round(progress)}%</span>
           </div>
           
           <div className="progress rounded-pill bg-slate-200 mb-0" style={{ height: '8px', backgroundColor: '#e2e8f0' }}>
             <div
               className={`progress-bar rounded-pill ${isRunning && !isPaused ? 'progress-bar-striped progress-bar-animated' : ''} ${isPaused ? 'bg-warning' : 'bg-primary'}`}
               role="progressbar"
               style={{ 
                  width: `${progress}%`, 
                  boxShadow: isRunning ? '0 0 10px rgba(79, 70, 229, 0.2)' : 'none',
                  transition: 'width 0.8s cubic-bezier(0.16, 1, 0.3, 1)' 
               }}
             />
           </div>
        </div>
      </div>
    </div>
  );
}
