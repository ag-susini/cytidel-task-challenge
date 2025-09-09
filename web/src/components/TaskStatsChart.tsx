import { PieChart, Pie, Tooltip, ResponsiveContainer, Cell } from 'recharts';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import type { TaskStats } from '@/types';

interface TaskStatsChartProps {
  stats: TaskStats;
}

export default function TaskStatsChart({ stats }: TaskStatsChartProps) {
  const statusData = stats ? [
    { name: 'Pending', value: stats.pendingCount, key: 'pending' },
    { name: 'In Progress', value: stats.inProgressCount, key: 'inProgress' },
    { name: 'Completed', value: stats.completedCount, key: 'completed' },
    { name: 'Archived', value: stats.archivedCount, key: 'archived' },
  ].filter(item => item.value > 0) : [];

  const statusColors: Record<string, string> = {
    pending: '#eab308',      // Yellow for Pending
    inProgress: 'hsl(200, 100%, 50%)', // Primary blue for In Progress  
    completed: '#10b981',    // Green for Completed
    archived: '#6b7280',     // Gray for Archived
  };

  return (
    <div className="grid gap-6">
      <Card className="shadow-lg dark:shadow-xl dark:shadow-black/30 dark:bg-card/80 dark:border-slate-700">
        <CardHeader>
          <CardTitle className="text-lg font-semibold">Task Overview</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-4">
              <div className="text-center">
                <div className="text-2xl font-bold text-primary">{stats?.totalCount || 0}</div>
                <div className="text-sm text-muted-foreground">Total Tasks</div>
              </div>
              
              <div className="space-y-2">
                {statusData.map((item, index) => (
                  <div key={index} className="flex items-center justify-between p-2 rounded-lg border bg-muted/20">
                    <div className="flex items-center gap-2">
                      <div 
                        className="w-3 h-3 rounded-full" 
                        style={{ backgroundColor: statusColors[item.key] }}
                      />
                      <span className="text-sm font-medium">{item.name}</span>
                    </div>
                    <span className="text-sm font-bold">{item.value}</span>
                  </div>
                ))}
              </div>
            </div>

            <div className="flex items-center justify-center">
              <ResponsiveContainer width="100%" height={280}>
                <PieChart>
                  <Pie
                    data={statusData}
                    cx="50%"
                    cy="50%"
                    outerRadius={125}
                    fill="#8884d8"
                    dataKey="value"
                    strokeWidth={2}
                    stroke="hsl(var(--background))"
                  >
                    {statusData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={statusColors[entry.key]} />
                    ))}
                  </Pie>
                  <Tooltip 
                    formatter={(value, name) => [value, name]}
                    itemStyle={{ color: 'hsl(var(--foreground))' }}
                    labelStyle={{ color: 'hsl(var(--foreground))' }}
                    contentStyle={{ 
                      backgroundColor: 'hsl(var(--popover))',
                      border: '1px solid hsl(var(--border))',
                      borderRadius: '6px',
                      boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)'
                    }}
                  />
                </PieChart>
              </ResponsiveContainer>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}