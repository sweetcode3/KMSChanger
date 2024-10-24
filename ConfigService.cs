public class ConfigService
{
    private readonly string _configPath;
    private Config _cachedConfig;

    public ConfigService(string configPath = "config.json")
    {
        _configPath = configPath;
    }

    public async Task<Config> GetConfigAsync()
    {
        if (_cachedConfig != null)
            return _cachedConfig;

        var json = await File.ReadAllTextAsync(_configPath);
        _cachedConfig = JsonSerializer.Deserialize<Config>(json);
        ValidateConfig(_cachedConfig);
        return _cachedConfig;
    }

    private void ValidateConfig(Config config)
    {
        if (config.ProductKeys == null || !config.ProductKeys.Any())
            throw new InvalidOperationException("Отсутствуют ключи продуктов в конфигурации");

        if (config.KmsServers == null || !config.KmsServers.Any())
            throw new InvalidOperationException("Отсутствуют KMS серверы в конфигурации");
    }
}
