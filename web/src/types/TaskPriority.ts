export const TaskPriority = {
  Low: 'Low',
  Medium: 'Medium',
  High: 'High'
} as const;

export type TaskPriority = typeof TaskPriority[keyof typeof TaskPriority];