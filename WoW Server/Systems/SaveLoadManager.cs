using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.IO.Compression;

namespace WoW_Server
{
    public static class SaveLoadManager
    {
        private const string SaveFilePath = "WoW_Server_Settings.json";

        public static string MysqlExeName { get; set; }
        public static string MysqlExePath { get; set; }
        public static bool MysqlAutoRestart { get; set; }
        public static string AuthExeName { get; set; }
        public static string AuthExePath { get; set; }
        public static bool AuthAutoRestart { get; set; }
        public static string WorldExeName { get; set; }
        public static string WorldExePath { get; set; }
        public static bool WorldAutoRestart { get; set; }
        public static double WindowWidth { get; set; } = 800;
        public static double WindowHeight { get; set; } = 600;
        public static int MysqlKillWaitTime { get; set; } = 10;
        public static int AuthKillWaitTime { get; set; } = 10;
        public static int WorldKillWaitTime { get; set; } = 15;
        public static string WorldCommand1 { get; set; } = "save";
        public static string WorldCommand2 { get; set; } = "announce Server has been forced to shutdown.";
        public static string WorldCommand3 { get; set; } = "server shutdown 10";
        public static List<string> LogFiles { get; set; } = new List<string>();

        public static void SaveSettings()
        {
            var settings = new
            {
                WindowWidth,
                WindowHeight,
                MysqlExeName,
                MysqlExePath,
                MysqlKillWaitTime,
                MysqlAutoRestart,
                AuthExeName,
                AuthExePath,
                AuthKillWaitTime,
                AuthAutoRestart,
                WorldExeName,
                WorldExePath,
                WorldKillWaitTime,
                WorldAutoRestart,
                WorldCommand1,
                WorldCommand2,
                WorldCommand3,
                LogFiles
            };

            try
            {
                File.WriteAllText(SaveFilePath, JsonConvert.SerializeObject(settings, Formatting.Indented));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving settings: {ex.Message}");
            }
        }

        public static void LoadSettings()
        {
            if (File.Exists(SaveFilePath))
            {
                try
                {
                    var settings = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(SaveFilePath));

                    WindowWidth = settings.WindowWidth;
                    WindowHeight = settings.WindowHeight;
                    MysqlExeName = settings.MysqlExeName;
                    MysqlExePath = settings.MysqlExePath;
                    MysqlKillWaitTime = settings.MysqlKillWaitTime;
                    MysqlAutoRestart = settings.MysqlAutoRestart;
                    AuthExeName = settings.AuthExeName;
                    AuthExePath = settings.AuthExePath;
                    AuthKillWaitTime = settings.AuthKillWaitTime;
                    AuthAutoRestart = settings.AuthAutoRestart;
                    WorldExeName = settings.WorldExeName;
                    WorldExePath = settings.WorldExePath;
                    WorldKillWaitTime = settings.WorldKillWaitTime;
                    WorldAutoRestart = settings.WorldAutoRestart;
                    WorldCommand1 = settings.WorldCommand1;
                    WorldCommand2 = settings.WorldCommand2;
                    WorldCommand3 = settings.WorldCommand3;
                    LogFiles = settings.LogFiles.ToObject<List<string>>();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error loading settings: {ex.Message}");
                }
            }
        }

        public static void AddLogFile(string filePath)
        {
            if (!LogFiles.Contains(filePath))
            {
                LogFiles.Add(filePath);
                SaveSettings();
            }
        }

        public static void RemoveLogFile(string filePath)
        {
            if (LogFiles.Contains(filePath))
            {
                LogFiles.Remove(filePath);
                SaveSettings();
            }
        }

        public static void ZipLogFiles()
        {
            if (LogFiles.Count == 0)
            {
                return; // No logs to zip
            }

            try
            {
                string logsDirectory = "Logs";
                if (!Directory.Exists(logsDirectory))
                {
                    Directory.CreateDirectory(logsDirectory);
                }

                string zipFileName = Path.Combine(logsDirectory, $"Logs_{DateTime.Now:yyyy_MM_dd_HHmmss}.zip");
                using (ZipArchive archive = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
                {
                    foreach (string logFile in LogFiles)
                    {
                        if (File.Exists(logFile))
                        {
                            archive.CreateEntryFromFile(logFile, Path.GetFileName(logFile));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error creating log zip file: {ex.Message}");
            }
        }
    }
}
