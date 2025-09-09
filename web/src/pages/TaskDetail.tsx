import { useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { ArrowLeft, Edit, Trash2, Save, X, Calendar } from 'lucide-react';
import { useTask, useUpdateTask, useDeleteTask } from '@/hooks/useTasks';
import { TaskStatus, TaskPriority } from '@/types';
import { useToast } from '@/hooks/use-toast';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
  DialogClose,
} from '@/components/ui/dialog';

export default function TaskDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { toast } = useToast();
  const [isEditing, setIsEditing] = useState(false);
  const [showHighPriorityConfirmation, setShowHighPriorityConfirmation] = useState(false);
  const [editData, setEditData] = useState({
    title: '',
    description: '',
    priority: TaskPriority.Medium as TaskPriority,
    status: TaskStatus.Pending as TaskStatus,
    dueDate: '',
  });

  const { data: task, isLoading, isError } = useTask(id!);
  const updateTaskMutation = useUpdateTask();
  const deleteTaskMutation = useDeleteTask();

  const handleEditStart = () => {
    if (task) {
      setEditData({
        title: task.title,
        description: task.description || '',
        priority: task.priority,
        status: task.status,
        dueDate: task.dueDate ? task.dueDate.split('T')[0] : '',
      });
      setIsEditing(true);
    }
  };

  const handleEditCancel = () => {
    setIsEditing(false);
    setEditData({
      title: '',
      description: '',
      priority: TaskPriority.Medium,
      status: TaskStatus.Pending,
      dueDate: '',
    });
  };

  const handleSave = async () => {
    if (!id || !task) return;

    // Check if priority is being changed to High and wasn't High before
    if (editData.priority === TaskPriority.High && task.priority !== TaskPriority.High) {
      setShowHighPriorityConfirmation(true);
      return;
    }

    await performSave();
  };

  const performSave = async () => {
    if (!id || !task) return;

    try {
      await updateTaskMutation.mutateAsync({
        id,
        task: {
          title: editData.title,
          description: editData.description || undefined,
          priority: editData.priority,
          status: editData.status,
          dueDate: editData.dueDate || undefined,
        },
      });
      setIsEditing(false);
      setShowHighPriorityConfirmation(false);
      toast({
        title: "Task Updated",
        description: "Your task has been successfully updated.",
      });
    } catch {
      toast({
        title: "Update Failed",
        description: "Failed to update the task. Please try again.",
        variant: "destructive",
      });
    }
  };

  const handleHighPriorityCancel = () => {
    setShowHighPriorityConfirmation(false);
    setEditData({ ...editData, priority: task?.priority || TaskPriority.Medium });
  };

  const handleDelete = async () => {
    if (!id) return;

    try {
      await deleteTaskMutation.mutateAsync(id);
      toast({
        title: "Task Deleted",
        description: "Your task has been successfully deleted.",
      });
      navigate('/tasks');
    } catch {
      toast({
        title: "Delete Failed",
        description: "Failed to delete the task. Please try again.",
        variant: "destructive",
      });
    }
  };

  const getPriorityColor = (priority: TaskPriority) => {
    switch (priority) {
      case TaskPriority.High:
        return 'destructive';
      case TaskPriority.Medium:
        return 'secondary';
      case TaskPriority.Low:
        return 'outline';
      default:
        return 'default';
    }
  };

  const getStatusColor = (status: TaskStatus) => {
    switch (status) {
      case TaskStatus.Completed:
        return 'bg-green-500/10 text-green-500 border-green-500/20';
      case TaskStatus.InProgress:
        return 'bg-blue-500/10 text-blue-500 border-blue-500/20';
      case TaskStatus.Pending:
        return 'bg-yellow-500/10 text-yellow-500 border-yellow-500/20';
      case TaskStatus.Archived:
        return 'bg-gray-500/10 text-gray-500 border-gray-500/20';
      default:
        return '';
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Skeleton className="h-10 w-10" />
          <Skeleton className="h-8 w-64" />
        </div>
        <Card>
          <CardContent className="p-6 space-y-4">
            <Skeleton className="h-6 w-3/4" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-2/3" />
            <div className="flex gap-2">
              <Skeleton className="h-6 w-20" />
              <Skeleton className="h-6 w-24" />
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (isError || !task) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" asChild>
            <Link to="/tasks">
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
          <h1 className="text-2xl font-bold">Task Not Found</h1>
        </div>
        <Card>
          <CardContent className="p-6">
            <p className="text-muted-foreground">The task you're looking for could not be found.</p>
            <Button asChild className="mt-4">
              <Link to="/tasks">Back to Tasks</Link>
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" asChild>
            <Link to="/tasks">
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
          <h1 className="text-2xl font-bold">
            {isEditing ? 'Edit Task' : task.title}
          </h1>
        </div>
        <div className="flex items-center gap-2">
          {!isEditing ? (
            <>
              <Button variant="outline" onClick={handleEditStart}>
                <Edit className="mr-2 h-4 w-4" />
                Edit
              </Button>
              <Dialog>
                <DialogTrigger asChild>
                  <Button variant="outline">
                    <Trash2 className="mr-2 h-4 w-4" />
                    Delete
                  </Button>
                </DialogTrigger>
                <DialogContent>
                  <DialogHeader>
                    <DialogTitle>Delete Task</DialogTitle>
                    <DialogDescription>
                      Are you sure you want to delete "{task.title}"? This action cannot be undone.
                    </DialogDescription>
                  </DialogHeader>
                  <DialogFooter>
                    <Button variant="outline" asChild>
                      <DialogClose>Cancel</DialogClose>
                    </Button>
                    <Button
                      variant="destructive"
                      onClick={handleDelete}
                      disabled={deleteTaskMutation.isPending}
                    >
                      {deleteTaskMutation.isPending ? 'Deleting...' : 'Delete'}
                    </Button>
                  </DialogFooter>
                </DialogContent>
              </Dialog>
            </>
          ) : (
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                onClick={handleEditCancel}
                disabled={updateTaskMutation.isPending}
              >
                <X className="mr-2 h-4 w-4" />
                Cancel
              </Button>
              <Button onClick={handleSave} disabled={updateTaskMutation.isPending}>
                <Save className="mr-2 h-4 w-4" />
                {updateTaskMutation.isPending ? 'Saving...' : 'Save'}
              </Button>
            </div>
          )}
        </div>
      </div>

      <Card className="shadow-lg dark:shadow-xl dark:shadow-black/30 dark:bg-card/80 dark:border-slate-700">
        <CardContent className="p-6 space-y-6">
          {isEditing ? (
            <div className="space-y-4">
              <div>
                <Label htmlFor="title">Title</Label>
                <Input
                  id="title"
                  value={editData.title}
                  onChange={(e) => setEditData({ ...editData, title: e.target.value })}
                />
              </div>
              <div>
                <Label htmlFor="description">Description</Label>
                <Input
                  id="description"
                  value={editData.description}
                  onChange={(e) => setEditData({ ...editData, description: e.target.value })}
                  placeholder="Optional description..."
                />
              </div>
              <div className="grid gap-4 md:grid-cols-2">
                <div>
                  <Label htmlFor="priority">Priority</Label>
                  <Select
                    value={editData.priority}
                    onValueChange={(value) => setEditData({ ...editData, priority: value as TaskPriority })}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value={TaskPriority.Low}>Low</SelectItem>
                      <SelectItem value={TaskPriority.Medium}>Medium</SelectItem>
                      <SelectItem value={TaskPriority.High}>High</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div>
                  <Label htmlFor="status">Status</Label>
                  <Select
                    value={editData.status}
                    onValueChange={(value) => setEditData({ ...editData, status: value as TaskStatus })}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value={TaskStatus.Pending}>Pending</SelectItem>
                      <SelectItem value={TaskStatus.InProgress}>In Progress</SelectItem>
                      <SelectItem value={TaskStatus.Completed}>Completed</SelectItem>
                      <SelectItem value={TaskStatus.Archived}>Archived</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
              <div>
                <Label htmlFor="dueDate">Due Date</Label>
                <Input
                  id="dueDate"
                  type="date"
                  value={editData.dueDate}
                  onChange={(e) => setEditData({ ...editData, dueDate: e.target.value })}
                  className="dark:[color-scheme:dark]"
                />
              </div>
            </div>
          ) : (
            <div className="space-y-4">
              <div>
                <h3 className="text-lg font-semibold mb-2">Description</h3>
                <p className="text-muted-foreground">
                  {task.description || 'No description provided.'}
                </p>
              </div>
              <div className="flex flex-wrap gap-4">
                <div className="flex items-center gap-2">
                  <span className="text-sm font-medium">Priority:</span>
                  <Badge variant={getPriorityColor(task.priority)}>
                    {task.priority}
                  </Badge>
                </div>
                <div className="flex items-center gap-2">
                  <span className="text-sm font-medium">Status:</span>
                  <Badge className={getStatusColor(task.status)} variant="outline">
                    {task.status}
                  </Badge>
                </div>
                {task.dueDate && (
                  <div className="flex items-center gap-2">
                    <Calendar className="h-4 w-4 text-muted-foreground" />
                    <span className="text-sm">
                      Due: {new Date(task.dueDate).toLocaleDateString()}
                    </span>
                  </div>
                )}
              </div>
              <div className="pt-4 border-t text-sm text-muted-foreground">
                <p>Created: {new Date(task.createdAt).toLocaleString()}</p>
                {task.updatedAt && task.updatedAt !== task.createdAt && (
                  <p>Updated: {new Date(task.updatedAt).toLocaleString()}</p>
                )}
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* High Priority Confirmation Modal */}
      <Dialog open={showHighPriorityConfirmation} onOpenChange={setShowHighPriorityConfirmation}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>High Priority Task</DialogTitle>
            <DialogDescription>
              Are you sure you want to set this task to high priority? This indicates the task requires immediate attention.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={handleHighPriorityCancel}>
              Cancel
            </Button>
            <Button onClick={performSave} disabled={updateTaskMutation.isPending}>
              {updateTaskMutation.isPending ? 'Saving...' : 'Yes, Set High Priority'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}