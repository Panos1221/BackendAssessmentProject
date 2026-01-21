import React from 'react';
import { NavLink } from 'react-router-dom';
import { Building, Users, FolderKanban, UserCheck } from 'lucide-react';

const tabs = [
  { name: 'Departments', path: '/departments', icon: Building },
  { name: 'Employees', path: '/employees', icon: Users },
  { name: 'Projects', path: '/projects', icon: FolderKanban },
  { name: 'Assignments', path: '/assignments', icon: UserCheck },
];

export const TabNavigation: React.FC = () => {
  return (
    <nav className="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex space-x-1 overflow-x-auto">
          {tabs.map((tab) => (
            <NavLink
              key={tab.path}
              to={tab.path}
              className={({ isActive }) => `
                flex items-center gap-2 px-4 py-3 text-sm font-medium
                border-b-2 transition-colors whitespace-nowrap
                ${
                  isActive
                    ? 'border-primary-600 text-primary-600 dark:text-primary-400 dark:border-primary-400'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300 dark:text-gray-400 dark:hover:text-gray-200 dark:hover:border-gray-600'
                }
              `}
            >
              <tab.icon className="w-4 h-4" />
              {tab.name}
            </NavLink>
          ))}
        </div>
      </div>
    </nav>
  );
};
