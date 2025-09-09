import { TaskPriority } from './TaskPriority';
import { TaskStatus } from './TaskStatus';

export interface Task {
  id: string;
  title: string;
  description?: string;
  priority: TaskPriority;
  status: TaskStatus;
  dueDate?: string;
  createdAt: string;
  updatedAt?: string;
}