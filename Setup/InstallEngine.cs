using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;

namespace dotSwitcher.Setup
{
    public class InstallOptions
    {
        public string InstallDir { get; set; }
        public bool AutoStart { get; set; }
        public bool RunAsAdmin { get; set; }
        public bool CreateStartMenuShortcut { get; set; }
        public bool CreateDesktopShortcut { get; set; }
        public bool LaunchAfterInstall { get; set; }
        public bool Silent { get; set; }
        public bool CleanupSelfAfterExit { get; set; }
        public bool MarkAsUpdated { get; set; }
    }

    public static class InstallEngine
    {
        private const string UninstallKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\dotSwitcher";
        private const string TaskName = "dotSwitcher";
        private const string PayloadResourceName = "dotSwitcher.exe";

        public static string DefaultInstallDir
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "dotSwitcher");
            }
        }

        public static string SettingsDir
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "dotSwitcher");
            }
        }

        public static string SettingsFilePath
        {
            get { return Path.Combine(SettingsDir, "settings.xml"); }
        }

        public static string GetInstalledLocation()
        {
            // 1. Check Uninstall registry entry
            try
            {
                foreach (var hive in new[] { RegistryHive.LocalMachine, RegistryHive.CurrentUser })
                {
                    using (var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
                    using (var key = baseKey.OpenSubKey(UninstallKeyPath))
                    {
                        if (key == null) continue;
                        var loc = key.GetValue("InstallLocation") as string;
                        if (!string.IsNullOrWhiteSpace(loc))
                        {
                            string cleanLoc = loc.Trim().Trim('"');
                            if (Directory.Exists(cleanLoc) || File.Exists(Path.Combine(cleanLoc, "dotSwitcher.exe")))
                            {
                                return cleanLoc;
                            }
                        }
                    }
                }
            }
            catch { }

            // 2. Check Task Scheduler ("dotSwitcher")
            string taskExe = GetTaskSchedulerExePath();
            if (!string.IsNullOrWhiteSpace(taskExe))
            {
                string dir = Path.GetDirectoryName(taskExe);
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    return dir;
                }
            }

            // 3. Check Startup folder shortcut ("dotSwitcher.lnk")
            string startupExe = GetStartupShortcutTargetPath();
            if (!string.IsNullOrWhiteSpace(startupExe))
            {
                string dir = Path.GetDirectoryName(startupExe);
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    return dir;
                }
            }

            // 4. Check currently running dotSwitcher process
            string runningExe = GetRunningDotSwitcherPath();
            if (!string.IsNullOrWhiteSpace(runningExe))
            {
                string dir = Path.GetDirectoryName(runningExe);
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    return dir;
                }
            }

            return null;
        }

        private static string GetTaskSchedulerExePath()
        {
            try
            {
                var psi = new ProcessStartInfo("schtasks.exe", string.Format("/Query /TN \"{0}\" /XML", TaskName))
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using (var proc = Process.Start(psi))
                {
                    if (proc == null) return null;
                    string xml = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit(3000);
                    if (proc.ExitCode != 0 || string.IsNullOrWhiteSpace(xml))
                    {
                        return null;
                    }

                    var doc = XDocument.Parse(xml);
                    foreach (var el in doc.Descendants())
                    {
                        if (string.Equals(el.Name.LocalName, "Command", StringComparison.OrdinalIgnoreCase))
                        {
                            string cmd = (el.Value ?? "").Trim().Trim('"');
                            if (cmd.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && File.Exists(cmd))
                            {
                                return cmd;
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        private static string GetStartupShortcutTargetPath()
        {
            try
            {
                string lnkPath = GetStartupShortcutPath();
                if (!File.Exists(lnkPath)) return null;

                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return null;
                dynamic shell = Activator.CreateInstance(shellType);
                dynamic shortcut = shell.CreateShortcut(lnkPath);
                string target = (shortcut.TargetPath as string ?? "").Trim().Trim('"');
                if (!string.IsNullOrWhiteSpace(target) && File.Exists(target))
                {
                    return target;
                }
            }
            catch { }
            return null;
        }

        private static string GetRunningDotSwitcherPath()
        {
            int selfPid = Process.GetCurrentProcess().Id;
            foreach (var proc in Process.GetProcessesByName("dotSwitcher"))
            {
                try
                {
                    if (proc.Id == selfPid || proc.HasExited) continue;
                    string path = proc.MainModule != null ? proc.MainModule.FileName : null;
                    if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                    {
                        return path;
                    }
                }
                catch { }
                finally
                {
                    proc.Dispose();
                }
            }
            return null;
        }

        public static bool IsAlreadyInstalled(string targetDir)
        {
            if (!string.IsNullOrWhiteSpace(GetInstalledLocation()))
            {
                return true;
            }
            return !string.IsNullOrWhiteSpace(targetDir) && File.Exists(Path.Combine(targetDir, "dotSwitcher.exe"));
        }

        public static string GetSetupVersion()
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            var fvi = FileVersionInfo.GetVersionInfo(exePath);
            if (!string.IsNullOrEmpty(fvi.FileVersion))
            {
                Version v;
                if (Version.TryParse(fvi.FileVersion, out v))
                {
                    return string.Format("{0}.{1}.{2}", v.Major, v.Minor, Math.Max(0, v.Build));
                }
            }
            return "1.3.0";
        }

        public static void LoadExistingSettingsFlags(out bool autoStart, out bool runAsAdmin)
        {
            bool hasTask = !string.IsNullOrWhiteSpace(GetTaskSchedulerExePath());
            bool hasStartupShortcut = !string.IsNullOrWhiteSpace(GetStartupShortcutTargetPath());

            autoStart = hasTask || hasStartupShortcut || !File.Exists(SettingsFilePath);
            runAsAdmin = hasTask || !File.Exists(SettingsFilePath);

            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var doc = XDocument.Load(SettingsFilePath);
                    var root = doc.Root;
                    if (root != null)
                    {
                        var autoEl = root.Element("AutoStart");
                        if (autoEl != null && !string.IsNullOrWhiteSpace(autoEl.Value))
                        {
                            bool b;
                            if (bool.TryParse(autoEl.Value.Trim(), out b)) autoStart = b || hasTask || hasStartupShortcut;
                        }
                        var adminEl = root.Element("RunAsAdmin");
                        if (adminEl != null && !string.IsNullOrWhiteSpace(adminEl.Value))
                        {
                            bool b;
                            if (bool.TryParse(adminEl.Value.Trim(), out b)) runAsAdmin = b || hasTask;
                        }
                    }
                }
            }
            catch { }
        }

        public static void Install(InstallOptions opts)
        {
            if (opts == null || string.IsNullOrWhiteSpace(opts.InstallDir))
            {
                throw new ArgumentException("Install directory cannot be empty.");
            }

            string installDir = Path.GetFullPath(opts.InstallDir.Trim());
            string exeDest = Path.Combine(installDir, "dotSwitcher.exe");
            string uninstallDest = Path.Combine(installDir, "uninstall.exe");
            string selfExe = Assembly.GetExecutingAssembly().Location;

            bool wasRunning = StopRunningDotSwitcher();

            Directory.CreateDirectory(installDir);

            // Extract embedded dotSwitcher.exe (overwrite existing instance)
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(PayloadResourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("Embedded dotSwitcher.exe resource not found in installer.");
                }
                CopyStreamWithRetries(stream, exeDest);
            }

            // Copy installer as uninstall.exe (unless running directly from uninstall.exe)
            if (!string.Equals(Path.GetFullPath(selfExe), Path.GetFullPath(uninstallDest), StringComparison.OrdinalIgnoreCase))
            {
                CopyFileWithRetries(selfExe, uninstallDest);
            }

            // Update settings.xml startup flags without wiping existing user settings
            UpdateStartupSettingsXml(opts.AutoStart, opts.RunAsAdmin);

            // Configure autorun / Task Scheduler pointing to the installed exe
            ConfigureAutorun(exeDest, opts.AutoStart, opts.RunAsAdmin);

            // Shortcuts
            if (opts.CreateStartMenuShortcut)
            {
                string startMenuDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms),
                    "dotSwitcher");
                Directory.CreateDirectory(startMenuDir);
                CreateShortcut(Path.Combine(startMenuDir, "dotSwitcher.lnk"), exeDest, installDir, "dotSwitcher keyboard layout switcher");
                CreateShortcut(Path.Combine(startMenuDir, "Uninstall dotSwitcher.lnk"), uninstallDest, installDir, "Uninstall dotSwitcher", "/uninstall");
            }

            if (opts.CreateDesktopShortcut)
            {
                string desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                CreateShortcut(Path.Combine(desktopDir, "dotSwitcher.lnk"), exeDest, installDir, "dotSwitcher keyboard layout switcher");
            }

            // Register in Windows Apps & Features (Programs and Features)
            RegisterUninstallEntry(installDir, exeDest, uninstallDest);

            if (opts.LaunchAfterInstall || wasRunning)
            {
                string launchArgs = opts.MarkAsUpdated ? "--updated" : "";
                var psi = new ProcessStartInfo(exeDest, launchArgs)
                {
                    UseShellExecute = true,
                    WorkingDirectory = installDir
                };
                Process.Start(psi);
            }

            if (opts.CleanupSelfAfterExit &&
                !string.Equals(Path.GetFullPath(selfExe), Path.GetFullPath(uninstallDest), StringComparison.OrdinalIgnoreCase))
            {
                ScheduleDeleteSelf(selfExe, false);
            }
        }

        public static void Uninstall(bool deleteUserSettings)
        {
            string installedDir = GetInstalledLocation();
            string selfExe = Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrWhiteSpace(installedDir))
            {
                installedDir = Path.GetDirectoryName(selfExe);
            }

            StopRunningDotSwitcher();

            // Remove Task Scheduler task & Startup shortcut
            RunSchtasks(string.Format("/Delete /TN \"{0}\" /F", TaskName));
            string startupShortcut = GetStartupShortcutPath();
            TryDeleteFile(startupShortcut);

            // Remove Start Menu & Desktop shortcuts
            try
            {
                string startMenuDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms),
                    "dotSwitcher");
                if (Directory.Exists(startMenuDir))
                {
                    Directory.Delete(startMenuDir, true);
                }
            }
            catch { }

            TryDeleteFile(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                "dotSwitcher.lnk"));

            // Remove Uninstall registry entries
            try
            {
                foreach (var hive in new[] { RegistryHive.LocalMachine, RegistryHive.CurrentUser })
                {
                    using (var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
                    {
                        baseKey.DeleteSubKeyTree(UninstallKeyPath, false);
                    }
                }
            }
            catch { }

            if (!string.IsNullOrWhiteSpace(installedDir))
            {
                TryDeleteFile(Path.Combine(installedDir, "dotSwitcher.exe"));
            }

            if (deleteUserSettings && Directory.Exists(SettingsDir))
            {
                try { Directory.Delete(SettingsDir, true); } catch { }
            }

            ScheduleDeleteSelf(selfExe, true, installedDir);
        }

        private static bool StopRunningDotSwitcher()
        {
            bool anyRunning = false;
            int selfPid = Process.GetCurrentProcess().Id;
            foreach (var proc in Process.GetProcessesByName("dotSwitcher"))
            {
                try
                {
                    if (proc.Id == selfPid) continue;
                    if (!proc.HasExited)
                    {
                        anyRunning = true;
                        proc.CloseMainWindow();
                        if (!proc.WaitForExit(1500))
                        {
                            proc.Kill();
                            proc.WaitForExit(3000);
                        }
                    }
                }
                catch { }
                finally
                {
                    proc.Dispose();
                }
            }
            if (anyRunning)
            {
                Thread.Sleep(250);
            }
            return anyRunning;
        }

        private static void UpdateStartupSettingsXml(bool autoStart, bool runAsAdmin)
        {
            try
            {
                Directory.CreateDirectory(SettingsDir);
                if (!File.Exists(SettingsFilePath))
                {
                    // Let dotSwitcher migrate legacy user.config on first run, then write just these overrides only if already exists
                    return;
                }

                var doc = XDocument.Load(SettingsFilePath);
                var root = doc.Root;
                if (root == null) return;

                SetElement(root, "AutoStart", autoStart ? "true" : "false");
                SetElement(root, "RunAsAdmin", runAsAdmin ? "true" : "false");

                string temp = SettingsFilePath + ".tmp";
                doc.Save(temp);
                File.Copy(temp, SettingsFilePath, true);
                File.Delete(temp);
            }
            catch { }
        }

        private static void SetElement(XElement root, string name, string value)
        {
            var el = root.Element(name);
            if (el == null)
            {
                root.Add(new XElement(name, value));
            }
            else
            {
                el.Value = value;
            }
        }

        private static void ConfigureAutorun(string exePath, bool autoStart, bool runAsAdmin)
        {
            string shortcutPath = GetStartupShortcutPath();
            if (!autoStart)
            {
                TryDeleteFile(shortcutPath);
                RunSchtasks(string.Format("/Delete /TN \"{0}\" /F", TaskName));
                return;
            }

            if (runAsAdmin)
            {
                TryDeleteFile(shortcutPath);
                var psi = new ProcessStartInfo(exePath, "--create-task")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var proc = Process.Start(psi))
                {
                    if (proc != null) proc.WaitForExit(5000);
                }
            }
            else
            {
                RunSchtasks(string.Format("/Delete /TN \"{0}\" /F", TaskName));
                CreateShortcut(shortcutPath, exePath, Path.GetDirectoryName(exePath), "Simple keyboard layout switcher");
            }
        }

        private static string GetStartupShortcutPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                "dotSwitcher.lnk");
        }

        private static void CreateShortcut(string shortcutPath, string targetPath, string workingDir, string description, string arguments = "")
        {
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) return;
            dynamic shell = Activator.CreateInstance(shellType);
            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            shortcut.Description = description;
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = workingDir;
            if (!string.IsNullOrEmpty(arguments))
            {
                shortcut.Arguments = arguments;
            }
            shortcut.Save();
        }

        private static void RegisterUninstallEntry(string installDir, string exePath, string uninstallExePath)
        {
            long kbSize = 250;
            try
            {
                long bytes = 0;
                if (File.Exists(exePath)) bytes += new FileInfo(exePath).Length;
                if (File.Exists(uninstallExePath)) bytes += new FileInfo(uninstallExePath).Length;
                kbSize = Math.Max(1, bytes / 1024);
            }
            catch { }

            using (var key = Registry.LocalMachine.CreateSubKey(UninstallKeyPath))
            {
                if (key == null) return;
                key.SetValue("DisplayName", "dotSwitcher");
                key.SetValue("DisplayVersion", GetSetupVersion());
                key.SetValue("Publisher", "wakeupich");
                key.SetValue("URLInfoAbout", "https://github.com/wakeupich/dotSwitcher");
                key.SetValue("DisplayIcon", exePath);
                key.SetValue("InstallLocation", installDir);
                key.SetValue("UninstallString", string.Format("\"{0}\" /uninstall", uninstallExePath));
                key.SetValue("QuietUninstallString", string.Format("\"{0}\" /uninstall /silent", uninstallExePath));
                key.SetValue("EstimatedSize", (int)kbSize, RegistryValueKind.DWord);
                key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
            }
        }

        private static void RunSchtasks(string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo("schtasks.exe", arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using (var proc = Process.Start(psi))
                {
                    if (proc != null) proc.WaitForExit(3000);
                }
            }
            catch { }
        }

        private static void CopyStreamWithRetries(Stream src, string destPath)
        {
            for (int i = 0; i < 15; i++)
            {
                try
                {
                    src.Position = 0;
                    using (var fs = File.Create(destPath))
                    {
                        src.CopyTo(fs);
                    }
                    return;
                }
                catch
                {
                    if (i == 14) throw;
                    Thread.Sleep(250);
                }
            }
        }

        private static void CopyFileWithRetries(string srcPath, string destPath)
        {
            for (int i = 0; i < 15; i++)
            {
                try
                {
                    File.Copy(srcPath, destPath, true);
                    return;
                }
                catch
                {
                    if (i == 14) throw;
                    Thread.Sleep(250);
                }
            }
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch { }
        }

        private static void ScheduleDeleteSelf(string selfExePath, bool removeInstallDir, string installDir = null)
        {
            try
            {
                string cmd = string.Format("/c ping 127.0.0.1 -n 3 >nul & del /f /q \"{0}\"", selfExePath);
                if (removeInstallDir && !string.IsNullOrWhiteSpace(installDir))
                {
                    cmd += string.Format(" & del /f /q \"{0}\" & rmdir \"{1}\"",
                        Path.Combine(installDir, "uninstall.exe"),
                        installDir);
                }

                var psi = new ProcessStartInfo("cmd.exe", cmd)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(psi);
            }
            catch { }
        }
    }
}
