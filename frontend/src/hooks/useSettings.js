import { useState, useEffect, useCallback } from 'react';
import { settingsApi } from '../api/settingsApi';

export function useSettings(autoLoad = true) {
  const [settings, setSettings] = useState({});
  const [groupedSettings, setGroupedSettings] = useState({});
  const [connectionResults, setConnectionResults] = useState({});
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [testing, setTesting] = useState({});
  const [error, setError] = useState(null);

  const groupByCategory = useCallback((settingsData) => {
    const grouped = {};
    if (Array.isArray(settingsData)) {
      settingsData.forEach((setting) => {
        const category = setting.category || 'general';
        if (!grouped[category]) {
          grouped[category] = [];
        }
        grouped[category].push(setting);
      });
    } else if (typeof settingsData === 'object' && settingsData !== null) {
      Object.entries(settingsData).forEach(([key, value]) => {
        const category = value?.category || key;
        if (!grouped[category]) {
          grouped[category] = {};
        }
        if (value?.category) {
          grouped[category][key] = value;
        } else {
          grouped[category] = value;
        }
      });
    }
    return grouped;
  }, []);

  const loadSettings = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await settingsApi.getAll();
      const data = response.data.settings || response.data;
      setSettings(data);
      setGroupedSettings(groupByCategory(data));
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to load settings');
    } finally {
      setLoading(false);
    }
  }, [groupByCategory]);

  useEffect(() => {
    if (autoLoad) {
      loadSettings();
    }
  }, [autoLoad, loadSettings]);

  const updateSetting = useCallback(async (key, value) => {
    setSaving(true);
    setError(null);
    try {
      const response = await settingsApi.update(key, { value });
      const updated = response.data;
      setSettings((prev) => {
        let next;
        if (Array.isArray(prev)) {
          next = prev.map((s) => (s.key === key ? { ...s, ...updated } : s));
        } else {
          next = { ...prev, [key]: updated.value !== undefined ? updated.value : updated };
        }
        setGroupedSettings(groupByCategory(next));
        return next;
      });
      return updated;
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to update setting');
      throw err;
    } finally {
      setSaving(false);
    }
  }, [groupByCategory]);

  const batchUpdate = useCallback(async (updates) => {
    setSaving(true);
    setError(null);
    try {
      const response = await settingsApi.batchUpdate(updates);
      const data = response.data.settings || response.data;
      setSettings(data);
      setGroupedSettings(groupByCategory(data));
      return data;
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to batch update settings');
      throw err;
    } finally {
      setSaving(false);
    }
  }, [groupByCategory]);

  const testConnection = useCallback(async (service) => {
    setTesting((prev) => ({ ...prev, [service]: true }));
    setError(null);
    try {
      const response = await settingsApi.testConnection(service);
      const result = {
        success: response.data.success ?? response.data.connected ?? true,
        message: response.data.message || 'Connection successful',
        testedAt: new Date().toISOString(),
      };
      setConnectionResults((prev) => ({ ...prev, [service]: result }));
      return result;
    } catch (err) {
      const result = {
        success: false,
        message: err.response?.data?.message || err.message || 'Connection failed',
        testedAt: new Date().toISOString(),
      };
      setConnectionResults((prev) => ({ ...prev, [service]: result }));
      return result;
    } finally {
      setTesting((prev) => ({ ...prev, [service]: false }));
    }
  }, []);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  const clearConnectionResults = useCallback(() => {
    setConnectionResults({});
  }, []);

  return {
    settings,
    groupedSettings,
    connectionResults,
    loading,
    saving,
    testing,
    error,
    loadSettings,
    updateSetting,
    batchUpdate,
    testConnection,
    clearError,
    clearConnectionResults,
  };
}

export default useSettings;
