namespace KMSChanger.Models
{
    public class WindowsInfo
    {
        public string Edition { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Architecture { get; set; } = string.Empty;
        public string CurrentProductKey { get; set; } = string.Empty;
        public string ActivationStatus { get; set; } = string.Empty;
        public string RegisteredOwner { get; set; } = string.Empty;
        public string RegisteredOrganization { get; set; } = string.Empty;
        public DateTime InstallDate { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $"Редакция: {Edition}\n" +
                   $"Версия: {Version}\n" +
                   $"Архитектура: {Architecture}\n" +
                   $"Текущий ключ: {CurrentProductKey}\n" +
                   $"Статус активации: {ActivationStatus}\n" +
                   $"Дата установки: {InstallDate:d}\n" +
                   $"Владелец: {RegisteredOwner}\n" +
                   $"Организация: {RegisteredOrganization}";
        }
    }
}
