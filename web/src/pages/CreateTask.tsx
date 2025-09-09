import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { ArrowLeft, Plus, AlertTriangle } from 'lucide-react';
import { useCreateTask } from '@/hooks/useTasks';
import { TaskPriority } from '@/types';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';

interface FormData {
  title: string;
  description: string;
  priority: TaskPriority;
  dueDate: string;
}

interface FormErrors {
  title?: string;
  priority?: string;
}

export default function CreateTask() {
  const navigate = useNavigate();
  const createTaskMutation = useCreateTask();
  const [showConfirmation, setShowConfirmation] = useState(false);
  
  const [formData, setFormData] = useState<FormData>({
    title: '',
    description: '',
    priority: TaskPriority.Medium as TaskPriority,
    dueDate: '',
  });

  const [errors, setErrors] = useState<FormErrors>({});

  const validateForm = (): boolean => {
    const newErrors: FormErrors = {};

    if (!formData.title.trim()) {
      newErrors.title = 'Title is required';
    } else if (formData.title.trim().length < 3) {
      newErrors.title = 'Title must be at least 3 characters long';
    }

    if (!formData.priority) {
      newErrors.priority = 'Priority is required';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    if (formData.priority === TaskPriority.High) {
      setShowConfirmation(true);
      return;
    }

    await submitTask();
  };

  const submitTask = async () => {
    try {
      const task = await createTaskMutation.mutateAsync({
        title: formData.title.trim(),
        description: formData.description.trim() || undefined,
        priority: formData.priority,
        dueDate: formData.dueDate || undefined,
      });
      navigate(`/tasks/${task.id}`);
    } catch {
      // Task creation errors are handled by user feedback in the form
    }
  };

  const handleConfirmHighPriority = async () => {
    setShowConfirmation(false);
    await submitTask();
  };

  const handleInputChange = (field: keyof FormData) => (value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    // Clear error when user starts typing
    if (errors[field as keyof FormErrors]) {
      setErrors(prev => ({ ...prev, [field]: undefined }));
    }
  };

  return (
    <>
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" asChild>
            <Link to="/tasks">
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
          <h1 className="text-2xl font-bold">Create New Task</h1>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Task Details</CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <Label htmlFor="title">
                  Title <span className="text-destructive">*</span>
                </Label>
                <Input
                  id="title"
                  value={formData.title}
                  onChange={(e) => handleInputChange('title')(e.target.value)}
                  placeholder="Enter task title..."
                  className={errors.title ? 'border-destructive' : ''}
                />
                {errors.title && (
                  <p className="text-sm text-destructive mt-1">{errors.title}</p>
                )}
              </div>

              <div>
                <Label htmlFor="description">Description</Label>
                <Input
                  id="description"
                  value={formData.description}
                  onChange={(e) => handleInputChange('description')(e.target.value)}
                  placeholder="Optional description..."
                />
              </div>

              <div className="grid gap-4 md:grid-cols-2">
                <div>
                  <Label htmlFor="priority">
                    Priority <span className="text-destructive">*</span>
                  </Label>
                  <Select
                    value={formData.priority}
                    onValueChange={(value) => handleInputChange('priority')(value)}
                  >
                    <SelectTrigger className={errors.priority ? 'border-destructive' : ''}>
                      <SelectValue placeholder="Select priority" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value={TaskPriority.Low}>Low</SelectItem>
                      <SelectItem value={TaskPriority.Medium}>Medium</SelectItem>
                      <SelectItem value={TaskPriority.High}>High</SelectItem>
                    </SelectContent>
                  </Select>
                  {errors.priority && (
                    <p className="text-sm text-destructive mt-1">{errors.priority}</p>
                  )}
                </div>

                <div>
                  <Label htmlFor="dueDate">Due Date</Label>
                  <Input
                    id="dueDate"
                    type="date"
                    value={formData.dueDate}
                    onChange={(e) => handleInputChange('dueDate')(e.target.value)}
                    min={new Date().toISOString().split('T')[0]}
                    className="dark:[color-scheme:dark]"
                  />
                </div>
              </div>

              <div className="flex items-center justify-end gap-4 pt-4">
                <Button
                  type="button"
                  variant="outline"
                  asChild
                  disabled={createTaskMutation.isPending}
                >
                  <Link to="/tasks">Cancel</Link>
                </Button>
                <Button
                  type="submit"
                  disabled={createTaskMutation.isPending}
                >
                  <Plus className="mr-2 h-4 w-4" />
                  {createTaskMutation.isPending ? 'Creating...' : 'Create Task'}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      </div>

      {/* High Priority Confirmation Dialog */}
      <Dialog open={showConfirmation} onOpenChange={setShowConfirmation}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 text-orange-500" />
              High Priority Task
            </DialogTitle>
            <DialogDescription>
              You are creating a high priority task. High priority tasks require immediate attention 
              and may impact other work. Are you sure you want to proceed?
            </DialogDescription>
          </DialogHeader>
          <div className="py-4">
            <div className="bg-orange-50 dark:bg-orange-950/20 border border-orange-200 dark:border-orange-800 rounded-lg p-4">
              <div className="flex">
                <AlertTriangle className="h-4 w-4 text-orange-500 mr-2 mt-0.5 flex-shrink-0" />
                <div className="text-sm">
                  <strong className="text-orange-800 dark:text-orange-200">Task: </strong>
                  <span className="text-orange-700 dark:text-orange-300">{formData.title}</span>
                  {formData.description && (
                    <>
                      <br />
                      <strong className="text-orange-800 dark:text-orange-200">Description: </strong>
                      <span className="text-orange-700 dark:text-orange-300">{formData.description}</span>
                    </>
                  )}
                </div>
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => setShowConfirmation(false)}
              disabled={createTaskMutation.isPending}
            >
              Cancel
            </Button>
            <Button
              type="button"
              onClick={handleConfirmHighPriority}
              disabled={createTaskMutation.isPending}
            >
              {createTaskMutation.isPending ? 'Creating...' : 'Yes, Create High Priority Task'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}