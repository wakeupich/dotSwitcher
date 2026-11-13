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
using System.Security.Cryptography;
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
        [DataMember(Name = "digest")]
        public string Digest { get; set; }
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
        public string InstallerDigest { get; set; }
        public string ExeDigest { get; set; }
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
                EnsureSecureTls();

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
                            result.InstallerDigest = asset.Digest;
                        }
                        else if (string.Equals(name, "dotSwitcher.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            result.ExeUrl = url;
                            result.ExeSize = size;
                            result.ExeDigest = asset.Digest;
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
                EnsureSecureTls();
                string currentExe = Assembly.GetExecutingAssembly().Location;
                string currentDir = Path.GetDirectoryName(currentExe);
                bool preferSetup = IsInstalledInCurrentDir(currentDir) && !string.IsNullOrEmpty(update.InstallerUrl);

                if (preferSetup)
                {
                    string tempSetup = Path.Combine(Path.GetTempPath(), string.Format("dotSwitcher-Setup-{0:N}.exe", Guid.NewGuid()));
                    if (!DownloadBinary(update.InstallerUrl, update.InstallerSize, tempSetup, update.InstallerDigest, out error))
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
                    if (!DownloadBinary(update.ExeUrl, update.ExeSize, tempExe, update.ExeDigest, out error))
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

        private static bool DownloadBinary(string url, long expectedSize, string destPath, string expectedSha256, out string error)
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

                if (!IsValidPeFile(destPath))
                {
                    error = "Downloaded file is not a valid Windows executable.";
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(expectedSha256))
                {
                    string actual = ComputeSha256(destPath);
                    if (!string.Equals(actual, NormalizeSha256(expectedSha256), StringComparison.OrdinalIgnoreCase))
                    {
                        error = "Downloaded file checksum mismatch.";
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

        private static void EnsureSecureTls()
        {
            const SecurityProtocolType insecure = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
            ServicePointManager.SecurityProtocol = (ServicePointManager.SecurityProtocol & ~insecure) | SecurityProtocolType.Tls12;
        }

        private static bool IsValidPeFile(string path)
        {
            try
            {
                using (var fs = File.OpenRead(path))
                {
                    if (fs.Length < 64) return false;
                    byte[] mz = new byte[2];
                    if (fs.Read(mz, 0, 2) < 2 || mz[0] != (byte)'M' || mz[1] != (byte)'Z') return false;
                    fs.Seek(0x3C, SeekOrigin.Begin);
                    byte[] off = new byte[4];
                    if (fs.Read(off, 0, 4) < 4) return false;
                    int peOffset = BitConverter.ToInt32(off, 0);
                    if (peOffset <= 0 || peOffset > fs.Length - 4) return false;
                    fs.Seek(peOffset, SeekOrigin.Begin);
                    byte[] sig = new byte[4];
                    if (fs.Read(sig, 0, 4) < 4) return false;
                    return sig[0] == (byte)'P' && sig[1] == (byte)'E' && sig[2] == 0 && sig[3] == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private static string ComputeSha256(string path)
        {
            using (var sha = SHA256.Create())
            using (var fs = File.OpenRead(path))
            {
                byte[] hash = sha.ComputeHash(fs);
                var sb = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        private static string NormalizeSha256(string digest)
        {
            if (string.IsNullOrWhiteSpace(digest)) return null;
            string d = digest.Trim();
            const string prefix = "sha256:";
            if (d.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                d = d.Substring(prefix.Length);
            }
            return d;
        }
    }
}
