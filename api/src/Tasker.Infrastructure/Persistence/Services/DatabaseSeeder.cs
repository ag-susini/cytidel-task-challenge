using Tasker.Domain.Entities;
using Tasker.Domain.Enums;

namespace Tasker.Infrastructure.Persistence.Services;

public class DatabaseSeeder
{
    private readonly TaskDbContext _context;
    private readonly Random _random = new();

    public DatabaseSeeder(TaskDbContext context)
    {
        _context = context;
    }

    public async Task SeedTasksAsync(int count = 100, bool clearExisting = false)
    {
        if (clearExisting)
        {
            _context.Tasks.RemoveRange(_context.Tasks);
            await _context.SaveChangesAsync();
        }

        var tasks = GenerateTasks(count);
        
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();
    }

    private List<TaskItem> GenerateTasks(int count)
    {
        var taskTemplates = GetTaskTemplates();
        var tasks = new List<TaskItem>();
        var now = DateTime.UtcNow;

        for (int i = 0; i < count; i++)
        {
            var template = taskTemplates[_random.Next(taskTemplates.Count)];
            var createdDaysAgo = _random.Next(0, 90);
            var createdAt = now.AddDays(-createdDaysAgo);
            
            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = template.Title,
                Description = template.Description,
                Priority = GetRandomPriority(),
                Status = GetRandomStatus(),
                CreatedAt = createdAt,
                UpdatedAt = ShouldHaveUpdate() ? createdAt.AddDays(_random.Next(1, 30)) : null,
                DueDate = ShouldHaveDueDate() ? createdAt.AddDays(_random.Next(7, 60)) : null
            };

            tasks.Add(task);
        }

