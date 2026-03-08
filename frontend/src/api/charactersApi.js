import api from './axiosInstance'

export const charactersApi = {
  getAll: () => api.get('/characters'),
  getById: (id) => api.get(`/characters/${id}`),
  create: (data) => api.post('/characters', data),
  update: (id, data) => api.put(`/characters/${id}`, data),
  delete: (id) => api.delete(`/characters/${id}`),
  getVoices: () => api.get('/characters/voices'),
  testVoice: (id) => api.post(`/characters/${id}/test-voice`),
}
