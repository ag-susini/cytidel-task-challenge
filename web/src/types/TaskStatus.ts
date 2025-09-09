export const TaskStatus = {
  Pending: 'Pending',
  InProgress: 'InProgress',
  Completed: 'Completed',
  Archived: 'Archived'
} as const;

export type TaskStatus = typeof TaskStatus[keyof typeof TaskStatus];