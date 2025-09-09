namespace Tasker.Infrastructure.Extensions.Configuration;

public class DatabaseSeedingOptions
{
    public const string SectionName = "DatabaseSeeding";

    public bool Enabled { get; set; } = false;
    public bool ClearExistingData { get; set; } = false;
    public int TaskCount { get; set; } = 100;
}