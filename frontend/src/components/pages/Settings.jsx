import { useEffect, useState } from 'react';
import { useApp } from '../../context/AppContext';
import { useSettings } from '../../hooks/useSettings';
import api from '../../api/axiosInstance';
import ApiKeyField from '../settings/ApiKeyField';
import SettingsSection from '../settings/SettingsSection';
import LoadingSpinner from '../shared/LoadingSpinner';
import ErrorAlert from '../shared/ErrorAlert';
import { toast } from 'react-toastify';

const API_KEY_SERVICES = [
  { key: 'Anthropic:ApiKey', label: 'Anthropic (Claude)', description: 'Script generation & image prompts', helpUrl: 'https://console.anthropic.com/account/keys' },
  { key: 'OpenAI:ApiKey', label: 'OpenAI (DALL·E 3)', description: 'Image generation', helpUrl: 'https://platform.openai.com/api-keys' },
  { key: 'ElevenLabs:ApiKey', label: 'ElevenLabs', description: 'Voice / TTS generation', helpUrl: 'https://elevenlabs.io/subscription' },
  { key: 'Suno:ApiKey', label: 'Suno', description: 'Background music generation (optional)', helpUrl: null },
  { key: 'Fal:ApiKey', label: 'Fal.ai (Kling Animation)', description: 'AI cartoon animation with lip sync — optional, needed for Kling video engine', helpUrl: 'https://fal.ai/dashboard' },
];

const TABS = [
  { id: 'apikeys', label: 'API Keys', icon: 'bi-key' },
  { id: 'pipeline', label: 'Pipeline', icon: 'bi-gear' },
  { id: 'images', label: 'Image Style', icon: 'bi-palette' },
  { id: 'prompts', label: 'Prompts', icon: 'bi-chat-left-text' },
  { id: 'youtube', label: 'YouTube', icon: 'bi-youtube' },
];

