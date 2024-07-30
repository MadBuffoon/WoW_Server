using System.Diagnostics;

namespace WoW_Server
{
    public static class ProcessStarter
    {
        public static void StartProcess(string executablePath, string arguments = "")
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            Process.Start(startInfo);
        }
    }
}