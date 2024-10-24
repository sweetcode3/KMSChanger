using System;
using System.Diagnostics;
using System.Text.Json;
using System.Management;
using System.Net.NetworkInformation;
using KMSChanger.Services;
using KMSChanger.Models;

namespace KMSChanger
{
    public class Program
    {
        private static readonly ILogger _logger;
        private static readonly WindowsActivationService _activationService;
        private static readonly ConfigurationService _configService;

        static Program()
        {
            _logger = new FileLogger();
            _configService = new ConfigurationService(_logger);
            _activationService = new WindowsActivationService(_logger, _configService);
        }

        [STAThread]
        static async Task Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                if (!IsRunAsAdministrator())
                {
                    RestartAsAdmin();
                    return;
                }

                await ProcessActivation();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Критическая ошибка: {ex}");
                ShowError("Произошла критическая ошибка. Проверьте лог-файл для деталей.");
            }
        }

        private static async Task ProcessActivation()
        {
            using (var loadingForm = new LoadingForm())
            {
                loadingForm.Show();
                Application.DoEvents();

                var result = await _activationService.ActivateWindowsAsync();

                loadingForm.Close();

                if (result.IsSuccess)
                {
                    ShowActivationSuccess(result);
                }
                else
                {
                    ShowError(result.Message, result.ErrorDetails);
                }
            }
        }

        private static bool IsRunAsAdministrator()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        private static void RestartAsAdmin()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Application.ExecutablePath,
                    Verb = "runas"
                };

                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка запуска с правами администратора: {ex}");
                ShowError("Для работы программы требуются права администратора.");
            }
            Application.Exit();
        }

        private static void ShowActivationSuccess(WindowsActivationResult result)
        {
            var message = $"Windows успешно активирована\n\n" +
                         $"Версия: {result.WindowsInfo.Edition}\n" +
                         $"Сборка: {result.WindowsInfo.Version}\n" +
                         $"Статус: Активирована";

            MessageBox.Show(
                message,
                "Активация успешна",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private static void ShowError(string message, string details = null)
        {
            var errorMessage = message;
            if (!string.IsNullOrEmpty(details))
            {
                errorMessage += $"\n\nДетали ошибки:\n{details}";
            }

            MessageBox.Show(
                errorMessage,
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    public class LoadingForm : Form
    {
        public LoadingForm()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(200, 70);
            this.ShowInTaskbar = false;

            var label = new Label
            {
                Text = "Выполняется активация...",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            this.Controls.Add(label);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ClassStyle |= 0x20000; // CS_DROPSHADOW
                return cp;
            }
        }
    }
}
