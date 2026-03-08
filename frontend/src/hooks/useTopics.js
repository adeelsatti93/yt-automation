import { useState, useEffect, useCallback } from 'react';
import { topicsApi } from '../api/topicsApi';

export function useTopics(autoFetch = true) {
  const [topics, setTopics] = useState([]);
  const [generatedIdeas, setGeneratedIdeas] = useState([]);
  const [loading, setLoading] = useState(false);
  const [generating, setGenerating] = useState(false);
  const [error, setError] = useState(null);

  const fetchTopics = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await topicsApi.getAll();
      setTopics(response.data.topics || response.data);
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to fetch topics');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (autoFetch) {
      fetchTopics();
    }
  }, [autoFetch, fetchTopics]);

  const createTopic = useCallback(async (topicData) => {
    setError(null);
    try {
      const response = await topicsApi.create(topicData);
      const newTopic = response.data;
      setTopics((prev) => [...prev, newTopic]);
      return newTopic;
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to create topic');
      throw err;
    }
  }, []);

  const updateTopic = useCallback(async (topicId, topicData) => {
    setError(null);
    try {
      const response = await topicsApi.update(topicId, topicData);
      const updated = response.data;
      setTopics((prev) =>
        prev.map((t) => (t.id === topicId ? { ...t, ...updated } : t))
      );
      return updated;
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to update topic');
      throw err;
    }
  }, []);

  const deleteTopic = useCallback(async (topicId) => {
    setError(null);
    try {
      await topicsApi.delete(topicId);
      setTopics((prev) => prev.filter((t) => t.id !== topicId));
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to delete topic');
      throw err;
    }
  }, []);

  const generateIdeas = useCallback(async (params = {}) => {
    setGenerating(true);
    setError(null);
    try {
      const response = await topicsApi.generateIdeas(params);
      const ideas = response.data.ideas || response.data;
      setGeneratedIdeas(ideas);
      return ideas;
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to generate ideas');
      throw err;
    } finally {
      setGenerating(false);
    }
  }, []);

  const triggerPipeline = useCallback(async (topicId) => {
    setError(null);
    try {
      const response = await topicsApi.triggerPipeline(topicId);
      return response.data;
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to trigger pipeline for topic');
      throw err;
    }
  }, []);

  const reorderTopics = useCallback(async (orderedIds) => {
    setError(null);
    try {
      const response = await topicsApi.reorder(orderedIds);
      const reordered = response.data.topics || response.data;
      if (Array.isArray(reordered) && reordered.length > 0) {
        setTopics(reordered);
      } else {
        setTopics((prev) => {
          const byId = {};
          prev.forEach((t) => { byId[t.id] = t; });
          return orderedIds.map((id) => byId[id]).filter(Boolean);
        });
      }
      return reordered;
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to reorder topics');
      throw err;
    }
  }, []);

  const clearIdeas = useCallback(() => {
    setGeneratedIdeas([]);
  }, []);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    topics,
    generatedIdeas,
    loading,
    generating,
    error,
    fetchTopics,
    createTopic,
    updateTopic,
    deleteTopic,
    generateIdeas,
    triggerPipeline,
    reorderTopics,
    clearIdeas,
    clearError,
  };
}

export default useTopics;
