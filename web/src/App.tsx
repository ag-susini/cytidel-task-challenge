import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { Toaster } from '@/components/ui/toaster';
import { AuthProvider } from '@/context/AuthContext';
import { FilterProvider } from '@/context/FilterContext';
import { useSignalR } from '@/hooks/useSignalR';
import ProtectedRoute from '@/components/ProtectedRoute';
import Layout from './components/Layout';
import TaskList from './pages/TaskList';
import TaskDetail from './pages/TaskDetail';
import CreateTask from './pages/CreateTask';
import Login from './pages/Login';
import Register from './pages/Register';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5,
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function AppContent() {
  // Enable SignalR real-time updates
  useSignalR();
  
  return (
    <BrowserRouter>
      <Routes>
        {/* Auth routes */}
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        
        {/* Protected routes */}
        <Route 
          path="/" 
          element={
            <ProtectedRoute>
              <Layout />
            </ProtectedRoute>
          }
        >
          <Route index element={<Navigate to="/tasks" replace />} />
          <Route path="tasks" element={<TaskList />} />
          <Route path="tasks/new" element={<CreateTask />} />
          <Route path="tasks/:id" element={<TaskDetail />} />
        </Route>
        
        {/* Catch all redirect to login for unauthenticated users */}
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
      <Toaster />
    </BrowserRouter>
  );
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <FilterProvider>
          <AppContent />
          {/* DevTools only appear in development, not in production builds */}
          {process.env.NODE_ENV === 'development' && (
            <ReactQueryDevtools initialIsOpen={false} />
          )}
        </FilterProvider>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;