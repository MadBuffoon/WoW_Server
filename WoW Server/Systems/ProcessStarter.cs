
using System;
using System.Diagnostics;
using System.Windows;

namespace WoW_Server
{
    public static class ProcessStarter
    {
        public static void StartProcess(string executablePath)
        {
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                MessageBox.Show("Executable path is not set.");
                return;
            }

            try
            {
                Process.Start(executablePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start process: {ex.Message}");
            }
        }
    }
}
