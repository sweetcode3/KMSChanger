namespace KMSChanger.Models
{
    public class WindowsInfo
    {
        public string Edition { get; set; }
        public string Version { get; set; }
        public string Architecture { get; set; }
        public string CurrentProductKey { get; set; }
        public string ActivationStatus { get; set; }
        public DateTime InstallDate { get; set; }
        public string RegisteredOwner { get; set; }
        public string RegisteredOrganization { get; set; }

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
