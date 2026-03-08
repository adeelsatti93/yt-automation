/**
 * Validation utility functions for the Kids Cartoon Pipeline app.
 */

/**
 * Validate that a value is present (not empty / whitespace-only).
 * @param {*} value - The value to validate.
 * @param {string} fieldName - Human-readable field name for the error message.
 * @returns {string|null} Error message or null if valid.
 */
export function validateRequired(value, fieldName) {
  if (value == null || String(value).trim() === '') {
    return `${fieldName} is required.`;
  }
  return null;
}

/**
 * Validate that a string meets a minimum length requirement.
 * @param {string} value - The value to validate.
 * @param {number} min - Minimum allowed length.
 * @param {string} fieldName - Human-readable field name for the error message.
 * @returns {string|null} Error message or null if valid.
 */
export function validateMinLength(value, min, fieldName) {
  if (value == null) return null;
  if (String(value).trim().length < min) {
    return `${fieldName} must be at least ${min} characters.`;
  }
  return null;
}

/**
 * Validate that a string does not exceed a maximum length.
 * @param {string} value - The value to validate.
 * @param {number} max - Maximum allowed length.
 * @param {string} fieldName - Human-readable field name for the error message.
 * @returns {string|null} Error message or null if valid.
 */
export function validateMaxLength(value, max, fieldName) {
  if (value == null) return null;
  if (String(value).length > max) {
    return `${fieldName} must be no more than ${max} characters.`;
  }
  return null;
}

/**
 * Validate that a value is a well-formed URL.
 * @param {string} value - The URL string to validate.
 * @returns {string|null} Error message or null if valid.
 */
export function validateUrl(value) {
  if (!value || String(value).trim() === '') return null; // not required by default
  try {
    const url = new URL(value);
    if (!['http:', 'https:'].includes(url.protocol)) {
      return 'URL must start with http:// or https://.';
    }
    return null;
  } catch {
    return 'Please enter a valid URL.';
  }
}

/**
 * Basic validation for an API key (non-empty, minimum length of 10).
 * @param {string} value - The API key to validate.
 * @returns {string|null} Error message or null if valid.
 */
export function validateApiKey(value) {
  if (!value || String(value).trim() === '') {
    return 'API key is required.';
  }
  if (String(value).trim().length < 10) {
    return 'API key must be at least 10 characters.';
  }
  return null;
}

/**
 * Validate the character creation / edit form.
 * @param {object} data - Form data with { name, description, voiceId }.
 * @returns {{ isValid: boolean, errors: object }} Validation result.
 */
export function validateCharacterForm(data = {}) {
  const errors = {};

  const nameError = validateRequired(data.name, 'Name');
  if (nameError) errors.name = nameError;

  const descriptionError = validateRequired(data.description, 'Description');
  if (descriptionError) errors.description = descriptionError;

  // voiceId is optional but if provided must be non-empty
  if (data.voiceId !== undefined && data.voiceId !== null && String(data.voiceId).trim() === '') {
    errors.voiceId = 'Voice ID must not be blank if provided.';
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
  };
}

/**
 * Validate the topic creation / edit form.
 * @param {object} data - Form data with { title, description }.
 * @returns {{ isValid: boolean, errors: object }} Validation result.
 */
export function validateTopicForm(data = {}) {
  const errors = {};

  const titleError = validateRequired(data.title, 'Title');
  if (titleError) errors.title = titleError;

  // description is optional but if provided should not be only whitespace
  if (data.description !== undefined && data.description !== null) {
    const trimmed = String(data.description).trim();
    if (trimmed.length > 0) {
      const maxLengthError = validateMaxLength(data.description, 2000, 'Description');
      if (maxLengthError) errors.description = maxLengthError;
    }
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
  };
}

/**
 * Validate the application settings form.
 * Checks that API keys are present for required services.
 * @param {object} settings - Settings object with service API keys.
 * @returns {{ isValid: boolean, errors: object }} Validation result.
 */
export function validateSettingsForm(settings = {}) {
  const errors = {};

  // Required service API keys
  const requiredKeys = [
    { field: 'openAiApiKey', label: 'OpenAI API Key' },
    { field: 'elevenLabsApiKey', label: 'ElevenLabs API Key' },
  ];

  for (const { field, label } of requiredKeys) {
    const keyError = validateApiKey(settings[field]);
    if (keyError) {
      errors[field] = `${label}: ${keyError}`;
    }
  }

  // Optional but validated if present
  const optionalKeys = [
    { field: 'youtubeApiKey', label: 'YouTube API Key' },
    { field: 'stabilityApiKey', label: 'Stability API Key' },
  ];

  for (const { field, label } of optionalKeys) {
    if (settings[field] && String(settings[field]).trim() !== '') {
      const keyError = validateApiKey(settings[field]);
      if (keyError) {
        errors[field] = `${label}: ${keyError}`;
      }
    }
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
  };
}
