import { useState, useEffect, useCallback } from 'react';
import { charactersApi } from '../api/charactersApi';

export function useCharacters(autoFetch = true) {
  const [characters, setCharacters] = useState([]);
  const [voices, setVoices] = useState([]);
  const [loading, setLoading] = useState(false);
  const [voicesLoading, setVoicesLoading] = useState(false);
  const [error, setError] = useState(null);

  const fetchCharacters = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await charactersApi.getAll();
      setCharacters(response.data.characters || response.data);
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to fetch characters');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (autoFetch) {
      fetchCharacters();
    }
  }, [autoFetch, fetchCharacters]);

  const createCharacter = useCallback(async (characterData) => {
    setError(null);
    try {
      const response = await charactersApi.create(characterData);
      const newCharacter = response.data;
      setCharacters((prev) => [...prev, newCharacter]);
      return newCharacter;
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to create character');
      throw err;
    }
  }, []);

  const updateCharacter = useCallback(async (characterId, characterData) => {
    setError(null);
    try {
      const response = await charactersApi.update(characterId, characterData);
      const updated = response.data;
      setCharacters((prev) =>
        prev.map((ch) => (ch.id === characterId ? { ...ch, ...updated } : ch))
      );
      return updated;
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to update character');
      throw err;
    }
  }, []);

  const deleteCharacter = useCallback(async (characterId) => {
    setError(null);
    try {
      await charactersApi.delete(characterId);
      setCharacters((prev) => prev.filter((ch) => ch.id !== characterId));
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to delete character');
      throw err;
    }
  }, []);

  const fetchVoices = useCallback(async () => {
    setVoicesLoading(true);
    setError(null);
    try {
      const response = await charactersApi.getVoices();
      const voiceData = response.data.voices || response.data;
      setVoices(voiceData);
      return voiceData;
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to fetch voices');
      throw err;
    } finally {
      setVoicesLoading(false);
    }
  }, []);

  const testVoice = useCallback(async (voiceId, text) => {
    setError(null);
    try {
      const response = await charactersApi.testVoice(voiceId, { text });
      return response.data;
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to test voice');
      throw err;
    }
  }, []);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    characters,
    voices,
    loading,
    voicesLoading,
    error,
    fetchCharacters,
    createCharacter,
    updateCharacter,
    deleteCharacter,
    fetchVoices,
    testVoice,
    clearError,
  };
}

export default useCharacters;
