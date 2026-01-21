import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { Button } from '../common';
import type { DepartmentResponse, CreateDepartmentRequest } from '../../types';

interface DepartmentFormProps {
  department?: DepartmentResponse | null;
  onSubmit: (data: CreateDepartmentRequest) => Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

export const DepartmentForm: React.FC<DepartmentFormProps> = ({
  department,
  onSubmit,
  onCancel,
  isLoading = false,
}) => {
  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm<CreateDepartmentRequest>({
    defaultValues: {
      name: department?.name || '',
      description: department?.description || '',
    },
  });

  useEffect(() => {
    if (department) {
      reset({
        name: department.name,
        description: department.description || '',
      });
    }
  }, [department, reset]);

  const onFormSubmit = async (data: CreateDepartmentRequest) => {
    try {
      await onSubmit(data);
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
          Name <span className="text-red-500">*</span>
        </label>
        <input
          id="name"
          type="text"
          {...register('name', {
            required: 'Name is required',
            minLength: { value: 2, message: 'Name must be at least 2 characters' },
            maxLength: { value: 100, message: 'Name must be less than 100 characters' },
          })}
          className={`
            block w-full px-3 py-2 rounded-lg
            bg-white dark:bg-gray-700
            border ${errors.name ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'}
            text-gray-900 dark:text-gray-100
            focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent
            transition-colors
          `}
          placeholder="Enter department name"
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
            transition-colors resize-none
          `}
          placeholder="Enter department description (optional)"
        />
        {errors.description && (
          <p className="mt-1 text-sm text-red-500">{errors.description.message}</p>
        )}
      </div>

      <div className="flex justify-end gap-3 pt-4 border-t border-gray-200 dark:border-gray-700">
        <Button type="button" variant="secondary" onClick={onCancel} disabled={isLoading}>
          Cancel
        </Button>
        <Button type="submit" isLoading={isLoading}>
          {department ? 'Update' : 'Create'} Department
        </Button>
      </div>
    </form>
  );
};
