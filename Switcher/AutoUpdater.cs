/**
 *   DotSwitcher: a simple keyboard layout switcher
 *   Copyright (C) 2014-2019 Kirill Mokhovtsev / kurumpa
 *   Contact: kiev.programmer@gmail.com
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using dotSwitcher.Data;
using dotSwitcher.WinApi;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace dotSwitcher.Switcher
{
    [DataContract]
    internal class GitHubAssetDto
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "browser_download_url")]
        public string BrowserDownloadUrl { get; set; }
        [DataMember(Name = "size")]
        public long Size { get; set; }
    }

    [DataContract]
    internal class GitHubReleaseDto
    {
        [DataMember(Name = "tag_name")]
        public string TagName { get; set; }
        [DataMember(Name = "html_url")]
        public string HtmlUrl { get; set; }
        [DataMember(Name = "assets")]
        public List<GitHubAssetDto> Assets { get; set; }
    }
    public class UpdateCheckResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public bool HasUpdate { get; set; }
        public Version CurrentVersion { get; set; }
        public Version LatestVersion { get; set; }
        public string TagName { get; set; }
        public string ReleaseUrl { get; set; }
        public string InstallerUrl { get; set; }
        public long InstallerSize { get; set; }
        public string ExeUrl { get; set; }
        public long ExeSize { get; set; }
    }

    public static class AutoUpdater
    {
        private const string LatestReleaseApiUrl = "https://api.github.com/repos/wakeupich/dotSwitcher/releases/latest";
        private const string UninstallRegistrySubKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\dotSwitcher";

        public static Version GetCurrentVersion()
        {
            try
            {
                string exePath = Assembly.GetExecutingAssembly().Location;
                var fvi = FileVersionInfo.GetVersionInfo(exePath);
                Version parsed;
                if (!string.IsNullOrEmpty(fvi.FileVersion) && Version.TryParse(fvi.FileVersion, out parsed))
                {
                    return NormalizeVersion(parsed);
                }
            }
            catch { }

            return NormalizeVersion(Assembly.GetExecutingAssembly().GetName().Version);
        }

        public static string FormatVersion(Version v)
        {
            if (v == null) return "0.0.0";
            return string.Format("{0}.{1}.{2}", v.Major, v.Minor, Math.Max(0, v.Build));
        }

        private static Version NormalizeVersion(Version v)
        {
            if (v == null) return new Version(0, 0, 0, 0);
            return new Version(
                Math.Max(0, v.Major),
                Math.Max(0, v.Minor),
                Math.Max(0, v.Build),
                Math.Max(0, v.Revision));
        }

        private static Version ParseTagVersion(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return null;
            string clean = tag.Trim();
            if (clean.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                clean = clean.Substring(1);
            }
            int dashIdx = clean.IndexOf('-');
            if (dashIdx > 0)
            {
                clean = clean.Substring(0, dashIdx);
            }
            Version v;
            if (Version.TryParse(clean, out v))
            {
                return NormalizeVersion(v);
            }
            return null;
        }

        public static UpdateCheckResult CheckForUpdate()
        {
            var currentVer = GetCurrentVersion();
            var result = new UpdateCheckResult
            {
                Success = false,
                HasUpdate = false,
                CurrentVersion = currentVer
            };

            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                var req = (HttpWebRequest)WebRequest.Create(LatestReleaseApiUrl);
                req.Method = "GET";
                req.UserAgent = "dotSwitcher/" + FormatVersion(currentVer);
                req.Accept = "application/vnd.github+json";
                req.Timeout = 10000;
                req.ReadWriteTimeout = 10000;

                string json;
                using (var resp = (HttpWebResponse)req.GetResponse())
                using (var stream = resp.GetResponseStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    json = reader.ReadToEnd();
                }

                GitHubReleaseDto release;
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    var serializer = new DataContractJsonSerializer(typeof(GitHubReleaseDto));
                    release = serializer.ReadObject(ms) as GitHubReleaseDto;
                }

                if (release == null)
                {
                    result.ErrorMessage = "Empty response from GitHub API.";
                    return result;
                }

                string tagName = release.TagName ?? "";
                string htmlUrl = release.HtmlUrl ?? "";
                var latestVer = ParseTagVersion(tagName);
                if (latestVer == null)
                {
                    result.ErrorMessage = "Cannot parse release version: " + tagName;
                    return result;
                }

                result.TagName = tagName;
                result.ReleaseUrl = htmlUrl;
                result.LatestVersion = latestVer;

                if (release.Assets != null)
                {
                    foreach (var asset in release.Assets)
                    {
                        if (asset == null) continue;
                        string name = asset.Name ?? "";
                        string url = asset.BrowserDownloadUrl ?? "";
                        long size = asset.Size;

                        if (string.Equals(name, "dotSwitcher-Setup.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            result.InstallerUrl = url;
                            result.InstallerSize = size;
                        }
                        else if (string.Equals(name, "dotSwitcher.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            result.ExeUrl = url;
                            result.ExeSize = size;
                        }
                    }
                }

                result.HasUpdate = latestVer > currentVer;
                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        public static bool TryDownloadAndApplyUpdate(UpdateCheckResult update, out string error)
        {
            error = null;
            if (update == null || !update.Success || !update.HasUpdate)
            {
                error = "No update available.";
                return false;
            }

            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                string currentExe = Assembly.GetExecutingAssembly().Location;
                string currentDir = Path.GetDirectoryName(currentExe);
                bool preferSetup = IsInstalledInCurrentDir(currentDir) && !string.IsNullOrEmpty(update.InstallerUrl);

                if (preferSetup)
                {
                    string tempSetup = Path.Combine(Path.GetTempPath(), string.Format("dotSwitcher-Setup-{0:N}.exe", Guid.NewGuid()));
                    if (!DownloadBinary(update.InstallerUrl, update.InstallerSize, tempSetup, out error))
                    {
                        TryDeleteFile(tempSetup);
                        return false;
                    }

                    var psi = new ProcessStartInfo(tempSetup, string.Format("/silent /dir=\"{0}\" /cleanup", currentDir))
                    {
                        UseShellExecute = true
                    };
                    if (!LowLevelAdapter.IsRunningAsAdmin() && !CanWriteToDirectory(currentDir))
                    {
                        psi.Verb = "runas";
                    }

                    Process.Start(psi);
                    return true;
                }

                if (!string.IsNullOrEmpty(update.ExeUrl))
                {
                    string tempExe = Path.Combine(Path.GetTempPath(), string.Format("dotSwitcher-update-{0:N}.exe", Guid.NewGuid()));
                    if (!DownloadBinary(update.ExeUrl, update.ExeSize, tempExe, out error))
                    {
                        TryDeleteFile(tempExe);
                        return false;
                    }

                    int pid = Process.GetCurrentProcess().Id;
                    var psi = new ProcessStartInfo(tempExe, string.Format("--apply-update {0} \"{1}\"", pid, currentExe))
                    {
                        UseShellExecute = true
                    };
                    if (!LowLevelAdapter.IsRunningAsAdmin() && !CanWriteToDirectory(currentDir))
                    {
                        psi.Verb = "runas";
                    }

                    Process.Start(psi);
                    return true;
                }

                error = "Release does not contain compatible binary assets.";
                return false;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static int RunApplyUpdateMode(string[] args)
        {
            try
            {
                if (args == null || args.Length < 3)
                {
                    return 1;
                }

                int oldPid;
                if (int.TryParse(args[1], out oldPid) && oldPid > 0)
                {
                    WaitForProcessExit(oldPid, 15000);
                }

                string targetExe = args[2].Trim('"');
                string selfExe = Assembly.GetExecutingAssembly().Location;
                string targetDir = Path.GetDirectoryName(targetExe);
                if (!string.IsNullOrEmpty(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                bool copied = false;
                for (int i = 0; i < 20; i++)
                {
                    try
                    {
                        File.Copy(selfExe, targetExe, true);
                        copied = true;
                        break;
                    }
                    catch
                    {
                        Thread.Sleep(250);
                    }
                }

                if (!copied)
                {
                    return 2;
                }

                var psi = new ProcessStartInfo(targetExe, string.Format("--cleanup-update \"{0}\" --updated", selfExe))
                {
                    UseShellExecute = true,
                    WorkingDirectory = targetDir ?? ""
                };
                Process.Start(psi);
                return 0;
            }
            catch
            {
                return 3;
            }
        }

        public static void CleanupTempFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            string clean = path.Trim('"');
            ThreadPool.QueueUserWorkItem(_ =>
            {
                for (int i = 0; i < 15; i++)
                {
                    if (!File.Exists(clean)) return;
                    try
                    {
                        File.Delete(clean);
                        return;
                    }
                    catch
                    {
                        Thread.Sleep(300);
                    }
                }
            });
        }

        private static bool IsInstalledInCurrentDir(string currentDir)
        {
            if (string.IsNullOrEmpty(currentDir)) return false;
            string normCurrent = NormalizeDir(currentDir);

            try
            {
                foreach (var hive in new[] { RegistryHive.LocalMachine, RegistryHive.CurrentUser })
                {
                    using (var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
                    using (var key = baseKey.OpenSubKey(UninstallRegistrySubKey))
                    {
                        if (key == null) continue;
                        var loc = key.GetValue("InstallLocation") as string;
                        if (!string.IsNullOrEmpty(loc) &&
                            string.Equals(NormalizeDir(loc), normCurrent, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            catch { }

            return File.Exists(Path.Combine(currentDir, "uninstall.exe"));
        }

        private static string NormalizeDir(string dir)
        {
            return Path.GetFullPath(dir).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private static bool CanWriteToDirectory(string dir)
        {
            try
            {
                string testFile = Path.Combine(dir, string.Format(".write_test_{0:N}.tmp", Guid.NewGuid()));
                File.WriteAllText(testFile, "ok");
                File.Delete(testFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool DownloadBinary(string url, long expectedSize, string destPath, out string error)
        {
            error = null;
            try
            {
                using (var wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent", "dotSwitcher/" + FormatVersion(GetCurrentVersion()));
                    wc.DownloadFile(url, destPath);
                }

                var fi = new FileInfo(destPath);
                if (!fi.Exists || fi.Length < 4096)
                {
                    error = "Downloaded file is too small or missing.";
                    return false;
                }
                if (expectedSize > 0 && fi.Length != expectedSize)
                {
                    error = string.Format("Downloaded size mismatch ({0} vs expected {1}).", fi.Length, expectedSize);
                    return false;
                }

                byte[] magic = new byte[2];
                using (var fs = File.OpenRead(destPath))
                {
                    if (fs.Read(magic, 0, 2) < 2 || magic[0] != (byte)'M' || magic[1] != (byte)'Z')
                    {
                        error = "Downloaded file is not a valid Windows executable.";
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static void WaitForProcessExit(int pid, int timeoutMs)
        {
            try
            {
                using (var proc = Process.GetProcessById(pid))
                {
                    if (!proc.HasExited)
                    {
                        proc.WaitForExit(timeoutMs);
                    }
                }
            }
            catch { }
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch { }
        }

        private static string GetString(Dictionary<string, object> dict, string key)
        {
            object val;
            if (dict.TryGetValue(key, out val) && val != null)
            {
                return val.ToString();
            }
            return "";
        }

        private static long GetLong(Dictionary<string, object> dict, string key)
        {
            object val;
            if (dict.TryGetValue(key, out val) && val != null)
            {
                long res;
                if (long.TryParse(val.ToString(), out res))
                {
                    return res;
                }
            }
            return 0;
        }
    }
}
