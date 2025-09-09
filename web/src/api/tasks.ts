import { apiClient } from './client';
import type {
  Task,
  CreateTaskDto,
  UpdateTaskDto,
  TaskStats,
  PaginatedResponse,
  TaskFilters,
} from '@/types';

export const tasksApi = {
  getAll: async (filters?: TaskFilters): Promise<PaginatedResponse<Task>> => {
    const { data } = await apiClient.get<PaginatedResponse<Task>>('/tasks', {
      params: filters,
    });
    return data;
  },

  getById: async (id: string): Promise<Task> => {
    const { data } = await apiClient.get<Task>(`/tasks/${id}`);
    return data;
  },

  create: async (task: CreateTaskDto): Promise<Task> => {
    const { data } = await apiClient.post<Task>('/tasks', task);
    return data;
  },

  update: async (id: string, task: UpdateTaskDto): Promise<Task> => {
    const { data } = await apiClient.put<Task>(`/tasks/${id}`, task);
    return data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/tasks/${id}`);
  },

  getStats: async (): Promise<TaskStats> => {
    const { data } = await apiClient.get<TaskStats>('/tasks/stats');
    return data;
  },
};