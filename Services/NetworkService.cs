namespace KMSChanger.Services
{
    public class NetworkService
    {
        private readonly ILogger _logger;
        
        public NetworkService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<bool> CheckKmsServerConnection(string server, int timeout = 3000)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(server, timeout);
                var isSuccess = reply.Status == IPStatus.Success;
                
                _logger.LogInfo($"Проверка соединения с {server}: {(isSuccess ? "успешно" : "неудачно")}");
                return isSuccess;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка проверки соединения с {server}: {ex.Message}");
                return false;
            }
        }

        public async Task<string?> FindWorkingKmsServer(List<string> servers)
        {
             foreach (var server in servers)
             {
                   if (await CheckKmsServerConnection(server))
                   {
                        return server;
                   }
             }
             return null;
        }

    }
}
