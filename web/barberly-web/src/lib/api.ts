import axios from 'axios';
import { apiConfig } from './config';

export const api = axios.create({
  baseURL: apiConfig.baseURL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor for auth token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('auth-token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Handle unauthorized - clear token and redirect to auth
      localStorage.removeItem('auth-token');
      window.location.href = '/auth';
    }

    if (error.response?.status === 403) {
      console.warn('Forbidden - insufficient permissions');
    }

    return Promise.reject(error);
  }
);

export default api;
