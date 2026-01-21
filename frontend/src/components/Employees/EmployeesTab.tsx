import React, { useState, useEffect, useCallback } from 'react';
import { Plus, Pencil, Trash2 } from 'lucide-react';
import {
  Button,
  Modal,
  SearchBar,
  Pagination,
  ConfirmDialog,
  LoadingSpinner,
  EmptyState,
} from '../common';
import { EmployeeForm } from './EmployeeForm';
import { employeesService } from '../../services/employeesService';
import type {
  EmployeeResponse,
  CreateEmployeeRequest,
  PaginatedResult,
} from '../../types';
import { EmployeeStatus } from '../../types';

export const EmployeesTab: React.FC = () => {
  const [employees, setEmployees] = useState<PaginatedResult<EmployeeResponse> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 10;

  // Modal states
  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [selectedEmployee, setSelectedEmployee] = useState<EmployeeResponse | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const fetchEmployees = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      let result: PaginatedResult<EmployeeResponse>;
      if (searchQuery.trim()) {
        result = await employeesService.search(searchQuery, currentPage, pageSize);
      } else {
        result = await employeesService.getAll(currentPage, pageSize);
      }
      setEmployees(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch employees');
    } finally {
      setIsLoading(false);
    }
  }, [searchQuery, currentPage, pageSize]);

  useEffect(() => {
    fetchEmployees();
  }, [fetchEmployees]);

  const handleSearch = (query: string) => {
    setSearchQuery(query);
    setCurrentPage(1);
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handleCreate = () => {
    setSelectedEmployee(null);
    setError(null); // Clear any previous errors
    setIsFormModalOpen(true);
  };

  const handleEdit = (employee: EmployeeResponse) => {
    setSelectedEmployee(employee);
    setError(null); // Clear any previous errors
    setIsFormModalOpen(true);
  };

  const handleDeleteClick = (employee: EmployeeResponse) => {
    setSelectedEmployee(employee);
    setIsDeleteDialogOpen(true);
  };

  const handleFormSubmit = async (data: CreateEmployeeRequest) => {
    setIsSubmitting(true);
    try {
      if (selectedEmployee) {
        await employeesService.update(selectedEmployee.id, data);
      } else {
        await employeesService.create(data);
      }
      setIsFormModalOpen(false);
      setSelectedEmployee(null);
      setError(null); // Clear any previous errors
      fetchEmployees();
    } catch (err) {
      // Don't set error here - form handles all errors internally
      // Re-throw so the form can catch and display the error
      throw err;
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeleteConfirm = async () => {
    if (!selectedEmployee) return;
    setIsSubmitting(true);
    try {
      await employeesService.delete(selectedEmployee.id);
      setIsDeleteDialogOpen(false);
      setSelectedEmployee(null);
      fetchEmployees();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete employee');
    } finally {
      setIsSubmitting(false);
    }
  };

  const getStatusBadge = (status: EmployeeStatus) => {
    const isActive = status === EmployeeStatus.Active;
    return (
      <span
        className={`
          inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium
          ${
            isActive
              ? 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400'
              : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-400'
          }
        `}
      >
        {isActive ? 'Active' : 'Inactive'}
      </span>
    );
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white">Employees</h2>
          <p className="text-sm text-gray-500 dark:text-gray-400">
            Manage organization employees
          </p>
        </div>
        <Button onClick={handleCreate}>
          <Plus className="w-4 h-4 mr-2" />
          Add Employee
        </Button>
      </div>

      {/* Search */}
      <div className="max-w-md">
        <SearchBar
          value={searchQuery}
          onChange={handleSearch}
          placeholder="Search employees..."
        />
      </div>

      {/* Error message */}
      {error && (
        <div className="p-4 bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-800 rounded-lg">
          <p className="text-sm text-red-600 dark:text-red-400">{error}</p>
        </div>
      )}

      {/* Content */}
      <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
        {isLoading ? (
          <LoadingSpinner text="Loading employees..." />
        ) : !employees || employees.items.length === 0 ? (
          <EmptyState
            title="No employees found"
            description={
              searchQuery
                ? 'Try adjusting your search query'
                : 'Get started by creating your first employee'
            }
            action={
              !searchQuery && (
                <Button onClick={handleCreate}>
                  <Plus className="w-4 h-4 mr-2" />
                  Add Employee
                </Button>
              )
            }
          />
        ) : (
          <>
            {/* Table */}
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
                <thead className="bg-gray-50 dark:bg-gray-900/50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Employee
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Department
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Status
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Hire Date
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                  {employees.items.map((employee) => (
                    <tr
                      key={employee.id}
                      className="hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors"
                    >
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div>
                          <div className="text-sm font-medium text-gray-900 dark:text-white">
                            {employee.firstName} {employee.lastName}
                          </div>
                          <div className="text-sm text-gray-500 dark:text-gray-400">
                            {employee.email}
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm text-gray-900 dark:text-white">
                          {employee.departmentName}
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        {getStatusBadge(employee.status)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm text-gray-500 dark:text-gray-400">
                          {formatDate(employee.hireDate)}
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right">
                        <div className="flex items-center justify-end gap-2">
                          <button
                            onClick={() => handleEdit(employee)}
                            className="p-1.5 text-gray-500 hover:text-primary-600 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
                            title="Edit"
                          >
                            <Pencil className="w-4 h-4" />
                          </button>
                          <button
                            onClick={() => handleDeleteClick(employee)}
                            className="p-1.5 text-gray-500 hover:text-red-600 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
                            title="Delete"
                          >
                            <Trash2 className="w-4 h-4" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            <Pagination
              currentPage={employees.pageNumber}
              totalPages={employees.totalPages}
              totalCount={employees.totalCount}
              pageSize={employees.pageSize}
              onPageChange={handlePageChange}
              hasPreviousPage={employees.hasPreviousPage}
              hasNextPage={employees.hasNextPage}
            />
          </>
        )}
      </div>

      {/* Create/Edit Modal */}
      <Modal
        isOpen={isFormModalOpen}
        onClose={() => {
          setIsFormModalOpen(false);
          setSelectedEmployee(null);
          setError(null); // Clear errors when modal closes
        }}
        title={selectedEmployee ? 'Edit Employee' : 'Create Employee'}
        size="lg"
      >
        <EmployeeForm
          employee={selectedEmployee}
          onSubmit={handleFormSubmit}
          onCancel={() => {
            setIsFormModalOpen(false);
            setSelectedEmployee(null);
            setError(null); // Clear errors when canceling
          }}
          isLoading={isSubmitting}
        />
      </Modal>

      {/* Delete Confirmation */}
      <ConfirmDialog
        isOpen={isDeleteDialogOpen}
        onClose={() => {
          setIsDeleteDialogOpen(false);
          setSelectedEmployee(null);
        }}
        onConfirm={handleDeleteConfirm}
        title="Delete Employee"
        message={`Are you sure you want to delete "${selectedEmployee?.firstName} ${selectedEmployee?.lastName}"? This action cannot be undone.`}
        confirmText="Delete"
        isLoading={isSubmitting}
      />
    </div>
  );
};
