import {
  useQuery,
  useMutation,
  useInfiniteQuery,
  useQueryClient,
} from '@tanstack/react-query';
import { tasksApi } from '../api/tasks';
import type {
  Task,
  CreateTaskDto,
  UpdateTaskDto,
  TaskFilters,
} from '../types';

export const useTasks = (filters?: TaskFilters) => {
  return useQuery({
    queryKey: ['tasks', filters],
    queryFn: () => tasksApi.getAll(filters),
  });
};

export const useInfiniteTasks = (filters?: Omit<TaskFilters, 'pageNumber'>) => {
  return useInfiniteQuery({
    queryKey: ['tasks', 'infinite', filters],
    queryFn: ({ pageParam = 1 }) => {
      return tasksApi.getAll({ ...filters, pageNumber: pageParam, pageSize: 20 });
    },
    getNextPageParam: (lastPage) => {
      return lastPage.hasNextPage ? lastPage.pageNumber + 1 : undefined;
    },
    initialPageParam: 1,
  });
};

export const useTask = (id: string) => {
  return useQuery({
    queryKey: ['task', id],
    queryFn: () => tasksApi.getById(id),
    enabled: !!id,
  });
};

export const useTaskStats = () => {
  return useQuery({
    queryKey: ['taskStats'],
    queryFn: tasksApi.getStats,
  });
};

export const useCreateTask = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (task: CreateTaskDto) => tasksApi.create(task),
    onSuccess: (createdTask) => {
      // Cache the created task so it's immediately available when navigating to detail page
      queryClient.setQueryData(['task', createdTask.id], createdTask);
      // Invalidate the lists to refresh them
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      queryClient.invalidateQueries({ queryKey: ['taskStats'] });
    },
  });
};

export const useUpdateTask = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, task }: { id: string; task: UpdateTaskDto }) =>
      tasksApi.update(id, task),
    onMutate: async ({ id, task }) => {
      await queryClient.cancelQueries({ queryKey: ['task', id] });
      const previousTask = queryClient.getQueryData<Task>(['task', id]);
      
      if (previousTask) {
        queryClient.setQueryData<Task>(['task', id], {
          ...previousTask,
          ...task,
        });
      }
      
      return { previousTask };
    },
    onError: (_err, { id }, context) => {
      if (context?.previousTask) {
        queryClient.setQueryData(['task', id], context.previousTask);
      }
    },
    onSettled: (_data, _error, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['task', id] });
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      queryClient.invalidateQueries({ queryKey: ['taskStats'] });
    },
  });
};

export const useDeleteTask = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => tasksApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      queryClient.invalidateQueries({ queryKey: ['taskStats'] });
    },
  });
};