import { useState, useEffect, useCallback } from 'react';
import { episodesApi } from '../api/episodesApi';

const DEFAULT_PAGE_SIZE = 10;

export function useEpisodes(initialFilters = {}) {
  const [episodes, setEpisodes] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const [filters, setFilters] = useState({
    status: initialFilters.status || null,
    sortBy: initialFilters.sortBy || 'createdAt',
    sortOrder: initialFilters.sortOrder || 'desc',
    page: initialFilters.page || 1,
    pageSize: initialFilters.pageSize || DEFAULT_PAGE_SIZE,
  });

  const fetchEpisodes = useCallback(async (overrideFilters) => {
    setLoading(true);
    setError(null);
    try {
      const params = overrideFilters || filters;
      const query = {
        page: params.page,
        pageSize: params.pageSize,
        sortBy: params.sortBy,
        sortOrder: params.sortOrder,
      };
      if (params.status) {
        query.status = params.status;
      }
      const response = await episodesApi.getAll(query);
      setEpisodes(response.data.episodes || response.data.items || response.data);
      setTotalCount(response.data.total ?? response.data.totalCount ?? 0);
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to fetch episodes');
    } finally {
      setLoading(false);
    }
  }, [filters]);

  useEffect(() => {
    fetchEpisodes();
  }, [fetchEpisodes]);

  const setPage = useCallback((page) => {
    setFilters((prev) => ({ ...prev, page }));
  }, []);

  const setPageSize = useCallback((pageSize) => {
    setFilters((prev) => ({ ...prev, pageSize, page: 1 }));
  }, []);

  const setStatusFilter = useCallback((status) => {
    setFilters((prev) => ({ ...prev, status, page: 1 }));
  }, []);

  const setSorting = useCallback((sortBy, sortOrder) => {
    setFilters((prev) => ({ ...prev, sortBy, sortOrder, page: 1 }));
  }, []);

  const deleteEpisode = useCallback(async (episodeId) => {
    setError(null);
    try {
      await episodesApi.delete(episodeId);
      setEpisodes((prev) => prev.filter((ep) => ep.id !== episodeId));
      setTotalCount((prev) => Math.max(0, prev - 1));
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to delete episode');
      throw err;
    }
  }, []);

  const approveEpisode = useCallback(async (episodeId) => {
    setError(null);
    try {
      const response = await episodesApi.approve(episodeId);
      const updated = response.data;
      setEpisodes((prev) =>
        prev.map((ep) => (ep.id === episodeId ? { ...ep, ...updated, status: 'approved' } : ep))
      );
      return updated;
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to approve episode');
      throw err;
    }
  }, []);

  const rejectEpisode = useCallback(async (episodeId, reason) => {
    setError(null);
    try {
      const response = await episodesApi.reject(episodeId, { reason });
      const updated = response.data;
      setEpisodes((prev) =>
        prev.map((ep) => (ep.id === episodeId ? { ...ep, ...updated, status: 'rejected' } : ep))
      );
      return updated;
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to reject episode');
      throw err;
    }
  }, []);

  const regenerateStage = useCallback(async (episodeId, stage) => {
    setError(null);
    try {
      const response = await episodesApi.regenerateStage(episodeId, stage);
      const updated = response.data;
      setEpisodes((prev) =>
        prev.map((ep) => (ep.id === episodeId ? { ...ep, ...updated } : ep))
      );
      return updated;
    } catch (err) {
      setError(
        err.response?.data?.message || err.message || `Failed to regenerate stage: ${stage}`
      );
      throw err;
    }
  }, []);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  const totalPages = Math.ceil(totalCount / filters.pageSize);

  return {
    episodes,
    totalCount,
    totalPages,
    loading,
    error,
    filters,
    fetchEpisodes,
    deleteEpisode,
    approveEpisode,
    rejectEpisode,
    regenerateStage,
    setPage,
    setPageSize,
    setStatusFilter,
    setSorting,
    clearError,
  };
}

export default useEpisodes;
