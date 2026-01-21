import React, { useState, useEffect, useCallback } from 'react';
import { Plus, Pencil, Trash2, Users } from 'lucide-react';
import {
  Button,
  Modal,
  SearchBar,
  Pagination,
  ConfirmDialog,
  LoadingSpinner,
  EmptyState,
} from '../common';
import { DepartmentForm } from './DepartmentForm';
import { departmentsService } from '../../services/departmentsService';
import type {
  DepartmentResponse,
  CreateDepartmentRequest,
  PaginatedResult,
} from '../../types';

export const DepartmentsTab: React.FC = () => {
  const [departments, setDepartments] = useState<PaginatedResult<DepartmentResponse> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 10;

  // Modal states
  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [selectedDepartment, setSelectedDepartment] = useState<DepartmentResponse | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const fetchDepartments = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      let result: PaginatedResult<DepartmentResponse>;
      if (searchQuery.trim()) {
        result = await departmentsService.search(searchQuery, currentPage, pageSize);
      } else {
        result = await departmentsService.getAll(currentPage, pageSize);
      }
      setDepartments(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch departments');
    } finally {
      setIsLoading(false);
    }
  }, [searchQuery, currentPage, pageSize]);

  useEffect(() => {
    fetchDepartments();
  }, [fetchDepartments]);

  const handleSearch = (query: string) => {
    setSearchQuery(query);
    setCurrentPage(1);
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handleCreate = () => {
    setSelectedDepartment(null);
    setError(null); // Clear any previous errors
    setIsFormModalOpen(true);
  };

  const handleEdit = (department: DepartmentResponse) => {
    setSelectedDepartment(department);
    setError(null); // Clear any previous errors
    setIsFormModalOpen(true);
  };

  const handleDeleteClick = (department: DepartmentResponse) => {
    setSelectedDepartment(department);
    setIsDeleteDialogOpen(true);
  };

  const handleFormSubmit = async (data: CreateDepartmentRequest) => {
    setIsSubmitting(true);
    try {
      if (selectedDepartment) {
        await departmentsService.update(selectedDepartment.id, data);
      } else {
        await departmentsService.create(data);
      }
      setIsFormModalOpen(false);
      setSelectedDepartment(null);
      setError(null); // Clear any previous errors
      fetchDepartments();
    } catch (err) {
      // Don't set error here - form handles all errors internally
      // Re-throw so the form can catch and display the error
      throw err;
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeleteConfirm = async () => {
    if (!selectedDepartment) return;
    setIsSubmitting(true);
    try {
      await departmentsService.delete(selectedDepartment.id);
      setIsDeleteDialogOpen(false);
      setSelectedDepartment(null);
      fetchDepartments();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete department');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white">Departments</h2>
          <p className="text-sm text-gray-500 dark:text-gray-400">
            Manage organization departments
          </p>
        </div>
        <Button onClick={handleCreate}>
          <Plus className="w-4 h-4 mr-2" />
          Add Department
        </Button>
      </div>

      {/* Search */}
      <div className="max-w-md">
        <SearchBar
          value={searchQuery}
          onChange={handleSearch}
          placeholder="Search departments..."
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
          <LoadingSpinner text="Loading departments..." />
        ) : !departments || departments.items.length === 0 ? (
          <EmptyState
            title="No departments found"
            description={
              searchQuery
                ? 'Try adjusting your search query'
                : 'Get started by creating your first department'
            }
            action={
              !searchQuery && (
                <Button onClick={handleCreate}>
                  <Plus className="w-4 h-4 mr-2" />
                  Add Department
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
                      Name
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Description
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Employees
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                      Actions
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                  {departments.items.map((department) => (
                    <tr
                      key={department.id}
                      className="hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors"
                    >
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm font-medium text-gray-900 dark:text-white">
                          {department.name}
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <div className="text-sm text-gray-500 dark:text-gray-400 max-w-xs truncate">
                          {department.description || '-'}
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="flex items-center gap-1 text-sm text-gray-500 dark:text-gray-400">
                          <Users className="w-4 h-4" />
                          {department.employeeCount}
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right">
                        <div className="flex items-center justify-end gap-2">
                          <button
                            onClick={() => handleEdit(department)}
                            className="p-1.5 text-gray-500 hover:text-primary-600 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
                            title="Edit"
                          >
                            <Pencil className="w-4 h-4" />
                          </button>
                          <button
                            onClick={() => handleDeleteClick(department)}
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
              currentPage={departments.pageNumber}
              totalPages={departments.totalPages}
              totalCount={departments.totalCount}
              pageSize={departments.pageSize}
              onPageChange={handlePageChange}
              hasPreviousPage={departments.hasPreviousPage}
              hasNextPage={departments.hasNextPage}
            />
          </>
        )}
      </div>

      {/* Create/Edit Modal */}
      <Modal
        isOpen={isFormModalOpen}
        onClose={() => {
          setIsFormModalOpen(false);
          setSelectedDepartment(null);
          setError(null); // Clear errors when modal closes
        }}
        title={selectedDepartment ? 'Edit Department' : 'Create Department'}
      >
        <DepartmentForm
          department={selectedDepartment}
          onSubmit={handleFormSubmit}
          onCancel={() => {
            setIsFormModalOpen(false);
            setSelectedDepartment(null);
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
          setSelectedDepartment(null);
        }}
        onConfirm={handleDeleteConfirm}
        title="Delete Department"
        message={`Are you sure you want to delete "${selectedDepartment?.name}"? This action cannot be undone.`}
        confirmText="Delete"
        isLoading={isSubmitting}
      />
    </div>
  );
};
