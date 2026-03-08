import api from './axiosInstance'

export const pipelineApi = {
  getStatus: () => api.get('/pipeline/status'),
  trigger: (data) => api.post('/pipeline/trigger', data),
  pause: () => api.post('/pipeline/pause'),
  resume: () => api.post('/pipeline/resume'),
  getLogs: (params) => api.get('/pipeline/logs', { params }),
  getSchedule: () => api.get('/pipeline/schedule'),
  updateSchedule: (data) => api.put('/pipeline/schedule', data),
}
