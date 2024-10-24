namespace KMSChanger.Services
{
    public class ConfigurationService
    {
        private const string CONFIG_FILE = "config.json";
        private readonly ILogger _logger;
        private Config _cachedConfig;

        public ConfigurationService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<Config> LoadConfigAsync()
        {
            if (_cachedConfig != null)
            {
                return _cachedConfig;
            }

            try
            {
                if (!File.Exists(CONFIG_FILE))
                {
                    throw new FileNotFoundException("Конфигурационный файл не найден", CONFIG_FILE);
                }

                var jsonContent = await File.ReadAllTextAsync(CONFIG_FILE);
                _cachedConfig = JsonSerializer.Deserialize<Config>(jsonContent);

                if (!_cachedConfig.IsValid())
                {
                    throw new InvalidOperationException("Неверный формат конфигурационного файла");
                }

                _logger.LogInfo("Конфигурация успешно загружена");
                return _cachedConfig;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка загрузки конфигурации: {ex.Message}");
                throw;
            }
        }
    }
}
