import { useState, useEffect } from 'react';
import ErrorAlert from '../shared/ErrorAlert';

const INITIAL_STATE = {
  title: '',
  description: '',
  targetMoral: '',
};

export default function TopicForm({ topic, onSubmit, onCancel, loading }) {
  const [formData, setFormData] = useState(INITIAL_STATE);
  const [errors, setErrors] = useState({});
  const [submitError, setSubmitError] = useState('');

  const isEditing = topic !== null && topic !== undefined;

  useEffect(() => {
    if (topic) {
      setFormData({
        title: topic.title || '',
        description: topic.description || '',
        targetMoral: topic.targetMoral || '',
      });
    } else {
      setFormData(INITIAL_STATE);
    }
    setErrors({});
    setSubmitError('');
  }, [topic]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    if (errors[name]) {
      setErrors((prev) => ({ ...prev, [name]: undefined }));
    }
  };

  const validate = () => {
    const newErrors = {};
    if (!formData.title.trim()) newErrors.title = 'Title is required.';
    return newErrors;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitError('');

    const validationErrors = validate();
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    try {
      await onSubmit(formData);
    } catch (err) {
      setSubmitError(err.message || 'Failed to save topic.');
    }
  };

  return (
    <form onSubmit={handleSubmit} noValidate>
      <ErrorAlert message={submitError} />

      <div className="mb-3">
        <label htmlFor="topic-title" className="form-label">
          Title <span className="text-danger">*</span>
        </label>
        <input
          type="text"
          id="topic-title"
          name="title"
          className={`form-control ${errors.title ? 'is-invalid' : ''}`}
          value={formData.title}
          onChange={handleChange}
          placeholder="e.g. Luna the Bunny learns to share her carrots"
          disabled={loading}
        />
        {errors.title && <div className="invalid-feedback">{errors.title}</div>}
        <small className="form-text text-muted">
          💡 Tip: Good topic titles include a character name and an action.
        </small>
      </div>

      <div className="mb-3">
        <label htmlFor="topic-description" className="form-label">
          Description
        </label>
        <textarea
          id="topic-description"
          name="description"
          className="form-control"
          rows="2"
          value={formData.description}
          onChange={handleChange}
          placeholder="Optional details about the episode concept..."
          disabled={loading}
        />
      </div>

      <div className="mb-3">
        <label htmlFor="topic-moral" className="form-label">
          Target Moral / Lesson
        </label>
        <input
          type="text"
          id="topic-moral"
          name="targetMoral"
          className="form-control"
          value={formData.targetMoral}
          onChange={handleChange}
          placeholder="e.g. Sharing is caring"
          disabled={loading}
        />
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
            isEditing ? 'Update Topic' : 'Create Topic'
          )}
        </button>
      </div>
    </form>
  );
}
