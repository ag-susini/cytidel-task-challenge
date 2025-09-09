import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';

export default function TaskStatsChartSkeleton() {
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
                <Skeleton className="h-8 w-16 mx-auto mb-1" />
                <Skeleton className="h-4 w-20 mx-auto" />
              </div>
              
              <div className="space-y-2">
                {[1, 2, 3, 4].map((index) => (
                  <div key={index} className="flex items-center justify-between p-2 rounded-lg border bg-muted/20">
                    <div className="flex items-center gap-2">
                      <Skeleton className="w-3 h-3 rounded-full" />
                      <Skeleton className="h-4 w-20" />
                    </div>
                    <Skeleton className="h-4 w-8" />
                  </div>
                ))}
              </div>
            </div>

            <div className="flex items-center justify-center">
              <Skeleton className="h-[280px] w-[280px] rounded-full" />
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}