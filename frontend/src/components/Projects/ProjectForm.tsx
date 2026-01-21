import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { Button } from '../common';
import type { ProjectResponse, CreateProjectRequest } from '../../types';

interface ProjectFormProps {
  project?: ProjectResponse | null;
  onSubmit: (data: CreateProjectRequest) => Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

export const ProjectForm: React.FC<ProjectFormProps> = ({
  project,
  onSubmit,
  onCancel,
  isLoading = false,
}) => {
  const {
    register,
    handleSubmit,
    reset,
    watch,
    setError,
    formState: { errors },
  } = useForm<CreateProjectRequest>({
    defaultValues: {
      name: project?.name || '',
      description: project?.description || '',
      startDate: project?.startDate?.split('T')[0] || new Date().toISOString().split('T')[0],
      endDate: project?.endDate?.split('T')[0] || '',
    },
  });

  const startDate = watch('startDate');

  useEffect(() => {
    if (project) {
      reset({
        name: project.name,
        description: project.description || '',
        startDate: project.startDate?.split('T')[0],
        endDate: project.endDate?.split('T')[0] || '',
      });
    }
  }, [project, reset]);

  const onFormSubmit = async (data: CreateProjectRequest) => {
    try {
      await onSubmit({
        ...data,
        endDate: data.endDate || null,
      });
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'An error occurred';
      
      // Show all errors on the name field (most likely cause is duplicate name)
      // This keeps the error inside the form instead of showing it on the main page
      setError('name', {
        type: 'server',
        message: errorMessage,
      });
      // Don't re-throw - we've handled it by showing in form
    }
  };

  return (
    <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-4">
      <div>
        <label
          htmlFor="name"
          className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
        >
          Project Name <span className="text-red-500">*</span>
        </label>
        <input
          id="name"
          type="text"
          {...register('name', {
            required: 'Project name is required',
            minLength: { value: 2, message: 'Name must be at least 2 characters' },
            maxLength: { value: 100, message: 'Name must be less than 100 characters' },
          })}
          className={`
            block w-full px-3 py-2 rounded-lg
            bg-white dark:bg-gray-700
            border ${errors.name ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'}
            text-gray-900 dark:text-gray-100
            focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent
          `}
          placeholder="Enter project name"
        />
        {errors.name && (
          <p className="mt-1 text-sm text-red-500">{errors.name.message}</p>
        )}
      </div>

      <div>
        <label
          htmlFor="description"
          className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
        >
          Description
        </label>
        <textarea
          id="description"
          {...register('description', {
            maxLength: { value: 500, message: 'Description must be less than 500 characters' },
          })}
          rows={3}
          className={`
            block w-full px-3 py-2 rounded-lg
            bg-white dark:bg-gray-700
            border ${errors.description ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'}
            text-gray-900 dark:text-gray-100
            focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent
            resize-none
          `}
          placeholder="Enter project description (optional)"
        />
        {errors.description && (
          <p className="mt-1 text-sm text-red-500">{errors.description.message}</p>
        )}
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div>
          <label
            htmlFor="startDate"
            className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
          >
            Start Date <span className="text-red-500">*</span>
          </label>
          <input
            id="startDate"
            type="date"
            {...register('startDate', { required: 'Start date is required' })}
            className={`
              block w-full px-3 py-2 rounded-lg
              bg-white dark:bg-gray-700
              border ${errors.startDate ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'}
              text-gray-900 dark:text-gray-100
              focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent
            `}
          />
          {errors.startDate && (
            <p className="mt-1 text-sm text-red-500">{errors.startDate.message}</p>
          )}
        </div>

        <div>
          <label
            htmlFor="endDate"
            className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
          >
            End Date
          </label>
          <input
            id="endDate"
            type="date"
            {...register('endDate', {
              validate: (value) => {
                if (value && startDate && new Date(value) < new Date(startDate)) {
                  return 'End date must be after start date';
                }
                return true;
              },
            })}
            className={`
              block w-full px-3 py-2 rounded-lg
              bg-white dark:bg-gray-700
              border ${errors.endDate ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'}
              text-gray-900 dark:text-gray-100
              focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent
            `}
          />
          {errors.endDate && (
            <p className="mt-1 text-sm text-red-500">{errors.endDate.message}</p>
          )}
        </div>
      </div>

      <div className="flex justify-end gap-3 pt-4 border-t border-gray-200 dark:border-gray-700">
        <Button type="button" variant="secondary" onClick={onCancel} disabled={isLoading}>
          Cancel
        </Button>
        <Button type="submit" isLoading={isLoading}>
          {project ? 'Update' : 'Create'} Project
        </Button>
      </div>
    </form>
  );
};
