import api from './axiosInstance'

export const topicsApi = {
  getAll: () => api.get('/topics'),
  getById: (id) => api.get(`/topics/${id}`),
  create: (data) => api.post('/topics', data),
  update: (id, data) => api.put(`/topics/${id}`, data),
  delete: (id) => api.delete(`/topics/${id}`),
  generateIdeas: (data) => api.post('/topics/generate-ideas', data),
  triggerPipeline: (id) => api.post(`/topics/${id}/trigger`),
  reorder: (data) => api.post('/topics/reorder', data),
}
