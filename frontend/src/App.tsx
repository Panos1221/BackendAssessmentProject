import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AppLayout } from './components/Layout';
import { DepartmentsTab } from './components/Departments';
import { EmployeesTab } from './components/Employees';
import { ProjectsTab } from './components/Projects';
import { EmployeeProjectsTab } from './components/EmployeeProjects';
import { ToastProvider } from './components/common';

const App: React.FC = () => {
  return (
    <ToastProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<AppLayout />}>
            <Route index element={<Navigate to="/departments" replace />} />
            <Route path="departments" element={<DepartmentsTab />} />
            <Route path="employees" element={<EmployeesTab />} />
            <Route path="projects" element={<ProjectsTab />} />
            <Route path="assignments" element={<EmployeeProjectsTab />} />
            <Route path="*" element={<Navigate to="/departments" replace />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </ToastProvider>
  );
};

export default App;
