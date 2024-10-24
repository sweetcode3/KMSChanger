namespace KMSChanger.Models
{
    public class WindowsActivationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string ErrorDetails { get; set; }
        public WindowsInfo WindowsInfo { get; set; }
        public string UsedKmsServer { get; set; }
        public string AppliedProductKey { get; set; }

        public WindowsActivationResult(bool success, string message, WindowsInfo info = null, string error = null)
        {
            IsSuccess = success;
            Message = message;
            WindowsInfo = info;
            ErrorDetails = error;
        }

        public override string ToString()
        {
            var result = $"Результат активации: {(IsSuccess ? "Успешно" : "Ошибка")}\n" +
                        $"Сообщение: {Message}\n";

            if (WindowsInfo != null)
            {
                result += $"\nИнформация о системе:\n{WindowsInfo}";
            }

            if (!string.IsNullOrEmpty(UsedKmsServer))
            {
                result += $"\nИспользованный KMS сервер: {UsedKmsServer}";
            }

            if (!string.IsNullOrEmpty(AppliedProductKey))
            {
                result += $"\nПримененный ключ продукта: {AppliedProductKey}";
            }

            if (!string.IsNullOrEmpty(ErrorDetails))
            {
                result += $"\n\nДетали ошибки:\n{ErrorDetails}";
            }

            return result;
        }
    }
}
