import React, { useState, useEffect, useCallback } from 'react';
import { Plus, UserMinus, FolderKanban, User } from 'lucide-react';
import {
  Button,
  Modal,
  LoadingSpinner,
  EmptyState,
  ConfirmDialog,
} from '../common';
import { AssignmentForm } from './AssignmentForm';
import { employeesService } from '../../services/employeesService';
import type {
  EmployeeDetailResponse,
  ProjectResponse,
  EmployeeProjectAssignment,
} from '../../types';

export const EmployeeProjectsTab: React.FC = () => {
  const [assignments, setAssignments] = useState<EmployeeProjectAssignment[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Modal states
  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [selectedAssignment, setSelectedAssignment] = useState<EmployeeProjectAssignment | null>(
    null
  );
  const [isSubmitting, setIsSubmitting] = useState(false);

  const fetchAssignments = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      // Fetch all employees with their projects
      const employeesResult = await employeesService.getAll(1, 100);
      const assignmentsList: EmployeeProjectAssignment[] = [];

      // Fetch detailed info for each employee to get their projects
      const employeeDetails = await Promise.all(
        employeesResult.items.map((emp) => employeesService.getById(emp.id))
      );

      // Build assignments list
      employeeDetails.forEach((employee: EmployeeDetailResponse) => {
        employee.projects.forEach((project: ProjectResponse) => {
          assignmentsList.push({
            employeeId: employee.id,
            employeeName: `${employee.firstName} ${employee.lastName}`,
            employeeEmail: employee.email,
            projectId: project.id,
            projectName: project.name,
            projectStartDate: project.startDate,
            projectEndDate: project.endDate,
          });
        });
      });

      setAssignments(assignmentsList);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch assignments');
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchAssignments();
  }, [fetchAssignments]);

  const handleCreate = () => {
    setIsFormModalOpen(true);
  };

  const handleRemoveClick = (assignment: EmployeeProjectAssignment) => {
    setSelectedAssignment(assignment);
    setIsDeleteDialogOpen(true);
  };

  const handleFormSubmit = async (employeeId: string, projectId: string) => {
    setIsSubmitting(true);
    try {
      await employeesService.assignToProject(employeeId, projectId);
      setIsFormModalOpen(false);
      fetchAssignments();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create assignment');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRemoveConfirm = async () => {
    if (!selectedAssignment) return;
    setIsSubmitting(true);
    try {
      await employeesService.removeFromProject(
        selectedAssignment.employeeId,
        selectedAssignment.projectId
      );
      setIsDeleteDialogOpen(false);
      setSelectedAssignment(null);
      fetchAssignments();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to remove assignment');
    } finally {
      setIsSubmitting(false);
    }
  };

  const formatDate = (dateString: string | null) => {
    if (!dateString) return '-';
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
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
            Employee Assignments
          </h2>
          <p className="text-sm text-gray-500 dark:text-gray-400">
            Manage employee-project assignments
          </p>
        </div>
        <Button onClick={handleCreate}>
          <Plus className="w-4 h-4 mr-2" />
          New Assignment
        </Button>
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
          <LoadingSpinner text="Loading assignments..." />
        ) : assignments.length === 0 ? (
          <EmptyState
            title="No assignments found"
            description="Get started by assigning employees to projects"
            action={
              <Button onClick={handleCreate}>
                <Plus className="w-4 h-4 mr-2" />
                New Assignment
              </Button>
            }
          />
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
              <thead className="bg-gray-50 dark:bg-gray-900/50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Employee
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Project
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Project Timeline
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                {assignments.map((assignment) => (
                  <tr
                    key={`${assignment.employeeId}-${assignment.projectId}`}
                    className="hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors"
                  >
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 rounded-full bg-primary-100 dark:bg-primary-900/30 flex items-center justify-center">
                          <User className="w-4 h-4 text-primary-600 dark:text-primary-400" />
                        </div>
                        <div>
                          <div className="text-sm font-medium text-gray-900 dark:text-white">
                            {assignment.employeeName}
                          </div>
                          <div className="text-sm text-gray-500 dark:text-gray-400">
                            {assignment.employeeEmail}
                          </div>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center gap-2">
                        <FolderKanban className="w-4 h-4 text-gray-400" />
                        <span className="text-sm text-gray-900 dark:text-white">
                          {assignment.projectName}
                        </span>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm text-gray-500 dark:text-gray-400">
                        {formatDate(assignment.projectStartDate)}
                        {assignment.projectEndDate &&
                          ` - ${formatDate(assignment.projectEndDate)}`}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right">
                      <button
                        onClick={() => handleRemoveClick(assignment)}
                        className="inline-flex items-center gap-1 px-3 py-1.5 text-sm font-medium text-red-600 hover:text-red-700 hover:bg-red-50 dark:text-red-400 dark:hover:text-red-300 dark:hover:bg-red-900/30 rounded-lg transition-colors"
                        title="Remove assignment"
                      >
                        <UserMinus className="w-4 h-4" />
                        Remove
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="bg-white dark:bg-gray-800 rounded-xl p-4 border border-gray-200 dark:border-gray-700">
          <div className="text-2xl font-bold text-gray-900 dark:text-white">
            {assignments.length}
          </div>
          <div className="text-sm text-gray-500 dark:text-gray-400">
            Total Assignments
          </div>
        </div>
        <div className="bg-white dark:bg-gray-800 rounded-xl p-4 border border-gray-200 dark:border-gray-700">
          <div className="text-2xl font-bold text-gray-900 dark:text-white">
            {new Set(assignments.map((a) => a.employeeId)).size}
          </div>
          <div className="text-sm text-gray-500 dark:text-gray-400">
            Employees with Projects
          </div>
        </div>
        <div className="bg-white dark:bg-gray-800 rounded-xl p-4 border border-gray-200 dark:border-gray-700">
          <div className="text-2xl font-bold text-gray-900 dark:text-white">
            {new Set(assignments.map((a) => a.projectId)).size}
          </div>
          <div className="text-sm text-gray-500 dark:text-gray-400">
            Projects with Team Members
          </div>
        </div>
      </div>

      {/* Create Modal */}
      <Modal
        isOpen={isFormModalOpen}
        onClose={() => setIsFormModalOpen(false)}
        title="Assign Employee to Project"
      >
        <AssignmentForm
          onSubmit={handleFormSubmit}
          onCancel={() => setIsFormModalOpen(false)}
          isLoading={isSubmitting}
        />
      </Modal>

      {/* Remove Confirmation */}
      <ConfirmDialog
        isOpen={isDeleteDialogOpen}
        onClose={() => {
          setIsDeleteDialogOpen(false);
          setSelectedAssignment(null);
        }}
        onConfirm={handleRemoveConfirm}
        title="Remove Assignment"
        message={`Are you sure you want to remove "${selectedAssignment?.employeeName}" from "${selectedAssignment?.projectName}"?`}
        confirmText="Remove"
        isLoading={isSubmitting}
      />
    </div>
  );
};
