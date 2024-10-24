namespace KMSChanger.Services
{
    public class ConfigurationService
    {
        private const string CONFIG_FILE = "config.json";
        private readonly ILogger _logger;

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
                    throw new FileNotFoundException("Конфигурационный файл не найден");
                }

                var jsonContent = await File.ReadAllTextAsync(CONFIG_FILE);
                var config = JsonSerializer.Deserialize<Config>(jsonContent);

                if (!ValidateConfig(config))
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

        private bool ValidateConfig(Config config)
        {
            return config?.ProductKeys?.Count > 0 && config?.KmsServers?.Count > 0;
        }
    }
}
