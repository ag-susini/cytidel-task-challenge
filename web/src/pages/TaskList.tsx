import { useState, useRef, useCallback, useEffect, useMemo } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Filter, Search, ChevronRight, Calendar, AlertCircle, Loader2, X } from 'lucide-react';
import { useInfiniteTasks, useTaskStats } from '@/hooks/useTasks';
import { TaskStatus, TaskPriority } from '@/types';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import TaskStatsChart from '@/components/TaskStatsChart';
import TaskStatsChartSkeleton from '@/components/TaskStatsChartSkeleton';
import { useFilters } from '@/context/FilterContext';

export default function TaskList() {
  const { filters, updateFilter, clearFilters } = useFilters();
  const [searchInput, setSearchInput] = useState(filters.search);
  const [debouncedSearch, setDebouncedSearch] = useState(filters.search);
  
  // Debounce search input and update filter context
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(searchInput);
      updateFilter('search', searchInput);
    }, 500); // 500ms debounce delay
    
    return () => clearTimeout(timer);
  }, [searchInput, updateFilter]);
  
  // Initialize search input from context when component mounts
  useEffect(() => {
    setSearchInput(filters.search);
    setDebouncedSearch(filters.search);
  }, []);
  
  const queryParams = useMemo(() => ({
    search: debouncedSearch || undefined,
    status: filters.status === 'all' ? undefined : filters.status as TaskStatus,
    priority: filters.priority === 'all' ? undefined : filters.priority as TaskPriority,
  }), [debouncedSearch, filters.status, filters.priority]);

  const {
    data,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
    isLoading,
    isError,
  } = useInfiniteTasks(queryParams);

  const { data: stats, isLoading: isLoadingStats } = useTaskStats();

  const observerTarget = useRef<HTMLDivElement>(null);

  const handleObserver = useCallback(
    (entries: IntersectionObserverEntry[]) => {
      const [target] = entries;
      if (target.isIntersecting && hasNextPage && !isFetchingNextPage) {
        fetchNextPage();
      }
    },
    [fetchNextPage, hasNextPage, isFetchingNextPage]
  );

  useEffect(() => {
    const element = observerTarget.current;
    if (!element || !hasNextPage) return;

    const observer = new IntersectionObserver(handleObserver, {
      threshold: 0.1,
      rootMargin: '100px',
    });

    observer.observe(element);
    return () => observer.unobserve(element);
  }, [handleObserver, hasNextPage]);

  const allTasks = data?.pages.flatMap((page) => page.items) ?? [];
  
  // Deduplicate tasks by ID to prevent duplicate key warnings
  const uniqueTasks = allTasks.filter((task, index, arr) => 
    arr.findIndex(t => t.id === task.id) === index
  );

  const getPriorityColor = (priority: TaskPriority) => {
    switch (priority) {
      case 'High':
        return 'destructive';
      case 'Medium':
        return 'secondary';
      case 'Low':
        return 'outline';
      default:
        return 'default';
    }
  };

  const getStatusColor = (status: TaskStatus) => {
    switch (status) {
      case 'Completed':
        return 'bg-green-500/10 text-green-500 border-green-500/20';
      case 'InProgress':
        return 'bg-primary/10 text-primary border-primary/20';
      case 'Pending':
        return 'bg-yellow-500/10 text-yellow-500 border-yellow-500/20';
      case 'Archived':
        return 'bg-gray-500/10 text-gray-500 border-gray-500/20';
      default:
        return '';
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">Tasks</h1>
        <Button asChild>
          <Link to="/tasks/new">
            <Plus className="mr-2 h-4 w-4" />
            New Task
          </Link>
        </Button>
      </div>

      {isLoadingStats ? <TaskStatsChartSkeleton /> : stats && <TaskStatsChart stats={stats} />}

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between min-h-[2rem]">
            <CardTitle className="flex items-center gap-2">
              <Filter className="h-5 w-5" />
              Filters
            </CardTitle>
            <div className="h-8 flex items-center">
              {(filters.search || filters.status !== 'all' || filters.priority !== 'all') && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => {
                    clearFilters();
                    setSearchInput('');
                    setDebouncedSearch('');
                  }}
                  className="h-8 px-2 lg:px-3"
                >
                  Clear filters
                  <X className="ml-2 h-4 w-4" />
                </Button>
              )}
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-3">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder="Search tasks..."
                value={searchInput}
                onChange={(e) => setSearchInput(e.target.value)}
                className="pl-10"
              />
            </div>
            <Select value={filters.status} onValueChange={(value) => updateFilter('status', value)}>
              <SelectTrigger>
                <SelectValue placeholder="All Statuses" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Statuses</SelectItem>
                <SelectItem value="Pending">Pending</SelectItem>
                <SelectItem value="InProgress">In Progress</SelectItem>
                <SelectItem value="Completed">Completed</SelectItem>
                <SelectItem value="Archived">Archived</SelectItem>
              </SelectContent>
            </Select>
            <Select value={filters.priority} onValueChange={(value) => updateFilter('priority', value)}>
              <SelectTrigger>
                <SelectValue placeholder="All Priorities" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Priorities</SelectItem>
                <SelectItem value="Low">Low</SelectItem>
                <SelectItem value="Medium">Medium</SelectItem>
                <SelectItem value="High">High</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      <div>
        {isLoading ? (
          Array.from({ length: 5 }).map((_, i) => (
            <Card key={i} className="mb-4">
              <CardContent className="p-6">
                <div className="flex items-center justify-between">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-3 mb-2">
                      <Skeleton className="h-6 w-48" />
                      <div className="flex items-center gap-2 flex-shrink-0">
                        <Skeleton className="h-6 w-16" />
                        <Skeleton className="h-6 w-20" />
                        <Skeleton className="h-5 w-24" />
                      </div>
                    </div>
                    <Skeleton className="h-4 w-3/4" />
                  </div>
                  <Skeleton className="h-5 w-5 ml-4" />
                </div>
              </CardContent>
            </Card>
          ))
        ) : isError ? (
          <Card>
            <CardContent className="p-6">
              <div className="flex items-center gap-2 text-destructive">
                <AlertCircle className="h-5 w-5" />
                <p>Failed to load tasks. Please try again.</p>
              </div>
            </CardContent>
          </Card>
        ) : uniqueTasks.length === 0 ? (
          <Card>
            <CardContent className="p-12 text-center">
              <p className="text-muted-foreground">No tasks found</p>
              <Button asChild className="mt-4">
                <Link to="/tasks/new">
                  <Plus className="mr-2 h-4 w-4" />
                  Create your first task
                </Link>
              </Button>
            </CardContent>
          </Card>
        ) : (
          uniqueTasks.map((task, index) => (
            <Link key={`${task.id}-${index}`} to={`/tasks/${task.id}`}>
              <Card className="transition-all hover:shadow-lg dark:hover:shadow-xl dark:hover:shadow-black/40 hover:border-primary/50 dark:hover:border-primary/30 mb-4 dark:bg-card/60 dark:hover:bg-card">
                <CardContent className="p-6">
                  <div className="flex items-center justify-between">
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-3 mb-2">
                        <h3 className="text-lg font-semibold truncate">{task.title}</h3>
                        <div className="flex items-center gap-2 flex-shrink-0">
                          <Badge variant={getPriorityColor(task.priority)}>
                            {task.priority}
                          </Badge>
                          <Badge className={getStatusColor(task.status)} variant="outline">
                            {task.status}
                          </Badge>
                          {task.dueDate && (
                            <div className="flex items-center gap-1 text-sm text-muted-foreground">
                              <Calendar className="h-3 w-3 text-muted-foreground" />
                              {new Date(task.dueDate).toLocaleDateString()}
                            </div>
                          )}
                        </div>
                      </div>
                      {task.description && (
                        <p className="text-sm text-muted-foreground line-clamp-2">
                          {task.description}
                        </p>
                      )}
                    </div>
                    <ChevronRight className="h-5 w-5 text-muted-foreground ml-4 flex-shrink-0" />
                  </div>
                </CardContent>
              </Card>
            </Link>
          ))
        )}

        {hasNextPage && (
          <div ref={observerTarget} className="h-20 flex items-center justify-center">
            {isFetchingNextPage ? (
              <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
            ) : (
              <div className="h-1" />
            )}
          </div>
        )}
      </div>
    </div>
  );
}