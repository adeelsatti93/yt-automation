import api from './axiosInstance'

export const settingsApi = {
  getAll: () => api.get('/settings'),
  update: (key, data) => api.put(`/settings/${key}`, data),
  batchUpdate: (data) => api.put('/settings/batch', data),
  testConnection: (service) => api.post(`/settings/test/${service}`),
  getStatus: () => api.get('/settings/status'),
}
