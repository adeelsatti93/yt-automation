import { useState, useEffect, useCallback, useRef } from 'react';
import { pipelineApi } from '../api/pipelineApi';

const POLL_INTERVAL_MS = 5000;

export function usePipeline() {
  const [isRunning, setIsRunning] = useState(false);
  const [currentStage, setCurrentStage] = useState(null);
  const [progress, setProgress] = useState(0);
  const [status, setStatus] = useState(null);
  const [logs, setLogs] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const pollTimerRef = useRef(null);
  const isMountedRef = useRef(true);

  const updateFromStatus = useCallback((data) => {
    if (!isMountedRef.current) return;
    setStatus(data);
    setIsRunning(data.isRunning ?? data.status === 'running');
    setCurrentStage(data.currentStage ?? data.stage ?? null);
    setProgress(data.progress ?? 0);
  }, []);

  const getStatus = useCallback(async () => {
    try {
      const response = await pipelineApi.getStatus();
      updateFromStatus(response.data);
      return response.data;
    } catch (err) {
      if (isMountedRef.current) {
        setError(err.response?.data?.message || err.message || 'Failed to get pipeline status');
      }
      return null;
    }
  }, [updateFromStatus]);

  const stopPolling = useCallback(() => {
    if (pollTimerRef.current) {
      clearInterval(pollTimerRef.current);
      pollTimerRef.current = null;
    }
  }, []);

  const startPolling = useCallback(() => {
    stopPolling();
    pollTimerRef.current = setInterval(async () => {
      const data = await getStatus();
      if (data && !(data.isRunning ?? data.status === 'running')) {
        stopPolling();
      }
    }, POLL_INTERVAL_MS);
  }, [getStatus, stopPolling]);

  useEffect(() => {
    if (isRunning) {
      startPolling();
    } else {
      stopPolling();
    }
  }, [isRunning, startPolling, stopPolling]);

  useEffect(() => {
    isMountedRef.current = true;
    getStatus();
    return () => {
      isMountedRef.current = false;
      stopPolling();
    };
  }, [getStatus, stopPolling]);

  const trigger = useCallback(async (options = {}) => {
    setLoading(true);
    setError(null);
    try {
      const response = await pipelineApi.trigger(options);
      updateFromStatus(response.data);
      startPolling();
      return response.data;
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to trigger pipeline');
      throw err;
    } finally {
      setLoading(false);
    }
  }, [updateFromStatus, startPolling]);

  const pause = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await pipelineApi.pause();
      updateFromStatus(response.data);
      stopPolling();
      return response.data;
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to pause pipeline');
      throw err;
    } finally {
      setLoading(false);
    }
  }, [updateFromStatus, stopPolling]);

  const resume = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await pipelineApi.resume();
      updateFromStatus(response.data);
      startPolling();
      return response.data;
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to resume pipeline');
      throw err;
    } finally {
      setLoading(false);
    }
  }, [updateFromStatus, startPolling]);

  const getLogs = useCallback(async (options = {}) => {
    setError(null);
    try {
      const response = await pipelineApi.getLogs(options);
      const logData = response.data.logs || response.data;
      if (isMountedRef.current) {
        setLogs(logData);
      }
      return logData;
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to get pipeline logs');
      throw err;
    }
  }, []);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    isRunning,
    currentStage,
    progress,
    status,
    logs,
    loading,
    error,
    trigger,
    pause,
    resume,
    getStatus,
    getLogs,
    clearError,
  };
}

export default usePipeline;
