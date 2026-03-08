import { useState, useEffect } from 'react';
import { validateCharacterForm } from '../../utils/validators';
import ErrorAlert from '../shared/ErrorAlert';

const INITIAL_STATE = {
  name: '',
  description: '',
  personality: '',
  voiceId: '',
  voiceSettings: '',
};

export default function CharacterForm({ character, voices = [], onSubmit, onCancel, loading }) {
  const [formData, setFormData] = useState(INITIAL_STATE);
  const [errors, setErrors] = useState({});
  const [submitError, setSubmitError] = useState('');

  const isEditing = character !== null && character !== undefined;

  useEffect(() => {
    if (character) {
      setFormData({
        name: character.name || '',
        description: character.description || '',
        personality: character.personality || '',
        voiceId: character.voiceId || '',
        voiceSettings: character.voiceSettings
          ? (typeof character.voiceSettings === 'string'
              ? character.voiceSettings
              : JSON.stringify(character.voiceSettings, null, 2))
          : '',
      });
    } else {
      setFormData(INITIAL_STATE);
    }
    setErrors({});
    setSubmitError('');
  }, [character]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    if (errors[name]) {
      setErrors((prev) => ({ ...prev, [name]: undefined }));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitError('');

    const { isValid, errors: validationErrors } = validateCharacterForm(formData);
    if (!isValid) {
      setErrors(validationErrors);
      return;
    }

    // Validate voiceSettings JSON if provided
    let parsedVoiceSettings = null;
    if (formData.voiceSettings.trim()) {
      try {
        parsedVoiceSettings = JSON.parse(formData.voiceSettings);
      } catch {
        setErrors((prev) => ({ ...prev, voiceSettings: 'Invalid JSON format.' }));
        return;
      }
    }

    try {
      await onSubmit({
        ...formData,
        voiceSettings: parsedVoiceSettings,
      });
    } catch (err) {
      setSubmitError(err.message || 'Failed to save character.');
    }
  };

  return (
    <form onSubmit={handleSubmit} noValidate>
      <ErrorAlert message={submitError} />

      <div className="mb-3">
        <label htmlFor="char-name" className="form-label">
          Name <span className="text-danger">*</span>
        </label>
        <input
          type="text"
          id="char-name"
          name="name"
          className={`form-control ${errors.name ? 'is-invalid' : ''}`}
          value={formData.name}
          onChange={handleChange}
          placeholder="e.g. Captain Sparkle"
          disabled={loading}
        />
        {errors.name && <div className="invalid-feedback">{errors.name}</div>}
      </div>

      <div className="mb-3">
        <label htmlFor="char-description" className="form-label">
          Description <span className="text-danger">*</span>
        </label>
        <textarea
          id="char-description"
          name="description"
          className={`form-control ${errors.description ? 'is-invalid' : ''}`}
          rows="3"
          value={formData.description}
          onChange={handleChange}
          placeholder="Describe the character's appearance and role..."
          disabled={loading}
        />
        {errors.description && <div className="invalid-feedback">{errors.description}</div>}
      </div>

      <div className="mb-3">
        <label htmlFor="char-personality" className="form-label">
          Personality
        </label>
        <textarea
          id="char-personality"
          name="personality"
          className="form-control"
          rows="2"
          value={formData.personality}
          onChange={handleChange}
          placeholder="Fun, adventurous, loves exploring..."
          disabled={loading}
        />
      </div>

      <div className="mb-3">
        <label htmlFor="char-voiceId" className="form-label">
          Voice
        </label>
        <select
          id="char-voiceId"
          name="voiceId"
          className={`form-select ${errors.voiceId ? 'is-invalid' : ''}`}
          value={formData.voiceId}
          onChange={handleChange}
          disabled={loading}
        >
          <option value="">Select a voice...</option>
          {voices.map((voice) => (
            <option key={voice.id || voice.voice_id} value={voice.id || voice.voice_id}>
              {voice.name}
            </option>
          ))}
        </select>
        {errors.voiceId && <div className="invalid-feedback">{errors.voiceId}</div>}
      </div>

      <div className="mb-3">
        <label htmlFor="char-voiceSettings" className="form-label">
          Voice Settings (JSON)
        </label>
        <textarea
          id="char-voiceSettings"
          name="voiceSettings"
          className={`form-control font-monospace ${errors.voiceSettings ? 'is-invalid' : ''}`}
          rows="3"
          value={formData.voiceSettings}
          onChange={handleChange}
          placeholder='{"stability": 0.5, "similarity_boost": 0.75}'
          disabled={loading}
          style={{ fontSize: '0.85rem' }}
        />
        {errors.voiceSettings && <div className="invalid-feedback">{errors.voiceSettings}</div>}
      </div>

      <div className="d-flex justify-content-end gap-2">
        <button type="button" className="btn btn-secondary" onClick={onCancel} disabled={loading}>
          Cancel
        </button>
        <button type="submit" className="btn btn-primary" disabled={loading}>
          {loading ? (
            <>
              <span className="spinner-border spinner-border-sm me-1"></span>
              Saving...
            </>
          ) : (
            isEditing ? 'Update Character' : 'Create Character'
          )}
        </button>
      </div>
    </form>
  );
}
