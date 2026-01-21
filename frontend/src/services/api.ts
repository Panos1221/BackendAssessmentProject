import axios, { AxiosError, AxiosInstance } from 'axios';

// Create axios instance with base configuration
const api: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor
api.interceptors.request.use(
  (config) => {
    // You can add auth tokens here if needed
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response) {
      // Server responded with error status
      const status = error.response.status;
      const data = error.response.data as { 
        message?: string; 
        errors?: Record<string, string[]> | Array<{ property?: string; message?: string }>;
      };
      
      let errorMessage = 'An error occurred';
      
      // Check if errors is an array (from ValidationException middleware)
      if (Array.isArray(data?.errors) && data.errors.length > 0) {
        // Extract messages from validation error array
        const messages = data.errors.map((e: any) => 
          typeof e === 'string' ? e : (e.message || e.Message || '')
        ).filter(Boolean);
        errorMessage = messages.join(', ');
      } else if (data?.errors && typeof data.errors === 'object') {
        // FluentValidation errors as Record<string, string[]>
        const validationErrors = Object.values(data.errors).flat();
        errorMessage = validationErrors.join(', ');
      } else if (data?.message) {
        errorMessage = data.message;
      } else {
        switch (status) {
          case 400:
            errorMessage = 'Invalid request';
            break;
          case 404:
            errorMessage = 'Resource not found';
            break;
          case 500:
            errorMessage = 'Server error';
            break;
        }
      }
      
      return Promise.reject(new Error(errorMessage));
    } else if (error.request) {
      // Request made but no response
      return Promise.reject(new Error('No response from server. Please check your connection.'));
    } else {
      // Something else happened
      return Promise.reject(new Error(error.message || 'An unexpected error occurred'));
    }
  }
);

export default api;
