using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace WoW_Server
{
    public static class ProcessChecker
    {
        public static bool IsProcessRunning(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
            {
                return false;
            }

            try
            {
                foreach (var process in Process.GetProcessesByName(processName))
                {
                    if (!process.HasExited)
                    {
                        return true;
                    }
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error checking process {processName}: {ex.Message}");
            }
            return false;
        }

        public static string GetProcessUptime(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
            {
                return "N/A";
            }

            try
            {
                var process = Process.GetProcessesByName(processName).FirstOrDefault();
                if (process != null)
                {
                    var uptime = DateTime.Now - process.StartTime;
                    return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting uptime for process {processName}: {ex.Message}");
            }
            return "N/A";
        }
    }
}