using System.Windows.Forms;

namespace KMSChanger
{
    public class Program
{
    private static readonly ILogger _logger;
    private static readonly ConfigurationService _configService;
    private static readonly SystemInfoService _systemInfoService;
    private static readonly NetworkService _networkService;
    private static readonly WindowsActivationService _activationService;

    static Program()
    {
        _logger = new FileLogger();
        _configService = new ConfigurationService(_logger);
        _systemInfoService = new SystemInfoService(_logger);
        _networkService = new NetworkService(_logger);
        _activationService = new WindowsActivationService(
            _logger, 
            _configService, 
            _systemInfoService, 
            _networkService
        );
   
        [STAThread]
        static async Task Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            InitializeServices();

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

        private static void InitializeServices()
        {
            _logger = new FileLogger();
            _configService = new ConfigurationService(_logger);
            _systemInfoService = new SystemInfoService(_logger);
            _networkService = new NetworkService(_logger);
            _activationService = new WindowsActivationService(_logger, _configService, _systemInfoService, _networkService);
        }

        private static async Task ProcessActivation()
        {
            using var loadingForm = new LoadingForm();
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
            var message = $"Windows успешно активирована\n\n{result.WindowsInfo}";
            if (!string.IsNullOrEmpty(result.UsedKmsServer))
            {
                message += $"\n\nИспользованный KMS сервер: {result.UsedKmsServer}";
            }

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
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(300, 100);
            this.ShowInTaskbar = false;

            var label = new Label
            {
                Text = "Выполняется активация Windows...",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Font = new Font(Font.FontFamily, 10)
            };

            var progress = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Dock = DockStyle.Bottom,
                Height = 20
            };

            this.Controls.AddRange(new Control[] { label, progress });
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ClassStyle |= 0x20000;
                return cp;
            }
        }
    }
}
