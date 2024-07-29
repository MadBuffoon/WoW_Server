using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WoW_Server
{
    public static class ProcessKiller
    {
        public static async Task KillProcessGracefully(string processName, int waitTimeInSeconds, string[] commands = null)
        {
            if (string.IsNullOrWhiteSpace(processName))
            {
                return;
            }

            try
            {
                var processes = Process.GetProcessesByName(processName);
                foreach (var process in processes)
                {
                    if (!process.HasExited)
                    {
                        if (commands != null && commands.Length > 0)
                        {
                            foreach (var command in commands)
                            {
                                await SendCommandToProcess(process, command);
                            }
                        }

                        await Task.Delay(waitTimeInSeconds * 1000);

                        if (!process.HasExited)
                        {
                            process.Kill();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping process {processName}: {ex.Message}");
            }
        }

        private static async Task SendCommandToProcess(Process process, string command)
        {
            try
            {
                using (StreamWriter writer = process.StandardInput)
                {
                    if (writer.BaseStream.CanWrite)
                    {
                        await writer.WriteLineAsync(command);
                        await writer.FlushAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending command to process {process.ProcessName}: {ex.Message}");
            }
        }
    }
}
