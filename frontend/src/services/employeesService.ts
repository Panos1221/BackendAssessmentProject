import api from './api';
import type {
  EmployeeResponse,
  EmployeeDetailResponse,
  CreateEmployeeRequest,
  UpdateEmployeeRequest,
  PaginatedResult,
} from '../types';

const BASE_URL = '/api/employees';

export const employeesService = {
  // Get all employees with pagination
  getAll: async (pageNumber = 1, pageSize = 10): Promise<PaginatedResult<EmployeeResponse>> => {
    const response = await api.get<PaginatedResult<EmployeeResponse>>(BASE_URL, {
      params: { pageNumber, pageSize },
    });
    return response.data;
  },

  // Get employee by ID (detailed response with projects)
  getById: async (id: string): Promise<EmployeeDetailResponse> => {
    const response = await api.get<EmployeeDetailResponse>(`${BASE_URL}/${id}`);
    return response.data;
  },

  // Search employees
  search: async (
    query: string,
    pageNumber = 1,
    pageSize = 10
  ): Promise<PaginatedResult<EmployeeResponse>> => {
    const response = await api.get<PaginatedResult<EmployeeResponse>>(`${BASE_URL}/search`, {
      params: { q: query, pageNumber, pageSize },
    });
    return response.data;
  },

  // Create a new employee
  create: async (data: CreateEmployeeRequest): Promise<EmployeeResponse> => {
    const response = await api.post<EmployeeResponse>(BASE_URL, data);
    return response.data;
  },

  // Update an existing employee
  update: async (id: string, data: UpdateEmployeeRequest): Promise<EmployeeResponse> => {
    const response = await api.put<EmployeeResponse>(`${BASE_URL}/${id}`, data);
    return response.data;
  },

  // Delete an employee (soft delete)
  delete: async (id: string): Promise<void> => {
    await api.delete(`${BASE_URL}/${id}`);
  },

  // Assign employee to a project
  assignToProject: async (employeeId: string, projectId: string): Promise<void> => {
    await api.post(`${BASE_URL}/${employeeId}/projects/${projectId}`);
  },

  // Remove employee from a project
  removeFromProject: async (employeeId: string, projectId: string): Promise<void> => {
    await api.delete(`${BASE_URL}/${employeeId}/projects/${projectId}`);
  },
};
