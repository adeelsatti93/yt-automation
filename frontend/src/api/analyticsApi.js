import api from './axiosInstance'

export const analyticsApi = {
  getSummary: () => api.get('/analytics/summary'),
  getEpisodeAnalytics: (id) => api.get(`/analytics/episodes/${id}`),
  sync: () => api.post('/analytics/sync'),
}
