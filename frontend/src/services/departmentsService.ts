import api from './api';
import type {
  DepartmentResponse,
  CreateDepartmentRequest,
  UpdateDepartmentRequest,
  PaginatedResult,
  EmployeeResponse,
} from '../types';

const BASE_URL = '/api/departments';

export const departmentsService = {
  // Get all departments with pagination
  getAll: async (pageNumber = 1, pageSize = 10): Promise<PaginatedResult<DepartmentResponse>> => {
    const response = await api.get<PaginatedResult<DepartmentResponse>>(BASE_URL, {
      params: { pageNumber, pageSize },
    });
    return response.data;
  },

  // Get department by ID
  getById: async (id: string): Promise<DepartmentResponse> => {
    const response = await api.get<DepartmentResponse>(`${BASE_URL}/${id}`);
    return response.data;
  },

  // Search departments
  search: async (
    query: string,
    pageNumber = 1,
    pageSize = 10
  ): Promise<PaginatedResult<DepartmentResponse>> => {
    const response = await api.get<PaginatedResult<DepartmentResponse>>(`${BASE_URL}/search`, {
      params: { q: query, pageNumber, pageSize },
    });
    return response.data;
  },

  // Get employees in a department
  getEmployees: async (
    id: string,
    pageNumber = 1,
    pageSize = 10
  ): Promise<PaginatedResult<EmployeeResponse>> => {
    const response = await api.get<PaginatedResult<EmployeeResponse>>(
      `${BASE_URL}/${id}/employees`,
      { params: { pageNumber, pageSize } }
    );
    return response.data;
  },

  // Create a new department
  create: async (data: CreateDepartmentRequest): Promise<DepartmentResponse> => {
    const response = await api.post<DepartmentResponse>(BASE_URL, data);
    return response.data;
  },

  // Update an existing department
  update: async (id: string, data: UpdateDepartmentRequest): Promise<DepartmentResponse> => {
    const response = await api.put<DepartmentResponse>(`${BASE_URL}/${id}`, data);
    return response.data;
  },

  // Delete a department (soft delete)
  delete: async (id: string): Promise<void> => {
    await api.delete(`${BASE_URL}/${id}`);
  },
};
