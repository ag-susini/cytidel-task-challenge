import { TaskPriority } from './TaskPriority';

export interface CreateTaskDto {
  title: string;
  description?: string;
  priority: TaskPriority;
  dueDate?: string;
}