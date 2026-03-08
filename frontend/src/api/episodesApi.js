import api from './axiosInstance'

export const episodesApi = {
  getAll: (params) => api.get('/episodes', { params }),
  getById: (id) => api.get(`/episodes/${id}`),
  updateMetadata: (id, data) => api.put(`/episodes/${id}/metadata`, data),
  approve: (id, data) => api.post(`/episodes/${id}/approve`, data),
  reject: (id, data) => api.post(`/episodes/${id}/reject`, data),
  regenerate: (id, stage) => api.post(`/episodes/${id}/regenerate/${stage}`),
  resume: (id, stage) => api.post(`/episodes/${id}/resume/${stage}`),
  delete: (id) => api.delete(`/episodes/${id}`),
  getPipelineJobs: (id) => api.get(`/episodes/${id}/pipeline-jobs`),
}
