namespace KMSChanger.Services
{
    public class ConfigurationService
    {
        private const string CONFIG_FILE = "config.json";
        private readonly ILogger _logger;
        private Config? _cachedConfig;

        public ConfigurationService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<Config> LoadConfigAsync()
{
    try
    {
        if (!File.Exists(CONFIG_FILE))
        {
            throw new FileNotFoundException("Конфигурационный файл не найден", CONFIG_FILE);
        }

        var jsonContent = await File.ReadAllTextAsync(CONFIG_FILE);
        var config = JsonSerializer.Deserialize<Config>(jsonContent) 
            ?? throw new InvalidOperationException("Ошибка десериализации конфигурации");

        if (!config.IsValid())
        {
            throw new InvalidOperationException("Неверный формат конфигурационного файла");
        }

        return config;
    }
    catch (Exception ex)
    {
        _logger.LogError($"Ошибка загрузки конфигурации: {ex.Message}");
        throw;
    }
}

    }
}
