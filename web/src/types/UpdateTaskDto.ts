import { TaskStatus } from './TaskStatus';
import { TaskPriority } from './TaskPriority';

export interface UpdateTaskDto {
  title: string;
  description?: string;
  priority: TaskPriority;
  status: TaskStatus;
  dueDate?: string;
}