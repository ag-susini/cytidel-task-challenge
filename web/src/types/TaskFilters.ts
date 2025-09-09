import { TaskStatus } from './TaskStatus';
import { TaskPriority } from './TaskPriority';

export interface TaskFilters {
  search?: string;
  status?: TaskStatus;
  priority?: TaskPriority;
  pageNumber?: number;
  pageSize?: number;
}