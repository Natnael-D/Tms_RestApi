public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EnrollmentWorker> _logger;

    public EnrollmentWorker(IServiceScopeFactory scopeFactory, ILogger<EnrollmentWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public void ProcessBatch()
    {
        // TODO2: Create a short-lived scope using the injected factory
        using var scope = _scopeFactory.CreateScope();
        
        // TODO3: Resolve the scoped service from the new scope's provider
        var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
        
        // TODO4: Use the service, then let the 'using' block dispose the scope
        _logger.LogInformation("Processing enrollment batch...");
        var enrollments = enrollmentService.GetAllAsync().GetAwaiter().GetResult();
        _logger.LogInformation("Found {Count} enrollments", enrollments.Count);
        
        // Scope is disposed automatically at the end of the using block
    }
}