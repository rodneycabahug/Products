namespace Products.API.Tests.Infrastructure;

/// <summary>
/// Singleton fixture shared across all test classes
/// </summary>
public static class SharedTestFixture
{
    private static SqlServerTestContainerFixture? _instance;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    // Global test execution lock to ensure only one test runs at a time
    private static readonly SemaphoreSlim _testExecutionLock = new(1, 1);

    public static async Task<SqlServerTestContainerFixture> GetInstanceAsync()
    {
        if (_instance != null)
            return _instance;

        await _lock.WaitAsync();
        try
        {
            if (_instance == null)
            {
                _instance = new SqlServerTestContainerFixture();
                await _instance.InitializeAsync();
            }
            return _instance;
        }
        finally
        {
            _lock.Release();
        }
    }

    public static async Task AcquireTestLockAsync()
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Attempting to acquire test lock...");
        await _testExecutionLock.WaitAsync();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Test lock ACQUIRED!");
    }

    public static void ReleaseTestLock()
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Releasing test lock");
        _testExecutionLock.Release();
    }

    public static async Task DisposeAsync()
    {
        if (_instance != null)
        {
            await _instance.DisposeAsync();
            _instance = null;
        }
    }
}