export default function Settings() {
  const { setPageTitle } = useApp();
  const {
    settings,
    connectionResults,
    loading,
    saving,
    testing,
    error,
    loadSettings,
    updateSetting,
    testConnection,
  } = useSettings();

  const [activeTab, setActiveTab] = useState('apikeys');
  const [localValues, setLocalValues] = useState({});
  const [ytAuthorizing, setYtAuthorizing] = useState(false);
  const [ytConnected, setYtConnected] = useState(false);

  // Listen for YouTube OAuth callback message
  useEffect(() => {
    const handler = (event) => {
      if (event.data?.type === 'youtube-auth-success') {
        setYtConnected(true);
        setYtAuthorizing(false);
        toast.success('YouTube connected successfully!');
      }
    };
    window.addEventListener('message', handler);
    return () => window.removeEventListener('message', handler);
  }, []);

  const handleYouTubeAuth = async () => {
    setYtAuthorizing(true);
    try {
      const res = await api.get('/youtube/auth-url');
      window.open(res.data.url, '_blank', 'width=600,height=700');
    } catch (err) {
      toast.error('Failed to get authorization URL. Make sure YouTube Client ID and Secret are saved first.');
      setYtAuthorizing(false);
    }
  };

  useEffect(() => {
    setPageTitle('Settings');
  }, [setPageTitle]);

  // Build a lookup from settings array or object
  const getValue = (key) => {
    if (localValues[key] !== undefined) return localValues[key];
    if (Array.isArray(settings)) {
      const s = settings.find((s) => s.key === key);
      return s?.value || '';
    }
    return settings[key]?.value || settings[key] || '';
  };

  const handleSave = async (key, value) => {
    setLocalValues((prev) => ({ ...prev, [key]: value }));
    await updateSetting(key, value);
  };

  const handleFieldChange = (key, value) => {
    setLocalValues((prev) => ({ ...prev, [key]: value }));
  };

  const handleFieldSave = async (key) => {
    if (localValues[key] !== undefined) {
      await updateSetting(key, localValues[key]);
    }
  };

  if (loading) return <LoadingSpinner text="Loading settings..." />;

  // Calculate setup progress
  const configured = API_KEY_SERVICES.filter((s) => !!getValue(s.key)).length;
  const setupProgress = Math.round((configured / API_KEY_SERVICES.length) * 100);

  return (
    <div className="container-fluid">
      {error && <ErrorAlert message={error} onRetry={loadSettings} />}

      {/* Tabs */}
      <ul className="nav nav-tabs mb-4">
        {TABS.map((tab) => (
          <li className="nav-item" key={tab.id}>
            <button
              className={`nav-link ${activeTab === tab.id ? 'active' : ''}`}
              onClick={() => setActiveTab(tab.id)}
            >
              <i className={`bi ${tab.icon} me-1`}></i>
              {tab.label}
            </button>
          </li>
        ))}
      </ul>

      {/* API Keys Tab */}
      {activeTab === 'apikeys' && (
        <div>
          {/* Setup Progress */}
          <div className="card shadow-sm mb-4">
            <div className="card-body">
              <div className="d-flex justify-content-between mb-2">
                <span className="fw-bold">Setup Progress</span>
                <span className="text-muted">{configured}/{API_KEY_SERVICES.length} configured</span>
              </div>
              <div className="progress" style={{ height: '8px' }}>
                <div
                  className={`progress-bar ${setupProgress === 100 ? 'bg-success' : 'bg-primary'}`}
                  style={{ width: `${setupProgress}%` }}
                ></div>
              </div>
            </div>
          </div>

          {API_KEY_SERVICES.map((service) => (
            <ApiKeyField
              key={service.key}
              label={service.label}
              settingKey={service.key}
              value={getValue(service.key)}
              description={service.description}
              helpUrl={service.helpUrl}
              onSave={handleSave}
              testResult={connectionResults[service.key.split(':')[0]]}
              onTest={testConnection}
              testing={testing[service.key.split(':')[0]]}
              saving={saving}
            />
          ))}

          {/* YouTube OAuth */}
          <div className="card mb-3 border">
            <div className="card-body">
              <div className="d-flex justify-content-between align-items-start mb-3">
                <div>
                  <h6 className="mb-1"><i className="bi bi-youtube text-danger me-2"></i>YouTube Connection</h6>
                  <small className="text-muted">Enter your Google OAuth credentials, then authorize</small>
                </div>
                {ytConnected && (
                  <span className="badge bg-success">✅ Connected</span>
                )}
              </div>

              <div className="row g-3 mb-3">
                <div className="col-md-6">
                  <label className="form-label small fw-bold">Client ID</label>
                  <input
                    type="text"
                    className="form-control"
                    value={localValues['YouTube:ClientId'] ?? getValue('YouTube:ClientId')}
                    onChange={(e) => handleFieldChange('YouTube:ClientId', e.target.value)}
                    onBlur={() => handleFieldSave('YouTube:ClientId')}
                    placeholder="xxxx.apps.googleusercontent.com"
                  />
                  <a href="https://console.cloud.google.com/apis/credentials" target="_blank" rel="noopener noreferrer" className="small">
                    <i className="bi bi-box-arrow-up-right me-1"></i>Google Cloud Console
                  </a>
                </div>
                <div className="col-md-6">
                  <label className="form-label small fw-bold">Client Secret</label>
                  <input
                    type="password"
                    className="form-control"
                    value={localValues['YouTube:ClientSecret'] ?? getValue('YouTube:ClientSecret')}
                    onChange={(e) => handleFieldChange('YouTube:ClientSecret', e.target.value)}
                    onBlur={() => handleFieldSave('YouTube:ClientSecret')}
                    placeholder="GOCSPX-..."
                  />
                </div>
              </div>

              <button
                className="btn btn-danger"
                onClick={handleYouTubeAuth}
                disabled={ytAuthorizing || !getValue('YouTube:ClientId') || !getValue('YouTube:ClientSecret')}
              >
                {ytAuthorizing ? (
                  <><span className="spinner-border spinner-border-sm me-2"></span>Waiting for authorization...</>
                ) : (
                  <><i className="bi bi-youtube me-2"></i>Authorize with YouTube</>
                )}
              </button>
              {(!getValue('YouTube:ClientId') || !getValue('YouTube:ClientSecret')) && (
                <small className="text-warning d-block mt-2">
                  <i className="bi bi-exclamation-triangle me-1"></i>
                  Enter Client ID and Secret above first.
                </small>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Pipeline Tab */}
      {activeTab === 'pipeline' && (
        <SettingsSection title="Pipeline Configuration" icon="bi-gear">
          <div className="row g-3">
            {/* Video Engine Selector */}
            <div className="col-12">
              <div className="card border mb-1">
                <div className="card-body py-3">
                  <h6 className="mb-3 fw-semibold"><i className="bi bi-camera-video me-2 text-primary"></i>Video Engine</h6>
                  <div className="row g-3">
                    <div className="col-md-6">
                      <div
                        className={`p-3 rounded h-100 ${getValue('Video:Provider') !== 'Kling' ? 'border border-primary bg-primary bg-opacity-10' : 'border'}`}
                        style={{ cursor: 'pointer' }}
                        onClick={() => handleSave('Video:Provider', 'FFmpeg')}
                      >
                        <div className="d-flex align-items-center gap-2 mb-2">
                          <i className="bi bi-film text-primary fs-4"></i>
                          <div>
                            <div className="fw-bold">FFmpeg 2D</div>
                            <small className="text-success fw-semibold">Free</small>
                          </div>
                        </div>
                        <small className="text-muted">Ken Burns zoom/pan, animated subtitles, background music mixing. No extra cost.</small>
                      </div>
                    </div>
                    <div className="col-md-6">
                      <div
                        className={`p-3 rounded h-100 ${getValue('Video:Provider') === 'Kling' ? 'border border-primary bg-primary bg-opacity-10' : 'border'}`}
                        style={{ cursor: 'pointer' }}
                        onClick={() => handleSave('Video:Provider', 'Kling')}
                      >
                        <div className="d-flex align-items-center gap-2 mb-2">
                          <i className="bi bi-stars text-warning fs-4"></i>
                          <div>
                            <div className="fw-bold">Kling AI Animation</div>
                            <small className="text-warning fw-semibold">~$6/episode</small>
                          </div>
                        </div>
                        <small className="text-muted">Real AI cartoon animation with character movement + lip sync. Requires Fal.ai API key in the API Keys tab.</small>
                        {getValue('Video:Provider') === 'Kling' && !getValue('Fal:ApiKey') && (
                          <div className="mt-2"><small className="text-danger"><i className="bi bi-exclamation-triangle me-1"></i>Add your Fal.ai API key in API Keys tab.</small></div>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            {/* Pipeline fields */}
            <div className="col-md-6">
              <label className="form-label">Auto-run Pipeline</label>
              <select
                className="form-select"
                value={getValue('Pipeline:IsActive')}
                onChange={(e) => handleFieldChange('Pipeline:IsActive', e.target.value)}
                onBlur={() => handleFieldSave('Pipeline:IsActive')}
              >
                <option value="true">Enabled</option>
                <option value="false">Disabled</option>
              </select>
            </div>
            <div className="col-md-6">
              <label className="form-label">Schedule (Cron Expression)</label>
              <input
                type="text"
                className="form-control font-monospace"
                value={localValues['Pipeline:CronSchedule'] ?? getValue('Pipeline:CronSchedule')}
                onChange={(e) => handleFieldChange('Pipeline:CronSchedule', e.target.value)}
                onBlur={() => handleFieldSave('Pipeline:CronSchedule')}
                placeholder="0 */6 * * *"
              />
              <small className="form-text text-muted">Default: every 6 hours</small>
            </div>
            <div className="col-md-6">
              <label className="form-label">Max Concurrent Episodes</label>
              <input
                type="number"
                className="form-control"
                min="1"
                max="5"
                value={localValues['Pipeline:MaxConcurrent'] ?? getValue('Pipeline:MaxConcurrent')}
                onChange={(e) => handleFieldChange('Pipeline:MaxConcurrent', e.target.value)}
                onBlur={() => handleFieldSave('Pipeline:MaxConcurrent')}
              />
            </div>
            <div className="col-md-6">
              <label className="form-label">Default Publish Time</label>
              <input
                type="time"
                className="form-control"
                value={localValues['Pipeline:DefaultPublishTime'] ?? getValue('Pipeline:DefaultPublishTime')}
                onChange={(e) => handleFieldChange('Pipeline:DefaultPublishTime', e.target.value)}
                onBlur={() => handleFieldSave('Pipeline:DefaultPublishTime')}
              />
            </div>
            <div className="col-12">
              <label className="form-label">Publish Days</label>
              <input
                type="text"
                className="form-control"
                value={localValues['Pipeline:PublishDays'] ?? getValue('Pipeline:PublishDays')}
                onChange={(e) => handleFieldChange('Pipeline:PublishDays', e.target.value)}
                onBlur={() => handleFieldSave('Pipeline:PublishDays')}
                placeholder="Saturday,Sunday"
              />
              <small className="form-text text-muted">Comma-separated day names</small>
            </div>
          </div>
        </SettingsSection>
      )}

      {/* Image Style Tab */}
      {activeTab === 'images' && (
        <SettingsSection title="Image Generation Settings" icon="bi-palette">
          <div className="mb-3">
            <label className="form-label">Global Art Style Prompt</label>
            <textarea
              className="form-control"
              rows="4"
              value={localValues['Images:GlobalStyle'] ?? getValue('Images:GlobalStyle')}
              onChange={(e) => handleFieldChange('Images:GlobalStyle', e.target.value)}
              onBlur={() => handleFieldSave('Images:GlobalStyle')}
              placeholder="2D flat cartoon, bright saturated colors..."
            />
            <small className="form-text text-muted">
              This style description is prepended to every image generation prompt.
            </small>
          </div>
          <div className="row g-3">
            <div className="col-md-6">
              <label className="form-label">Image Quality</label>
              <select
                className="form-select"
                value={localValues['Images:Quality'] ?? getValue('Images:Quality')}
                onChange={(e) => handleFieldChange('Images:Quality', e.target.value)}
                onBlur={() => handleFieldSave('Images:Quality')}
              >
                <option value="standard">Standard</option>
                <option value="hd">HD</option>
              </select>
            </div>
            <div className="col-md-6">
              <label className="form-label">Image Size</label>
              <select
                className="form-select"
                value={localValues['Images:Size'] ?? getValue('Images:Size')}
                onChange={(e) => handleFieldChange('Images:Size', e.target.value)}
                onBlur={() => handleFieldSave('Images:Size')}
              >
                <option value="1024x1024">1024x1024 (Square)</option>
                <option value="1792x1024">1792x1024 (Landscape)</option>
                <option value="1024x1792">1024x1792 (Portrait)</option>
              </select>
            </div>
          </div>
        </SettingsSection>
      )}

      {/* Prompts Tab */}
      {activeTab === 'prompts' && (
        <div>
          <SettingsSection title="Script Generation Prompt" icon="bi-file-earmark-text">
            <textarea
              className="form-control font-monospace"
              rows="8"
              value={localValues['Prompts:Script'] ?? getValue('Prompts:Script')}
              onChange={(e) => handleFieldChange('Prompts:Script', e.target.value)}
              onBlur={() => handleFieldSave('Prompts:Script')}
              style={{ fontSize: '0.85rem' }}
            />
            <small className="form-text text-muted">
              Variables: {'{topic}'}, {'{characters_json}'}, {'{moral}'}
            </small>
          </SettingsSection>

          <SettingsSection title="Image Prompt Builder" icon="bi-image">
            <textarea
              className="form-control font-monospace"
              rows="6"
              value={localValues['Prompts:ImageBuilder'] ?? getValue('Prompts:ImageBuilder')}
              onChange={(e) => handleFieldChange('Prompts:ImageBuilder', e.target.value)}
              onBlur={() => handleFieldSave('Prompts:ImageBuilder')}
              style={{ fontSize: '0.85rem' }}
            />
          </SettingsSection>

          <SettingsSection title="SEO Title Prompt" icon="bi-search">
            <textarea
              className="form-control font-monospace"
              rows="4"
              value={localValues['Prompts:SeoTitle'] ?? getValue('Prompts:SeoTitle')}
              onChange={(e) => handleFieldChange('Prompts:SeoTitle', e.target.value)}
              onBlur={() => handleFieldSave('Prompts:SeoTitle')}
              style={{ fontSize: '0.85rem' }}
            />
          </SettingsSection>

          <SettingsSection title="SEO Description Prompt" icon="bi-card-text">
            <textarea
              className="form-control font-monospace"
              rows="4"
              value={localValues['Prompts:SeoDescription'] ?? getValue('Prompts:SeoDescription')}
              onChange={(e) => handleFieldChange('Prompts:SeoDescription', e.target.value)}
              onBlur={() => handleFieldSave('Prompts:SeoDescription')}
              style={{ fontSize: '0.85rem' }}
            />
          </SettingsSection>
        </div>
      )}

      {/* YouTube Tab */}
      {activeTab === 'youtube' && (
        <SettingsSection title="YouTube Upload Settings" icon="bi-youtube">
          <div className="row g-3">
            <div className="col-md-6">
              <label className="form-label">Video Category ID</label>
              <input
                type="text"
                className="form-control"
                value={localValues['YouTube:CategoryId'] ?? getValue('YouTube:CategoryId')}
                onChange={(e) => handleFieldChange('YouTube:CategoryId', e.target.value)}
                onBlur={() => handleFieldSave('YouTube:CategoryId')}
                placeholder="27"
              />
              <small className="form-text text-muted">27 = Education</small>
            </div>
            <div className="col-md-6">
              <label className="form-label">Default Privacy</label>
              <select
                className="form-select"
                value={localValues['YouTube:Privacy'] ?? getValue('YouTube:Privacy')}
                onChange={(e) => handleFieldChange('YouTube:Privacy', e.target.value)}
                onBlur={() => handleFieldSave('YouTube:Privacy')}
              >
                <option value="public">Public</option>
                <option value="private">Private</option>
                <option value="unlisted">Unlisted</option>
              </select>
            </div>
            <div className="col-md-6">
              <label className="form-label">Made For Kids</label>
              <select
                className="form-select"
                value={localValues['YouTube:MadeForKids'] ?? getValue('YouTube:MadeForKids')}
                onChange={(e) => handleFieldChange('YouTube:MadeForKids', e.target.value)}
                onBlur={() => handleFieldSave('YouTube:MadeForKids')}
              >
                <option value="true">Yes</option>
                <option value="false">No</option>
              </select>
            </div>
            <div className="col-md-6">
              <label className="form-label">Auto-Publish</label>
              <select
                className="form-select"
                value={localValues['YouTube:AutoPublish'] ?? getValue('YouTube:AutoPublish')}
                onChange={(e) => handleFieldChange('YouTube:AutoPublish', e.target.value)}
                onBlur={() => handleFieldSave('YouTube:AutoPublish')}
              >
                <option value="true">Enabled</option>
                <option value="false">Disabled</option>
              </select>
            </div>
            <div className="col-12">
              <label className="form-label">Description Suffix</label>
              <textarea
                className="form-control"
                rows="3"
                value={localValues['YouTube:DescriptionSuffix'] ?? getValue('YouTube:DescriptionSuffix')}
                onChange={(e) => handleFieldChange('YouTube:DescriptionSuffix', e.target.value)}
                onBlur={() => handleFieldSave('YouTube:DescriptionSuffix')}
                placeholder="Hashtags and links appended to every description..."
              />
            </div>
          </div>
        </SettingsSection>
      )}
    </div>
  );
}
