import React from 'react';
import { Outlet } from 'react-router-dom';
import { Header } from './Header';
import { TabNavigation } from './TabNavigation';
import { useTheme } from '../../hooks/useTheme';

export const AppLayout: React.FC = () => {
  const { theme, toggleTheme } = useTheme();

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 transition-colors">
      <Header theme={theme} onToggleTheme={toggleTheme} />
      <TabNavigation />
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
        <Outlet />
      </main>
    </div>
  );
};
