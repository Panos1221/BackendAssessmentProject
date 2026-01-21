import React, { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { Button, LoadingSpinner } from '../common';
import { employeesService } from '../../services/employeesService';
import { projectsService } from '../../services/projectsService';
import type { EmployeeResponse, ProjectResponse } from '../../types';

interface AssignmentFormData {
  employeeId: string;
  projectId: string;
}

interface AssignmentFormProps {
  onSubmit: (employeeId: string, projectId: string) => Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

export const AssignmentForm: React.FC<AssignmentFormProps> = ({
  onSubmit,
  onCancel,
  isLoading = false,
}) => {
  const [employees, setEmployees] = useState<EmployeeResponse[]>([]);
  const [projects, setProjects] = useState<ProjectResponse[]>([]);
  const [isLoadingData, setIsLoadingData] = useState(true);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<AssignmentFormData>({
    defaultValues: {
      employeeId: '',
      projectId: '',
    },
  });

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [employeesResult, projectsResult] = await Promise.all([
          employeesService.getAll(1, 100),
          projectsService.getAll(1, 100),
        ]);
        setEmployees(employeesResult.items);
        setProjects(projectsResult.items);
      } catch (error) {
        console.error('Failed to fetch data:', error);
      } finally {
        setIsLoadingData(false);
      }
    };
    fetchData();
  }, []);

  const onFormSubmit = async (data: AssignmentFormData) => {
    await onSubmit(data.employeeId, data.projectId);
  };

  if (isLoadingData) {
    return <LoadingSpinner text="Loading form..." />;
  }

  return (
    <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-4">
      <div>
        <label
          htmlFor="employeeId"
          className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
        >
          Employee <span className="text-red-500">*</span>
        </label>
        <select
          id="employeeId"
          {...register('employeeId', { required: 'Employee is required' })}
          className={`
            block w-full px-3 py-2 rounded-lg
            bg-white dark:bg-gray-700
            border ${errors.employeeId ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'}
            text-gray-900 dark:text-gray-100
            focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent
          `}
        >
          <option value="">Select an employee</option>
          {employees.map((emp) => (
            <option key={emp.id} value={emp.id}>
              {emp.firstName} {emp.lastName} ({emp.email})
            </option>
          ))}
        </select>
        {errors.employeeId && (
          <p className="mt-1 text-sm text-red-500">{errors.employeeId.message}</p>
        )}
      </div>

      <div>
        <label
          htmlFor="projectId"
          className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
        >
          Project <span className="text-red-500">*</span>
        </label>
        <select
          id="projectId"
          {...register('projectId', { required: 'Project is required' })}
          className={`
            block w-full px-3 py-2 rounded-lg
            bg-white dark:bg-gray-700
            border ${errors.projectId ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'}
            text-gray-900 dark:text-gray-100
            focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent
          `}
        >
          <option value="">Select a project</option>
          {projects.map((proj) => (
            <option key={proj.id} value={proj.id}>
              {proj.name}
            </option>
          ))}
        </select>
        {errors.projectId && (
          <p className="mt-1 text-sm text-red-500">{errors.projectId.message}</p>
        )}
      </div>

      <div className="flex justify-end gap-3 pt-4 border-t border-gray-200 dark:border-gray-700">
        <Button type="button" variant="secondary" onClick={onCancel} disabled={isLoading}>
          Cancel
        </Button>
        <Button type="submit" isLoading={isLoading}>
          Assign Employee
        </Button>
      </div>
    </form>
  );
};
