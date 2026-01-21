import api from './api';
import type {
  ProjectResponse,
  CreateProjectRequest,
  UpdateProjectRequest,
  PaginatedResult,
  EmployeeResponse,
} from '../types';

const BASE_URL = '/api/projects';

export const projectsService = {
  // Get all projects with pagination
  getAll: async (pageNumber = 1, pageSize = 10): Promise<PaginatedResult<ProjectResponse>> => {
    const response = await api.get<PaginatedResult<ProjectResponse>>(BASE_URL, {
      params: { pageNumber, pageSize },
    });
    return response.data;
  },

  // Get project by ID
  getById: async (id: string): Promise<ProjectResponse> => {
    const response = await api.get<ProjectResponse>(`${BASE_URL}/${id}`);
    return response.data;
  },

  // Search projects
  search: async (
    query: string,
    pageNumber = 1,
    pageSize = 10
  ): Promise<PaginatedResult<ProjectResponse>> => {
    const response = await api.get<PaginatedResult<ProjectResponse>>(`${BASE_URL}/search`, {
      params: { q: query, pageNumber, pageSize },
    });
    return response.data;
  },

  // Get employees assigned to a project
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

  // Create a new project
  create: async (data: CreateProjectRequest): Promise<ProjectResponse> => {
    const response = await api.post<ProjectResponse>(BASE_URL, data);
    return response.data;
  },

  // Update an existing project
  update: async (id: string, data: UpdateProjectRequest): Promise<ProjectResponse> => {
    const response = await api.put<ProjectResponse>(`${BASE_URL}/${id}`, data);
    return response.data;
  },

  // Delete a project (soft delete)
  delete: async (id: string): Promise<void> => {
    await api.delete(`${BASE_URL}/${id}`);
  },
};
