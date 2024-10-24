public class SystemInfoService
{
    public async Task<SystemInfo> GetSystemInfoAsync()
    {
        var info = new SystemInfo();
        
        using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
        using (var results = searcher.Get())
        {
            var os = results.Cast<ManagementObject>().First();
            info.OSVersion = os["Version"].ToString();
            info.OSName = os["Caption"].ToString();
            info.Architecture = os["OSArchitecture"].ToString();
        }

        info.ComputerName = Environment.MachineName;
        info.DomainName = Environment.UserDomainName;
        info.UserName = Environment.UserName;

        return info;
    }
}
