// Employee Status Enum
export enum EmployeeStatus {
  Active = 0,
  Inactive = 1,
}

// Pagination
export interface PaginationParams {
  pageNumber: number;
  pageSize: number;
}

export interface PaginatedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// Department DTOs
export interface DepartmentResponse {
  id: string;
  name: string;
  description: string | null;
  employeeCount: number;
}

export interface CreateDepartmentRequest {
  name: string;
  description?: string | null;
}

export interface UpdateDepartmentRequest {
  name: string;
  description?: string | null;
}

// Employee DTOs
export interface EmployeeResponse {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  status: EmployeeStatus;
  hireDate: string;
  notes: string | null;
  departmentId: string;
  departmentName: string;
}

export interface EmployeeDetailResponse extends EmployeeResponse {
  projects: ProjectResponse[];
}

export interface CreateEmployeeRequest {
  firstName: string;
  lastName: string;
  email: string;
  status: EmployeeStatus;
  hireDate: string;
  notes?: string | null;
  departmentId: string;
}

export interface UpdateEmployeeRequest {
  firstName: string;
  lastName: string;
  email: string;
  status: EmployeeStatus;
  hireDate: string;
  notes?: string | null;
  departmentId: string;
}

// Project DTOs
export interface ProjectResponse {
  id: string;
  name: string;
  description: string | null;
  startDate: string;
  endDate: string | null;
  employeeCount: number;
}

export interface CreateProjectRequest {
  name: string;
  description?: string | null;
  startDate: string;
  endDate?: string | null;
}

export interface UpdateProjectRequest {
  name: string;
  description?: string | null;
  startDate: string;
  endDate?: string | null;
}

// Employee-Project Assignment (for display purposes)
export interface EmployeeProjectAssignment {
  employeeId: string;
  employeeName: string;
  employeeEmail: string;
  projectId: string;
  projectName: string;
  projectStartDate: string;
  projectEndDate: string | null;
}
