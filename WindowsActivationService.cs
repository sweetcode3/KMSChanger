public class WindowsActivationService
{
    private readonly ILogger _logger;
    private readonly ConfigService _configService;

    public WindowsActivationService(ILogger logger, ConfigService configService)
    {
        _logger = logger;
        _configService = configService;
    }

    public async Task<ActivationResult> GetActivationStatus()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "slmgr.vbs",
                    Arguments = "/dli",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return ParseActivationStatus(output);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка получения статуса активации: {ex.Message}");
            return new ActivationResult 
            { 
                IsSuccess = false, 
                ErrorDetails = ex.Message 
            };
        }
    }

    private ActivationResult ParseActivationStatus(string output)
    {
        // Парсинг вывода slmgr
        var result = new ActivationResult();
        // ... логика парсинга
        return result;
    }
}