        return tasks;
    }

    private Priority GetRandomPriority()
    {
        // Weight priorities: Low=50%, Medium=35%, High=15%
        var rand = _random.NextDouble();
        return rand switch
        {
            < 0.15 => Priority.High,
            < 0.50 => Priority.Medium,
            _ => Priority.Low
        };
    }

    private Status GetRandomStatus()
    {
        // Weight statuses: Pending=40%, InProgress=25%, Completed=30%, Archived=5%
        var rand = _random.NextDouble();
        return rand switch
        {
            < 0.40 => Status.Pending,
            < 0.65 => Status.InProgress,
            < 0.95 => Status.Completed,
            _ => Status.Archived
        };
    }

    private bool ShouldHaveUpdate() => _random.NextDouble() < 0.6; // 60% chance of having updates
    private bool ShouldHaveDueDate() => _random.NextDouble() < 0.8; // 80% chance of having due date

    private static List<(string Title, string Description)> GetTaskTemplates()
    {
        return new List<(string Title, string Description)>
        {
            // Development Tasks
            ("Implement user authentication system", "Add JWT-based authentication with role-based access control and password reset functionality"),
            ("Fix login page styling issues", "Resolve responsive design problems and improve mobile experience on login form"),
            ("Add email notification service", "Integrate SendGrid or similar service for transactional emails and notifications"),
            ("Optimize database queries", "Review and optimize slow-running queries, add proper indexing for better performance"),
            ("Update API documentation", "Refresh Swagger documentation with new endpoints and add comprehensive examples"),
            ("Implement caching layer", "Add Redis caching for frequently accessed data to improve response times"),
            ("Set up CI/CD pipeline", "Configure GitHub Actions for automated testing, building, and deployment"),
            ("Add unit tests for user service", "Increase test coverage for user-related business logic and edge cases"),
            ("Implement file upload functionality", "Allow users to upload profile pictures and document attachments"),
            ("Fix memory leak in data processor", "Investigate and resolve memory consumption issues in background service"),
            
            // Security Tasks
            ("Conduct security audit", "Perform comprehensive security review of authentication and authorization mechanisms"),
            ("Implement input validation", "Add proper input sanitization and validation across all API endpoints"),
            ("Update dependencies", "Review and update all NuGet packages to latest secure versions"),
            ("Configure SSL certificates", "Set up and configure SSL certificates for production environment"),
            ("Implement rate limiting", "Add API rate limiting to prevent abuse and ensure service stability"),
            ("Set up monitoring alerts", "Configure application monitoring and alerting for security incidents"),
            ("Review user permissions", "Audit and update user role permissions according to security best practices"),
            ("Implement audit logging", "Add comprehensive audit trail for all user actions and system changes"),
            
            // Infrastructure Tasks
            ("Set up backup strategy", "Implement automated database backups with restore testing procedures"),
            ("Configure load balancing", "Set up load balancer for high availability and better performance"),
            ("Migrate to cloud infrastructure", "Plan and execute migration from on-premises to cloud infrastructure"),
            ("Optimize server performance", "Analyze server metrics and optimize resource allocation and usage"),
            ("Set up disaster recovery", "Implement disaster recovery plan and test failover procedures"),
            ("Configure monitoring dashboard", "Set up comprehensive monitoring dashboard for system health metrics"),
            ("Implement log aggregation", "Centralize log collection and analysis using ELK stack or similar"),
            ("Upgrade server hardware", "Plan and execute server hardware upgrade to meet growing demands"),
            
            // UI/UX Tasks
            ("Redesign user dashboard", "Create modern, intuitive dashboard design with improved user experience"),
            ("Fix mobile responsive issues", "Resolve layout problems and improve mobile user experience across all pages"),
            ("Implement dark mode", "Add dark theme option with proper color scheme and user preferences"),
            ("Add accessibility features", "Implement WCAG 2.1 AA compliance for screen readers and keyboard navigation"),
            ("Optimize page load times", "Improve frontend performance through code splitting and asset optimization"),
            ("Update branding elements", "Apply new brand guidelines to logos, colors, and typography across the application"),
            ("Implement search functionality", "Add powerful search with filters, sorting, and advanced query options"),
            ("Add data visualization", "Create interactive charts and graphs for better data representation"),
            
            // Business Logic Tasks
            ("Implement reporting system", "Create comprehensive reporting module with export capabilities"),
            ("Add workflow automation", "Implement business process automation for common workflows"),
            ("Integrate third-party APIs", "Connect with external services for enhanced functionality"),
            ("Implement data export", "Add ability to export data in various formats (CSV, Excel, PDF)"),
            ("Add notification preferences", "Allow users to customize notification settings and delivery methods"),
            ("Implement user onboarding", "Create guided onboarding flow for new users"),
            ("Add team collaboration features", "Implement team workspaces, sharing, and collaboration tools"),
            ("Implement data archiving", "Set up automated data archiving for compliance and storage optimization"),
            
            // Maintenance Tasks
            ("Clean up deprecated code", "Remove unused code, dependencies, and legacy functionality"),
            ("Update documentation", "Refresh technical documentation, API guides, and user manuals"),
            ("Refactor legacy modules", "Modernize old code modules to follow current best practices"),
            ("Optimize build process", "Improve build times and deployment efficiency"),
            ("Review error handling", "Audit and improve error handling and user-friendly error messages"),
            ("Update coding standards", "Review and update team coding standards and style guidelines"),
            ("Implement code reviews", "Establish formal code review process and quality gates"),
            ("Set up automated testing", "Implement comprehensive automated testing strategy"),
            
            // Data Tasks
            ("Migrate legacy data", "Transfer data from old system to new database structure"),
            ("Implement data validation", "Add comprehensive data integrity checks and validation rules"),
            ("Set up data warehousing", "Implement data warehouse for analytics and reporting"),
            ("Optimize data storage", "Review and optimize database schema and storage efficiency"),
            ("Implement data retention", "Set up automated data retention policies for compliance"),
            ("Add data encryption", "Implement encryption for sensitive data at rest and in transit"),
            ("Set up data synchronization", "Implement real-time data sync between systems"),
            ("Create data backup tests", "Establish regular backup verification and restore testing"),
            
            // Operations Tasks
            ("Plan feature rollout", "Coordinate deployment strategy for major feature release"),
            ("Update user training", "Create training materials and conduct user education sessions"),
            ("Review system capacity", "Analyze system capacity and plan for future growth"),
            ("Implement feature flags", "Set up feature toggle system for safer deployments"),
            ("Plan maintenance window", "Schedule and coordinate system maintenance activities"),
            ("Update support documentation", "Refresh help desk documentation and troubleshooting guides"),
            ("Review vendor contracts", "Evaluate and renew software licenses and service contracts"),
            ("Conduct performance testing", "Execute load testing and performance benchmarking"),
            
            // Compliance Tasks
            ("GDPR compliance review", "Ensure application meets GDPR requirements for data protection"),
            ("SOC 2 audit preparation", "Prepare systems and documentation for SOC 2 compliance audit"),
            ("Update privacy policy", "Review and update privacy policy to reflect current practices"),
            ("Implement data subject rights", "Add functionality for data subject access and deletion requests"),
            ("Review access controls", "Audit user access controls and implement principle of least privilege"),
            ("Document security procedures", "Create comprehensive security procedures and incident response plans"),
            ("Conduct vulnerability scanning", "Perform regular security scans and address identified vulnerabilities"),
            ("Update compliance training", "Refresh compliance training materials for all team members")
        };
    }
}