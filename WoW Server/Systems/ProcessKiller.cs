using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WoW_Server
{
    public static class ProcessKiller
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static async Task KillProcessGracefully(string processName, int waitTimeInSeconds, string[] commands = null)
        {
            var processes = Process.GetProcessesByName(processName).ToList();

            foreach (var process in processes)
            {
                if (commands != null && commands.Length > 0)
                {
                    try
                    {
                        IntPtr hWnd = FindWindow(null, process.MainWindowTitle);
                        if (hWnd != IntPtr.Zero)
                        {
                            SetForegroundWindow(hWnd);

                            foreach (var command in commands)
                            {
                                SendKeys.SendWait(command + "{ENTER}");
                                await Task.Delay(500); // Small delay between commands
                            }

                            await Task.Delay(waitTimeInSeconds * 1000);
                            if (!process.HasExited)
                            {
                                process.Kill();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending commands to process: {ex.Message}");
                    }
                }
                else
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error killing process: {ex.Message}");
                    }
                }
            }
        }
    }
}
