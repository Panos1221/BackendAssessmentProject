import React, { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { Button, LoadingSpinner } from '../common';
import { departmentsService } from '../../services/departmentsService';
import type { EmployeeResponse, CreateEmployeeRequest, DepartmentResponse } from '../../types';
import { EmployeeStatus } from '../../types';

interface EmployeeFormProps {
  employee?: EmployeeResponse | null;
  onSubmit: (data: CreateEmployeeRequest) => Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

export const EmployeeForm: React.FC<EmployeeFormProps> = ({
  employee,
  onSubmit,
  onCancel,
  isLoading = false,
}) => {
  const [departments, setDepartments] = useState<DepartmentResponse[]>([]);
  const [isLoadingDepartments, setIsLoadingDepartments] = useState(true);

  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm<CreateEmployeeRequest>({
    defaultValues: {
      firstName: employee?.firstName || '',
      lastName: employee?.lastName || '',
      email: employee?.email || '',
      status: employee?.status ?? EmployeeStatus.Active,
      hireDate: employee?.hireDate?.split('T')[0] || new Date().toISOString().split('T')[0],
      notes: employee?.notes || '',
      departmentId: employee?.departmentId || '',
    },
  });

  useEffect(() => {
    const fetchDepartments = async () => {
      try {
        const result = await departmentsService.getAll(1, 100);
        setDepartments(result.items);
      } catch (error) {
        console.error('Failed to fetch departments:', error);
      } finally {
        setIsLoadingDepartments(false);
      }
    };
    fetchDepartments();
  }, []);

  useEffect(() => {
    if (employee) {
      reset({
        firstName: employee.firstName,
        lastName: employee.lastName,
        email: employee.email,
        status: employee.status,
        hireDate: employee.hireDate?.split('T')[0],
        notes: employee.notes || '',
        departmentId: employee.departmentId,
      });
    }
  }, [employee, reset]);

  const onFormSubmit = async (data: CreateEmployeeRequest) => {
    try {
      await onSubmit({
        ...data,
        status: Number(data.status),
      });
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'An error occurred';
      
      // Show all errors on the email field (most likely cause is duplicate email)
      // This keeps the error inside the form instead of showing it on the main page
      setError('email', {
        type: 'server',
        message: errorMessage,
      });
      // Don't re-throw - we've handled it by showing in form
    }
  };

  if (isLoadingDepartments) {
    return <LoadingSpinner text="Loading form..." />;
  }

  return (
    <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-4">
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div>
          <label
            htmlFor="firstName"
            className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
          >
            First Name <span className="text-red-500">*</span>
          </label>
          <input
            id="firstName"
            type="text"
            {...register('firstName', {
              required: 'First name is required',
              minLength: { value: 2, message: 'First name must be at least 2 characters' },
            })}
            className={`
              block w-full px-3 py-2 rounded-lg
              bg-white dark:bg-gray-700
              border ${errors.firstName ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'}
              text-gray-900 dark:text-gray-100
              focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent
            `}
            placeholder="John"
          />
          {errors.firstName && (
            <p className="mt-1 text-sm text-red-500">{errors.firstName.message}</p>
          )}
        </div>

        <div>
          <label
            htmlFor="lastName"
            className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
          >
            Last Name <span className="text-red-500">*</span>
          </label>
          <input
            id="lastName"
            type="text"
            {...register('lastName', {
              required: 'Last name is required',
              minLength: { value: 2, message: 'Last name must be at least 2 characters' },
            })}
            className={`
              block w-full px-3 py-2 rounded-lg
              bg-white dark:bg-gray-700
              border ${errors.lastName ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'}
              text-gray-900 dark:text-gray-100
              focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent
            `}
            placeholder="Doe"
          />
          {errors.lastName && (
            <p className="mt-1 text-sm text-red-500">{errors.lastName.message}</p>
          )}
        </div>
      </div>

      <div>
        <label
          htmlFor="email"
          className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
        >
          Email <span className="text-red-500">*</span>
        </label>
        <input
          id="email"
          type="email"
          {...register('email', {
            required: 'Email is required',
            pattern: {
              value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
              message: 'Invalid email address',
            },
          })}
          className={`
            block w-full px-3 py-2 rounded-lg
            bg-white dark:bg-gray-700
            border ${errors.email ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'}
            text-gray-900 dark:text-gray-100
            focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent
          `}
          placeholder="john.doe@example.com"
        />
        {errors.email && (
          <p className="mt-1 text-sm text-red-500">{errors.email.message}</p>
        )}
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div>
          <label
            htmlFor="departmentId"
            className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
          >
            Department <span className="text-red-500">*</span>
          </label>
          <select
            id="departmentId"
            {...register('departmentId', { required: 'Department is required' })}
            className={`
              block w-full px-3 py-2 rounded-lg
              bg-white dark:bg-gray-700
              border ${errors.departmentId ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'}
              text-gray-900 dark:text-gray-100
              focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent
            `}
          >
            <option value="">Select department</option>
            {departments.map((dept) => (
              <option key={dept.id} value={dept.id}>
                {dept.name}
              </option>
            ))}
          </select>
          {errors.departmentId && (
            <p className="mt-1 text-sm text-red-500">{errors.departmentId.message}</p>
          )}
        </div>

        <div>
          <label
            htmlFor="status"
            className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
          >
            Status <span className="text-red-500">*</span>
          </label>
          <select
            id="status"
            {...register('status', { required: 'Status is required' })}
            className={`
              block w-full px-3 py-2 rounded-lg
              bg-white dark:bg-gray-700
              border ${errors.status ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'}
              text-gray-900 dark:text-gray-100
              focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent
            `}
          >
            <option value={EmployeeStatus.Active}>Active</option>
            <option value={EmployeeStatus.Inactive}>Inactive</option>
          </select>
          {errors.status && (
            <p className="mt-1 text-sm text-red-500">{errors.status.message}</p>
          )}
        </div>
      </div>

      <div>
        <label
          htmlFor="hireDate"
          className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
        >
          Hire Date <span className="text-red-500">*</span>
        </label>
        <input
          id="hireDate"
          type="date"
          {...register('hireDate', { required: 'Hire date is required' })}
          className={`
            block w-full px-3 py-2 rounded-lg
            bg-white dark:bg-gray-700
            border ${errors.hireDate ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'}
            text-gray-900 dark:text-gray-100
            focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent
          `}
        />
        {errors.hireDate && (
          <p className="mt-1 text-sm text-red-500">{errors.hireDate.message}</p>
        )}
      </div>

      <div>
        <label
          htmlFor="notes"
          className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
        >
          Notes
        </label>
        <textarea
          id="notes"
          {...register('notes')}
          rows={3}
          className="
            block w-full px-3 py-2 rounded-lg
            bg-white dark:bg-gray-700
            border border-gray-300 dark:border-gray-600
            text-gray-900 dark:text-gray-100
            focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent
            resize-none
          "
          placeholder="Additional notes (optional)"
        />
      </div>

      <div className="flex justify-end gap-3 pt-4 border-t border-gray-200 dark:border-gray-700">
        <Button type="button" variant="secondary" onClick={onCancel} disabled={isLoading}>
          Cancel
        </Button>
        <Button type="submit" isLoading={isLoading}>
          {employee ? 'Update' : 'Create'} Employee
        </Button>
      </div>
    </form>
  );
};
