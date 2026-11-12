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

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Reflection;
using System.Collections.Generic;
using System.Security.Principal;
using System.Security;

namespace dotSwitcher.WinApi
{
    public static partial class LowLevelAdapter
    {
        public const string TaskName = "dotSwitcher";

        public static void AllowShowSettingsFromNonElevated()
        {
            try
            {
                ChangeWindowMessageFilter(WM_SHOW_SETTINGS, MSGFLT_ADD);
            }
            catch { }
        }

        public static IntPtr SetHook(int type, HookProc callback)
        {
            var process = Process.GetCurrentProcess();
            var module = process.MainModule;
            var handle = GetModuleHandle(module.ModuleName);
            return SetWindowsHookEx(type, callback, handle, 0);
        }

        public static void ReleaseHook(IntPtr id)
        {
            UnhookWindowsHookEx(id);
        }

        public static IntPtr NextHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        public static bool KeyPressed(Keys keyCode)
        {
            return (GetKeyState((int)keyCode) & 0x8000) == 0x8000 ||
                   (GetAsyncKeyState((int)keyCode) & 0x8000) == 0x8000;
        }

        private static IntPtr GetFocusedHandle()
        {
            var threadId = GetCurrentThreadId();
            var wndThreadId = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);

            if (threadId == wndThreadId)
            {
                return IntPtr.Zero;
            }

            AttachThreadInput(wndThreadId, threadId, true);
            IntPtr focusedHandle = GetFocus();
            AttachThreadInput(wndThreadId, threadId, false);
            return focusedHandle;
        }

        public static IntPtr GetTargetWindowHandle()
        {
            IntPtr fgWnd = GetForegroundWindow();
            IntPtr hWnd = IntPtr.Zero;
            var threadId = GetWindowThreadProcessId(fgWnd, IntPtr.Zero);
            var info = new GUITHREADINFO();
            info.cbSize = Marshal.SizeOf(info);
            var success = GetGUIThreadInfo(threadId, ref info);

            var focusedHandle = GetFocusedHandle();
            if (success)
            {
                if (info.hwndCaret != IntPtr.Zero)
                    hWnd = info.hwndCaret;
                else if (info.hwndFocus != IntPtr.Zero)
                    hWnd = info.hwndFocus;
                else if (focusedHandle != IntPtr.Zero)
                    hWnd = focusedHandle;
                else if (info.hwndActive != IntPtr.Zero)
                    hWnd = info.hwndActive;
            }
            else
            {
                hWnd = focusedHandle;
            }
            if (hWnd == IntPtr.Zero)
            {
                hWnd = fgWnd;
            }
            return hWnd;
        }

        public static IntPtr GetCurrentLayout()
        {
            var hWnd = GetTargetWindowHandle();
            var wndThreadId = GetWindowThreadProcessId(hWnd != IntPtr.Zero ? hWnd : GetForegroundWindow(), IntPtr.Zero);
            return GetKeyboardLayout(wndThreadId);
        }

        public static IntPtr[] GetInstalledLayouts()
        {
            int count = GetKeyboardLayoutList(0, null);
            if (count <= 0)
            {
                return new IntPtr[0];
            }
            var layouts = new IntPtr[count];
            GetKeyboardLayoutList(count, layouts);
            return layouts;
        }

        public static IntPtr GetNextLayout(IntPtr currentLayout)
        {
            var layouts = GetInstalledLayouts();
            if (layouts.Length == 0)
            {
                return (IntPtr)HKL_NEXT;
            }
            int idx = Array.IndexOf(layouts, currentLayout);
            if (idx < 0)
            {
                for (int i = 0; i < layouts.Length; i++)
                {
                    if (((long)layouts[i] & 0xFFFF) == ((long)currentLayout & 0xFFFF))
                    {
                        idx = i;
                        break;
                    }
                }
            }
            int nextIdx = (idx + 1) % layouts.Length;
            return layouts[nextIdx];
        }

        public static void SetKeyboardLayout(IntPtr targetHkl)
        {
            IntPtr fgWnd = GetForegroundWindow();
            IntPtr hWnd = GetTargetWindowHandle();

            if (targetHkl != IntPtr.Zero && targetHkl != (IntPtr)HKL_NEXT)
            {
                if (hWnd != IntPtr.Zero && hWnd != fgWnd)
                {
                    PostMessage(hWnd, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, targetHkl);
                }
                if (fgWnd != IntPtr.Zero)
                {
                    PostMessage(fgWnd, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, targetHkl);
                }
            }
            else
            {
                if (hWnd != IntPtr.Zero)
                {
                    PostMessage(hWnd, WM_INPUTLANGCHANGEREQUEST, (IntPtr)INPUTLANGCHANGE_FORWARD, (IntPtr)HKL_NEXT);
                }
            }
        }

        public static void SetNextKeyboardLayout()
        {
            var current = GetCurrentLayout();
            var next = GetNextLayout(current);
            SetKeyboardLayout(next);
        }

        public static IntPtr GetNextLayoutForConversion(IList<Keys> vkCodes)
        {
            var currentHkl = GetCurrentLayout();
            var layouts = GetInstalledLayouts();
            if (layouts.Length < 2 || vkCodes == null || vkCodes.Count == 0)
            {
                return GetNextLayout(currentHkl);
            }

            int currentIdx = Array.IndexOf(layouts, currentHkl);
            if (currentIdx < 0)
            {
                for (int i = 0; i < layouts.Length; i++)
                {
                    if (((long)layouts[i] & 0xFFFF) == ((long)currentHkl & 0xFFFF))
                    {
                        currentIdx = i;
                        break;
                    }
                }
            }
            if (currentIdx < 0) currentIdx = 0;

            var keyState = new byte[256];
            var buf1 = new StringBuilder(8);
            var buf2 = new StringBuilder(8);

            for (int step = 1; step < layouts.Length; step++)
            {
                IntPtr candidate = layouts[(currentIdx + step) % layouts.Length];
                bool differs = false;
                foreach (var k in vkCodes)
                {
                    uint vk = (uint)(k & Keys.KeyCode);
                    if (vk == (uint)Keys.Space || vk == (uint)Keys.Back) continue;
                    buf1.Clear();
                    buf2.Clear();
                    int r1 = ToUnicodeEx(vk, MapVirtualKeyEx(vk, 0, currentHkl), keyState, buf1, buf1.Capacity, 0, currentHkl);
                    int r2 = ToUnicodeEx(vk, MapVirtualKeyEx(vk, 0, candidate), keyState, buf2, buf2.Capacity, 0, candidate);
                    if (r1 > 0 && r2 > 0 && buf1.ToString(0, r1) != buf2.ToString(0, r2))
                    {
                        differs = true;
                        break;
                    }
                }
                if (differs)
                {
                    return candidate;
                }
            }

            return GetNextLayout(currentHkl);
        }

        public static string ConvertTextBetweenLayouts(string text, out IntPtr targetHkl)
        {
            var currentHkl = GetCurrentLayout();
            var layouts = GetInstalledLayouts();
            targetHkl = GetNextLayout(currentHkl);

            if (string.IsNullOrEmpty(text) || layouts.Length < 2)
            {
                return text;
            }

            // Detect source layout by checking which layout maps the characters best (especially non-ASCII / letters)
            IntPtr sourceHkl = currentHkl;
            int bestScore = -1;

            foreach (var hkl in layouts)
            {
                int score = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    if (char.IsWhiteSpace(c)) continue;
                    short vkScan = VkKeyScanEx(c, hkl);
                    if (vkScan != -1)
                    {
                        int shiftState = (vkScan >> 8) & 0xFF;
                        // Prefer direct or Shift mappings over AltGr/Ctrl+Alt mappings
                        if ((shiftState & 6) == 0)
                            score += char.IsLetter(c) ? 3 : 1;
                    }
                }
                if (score > bestScore || (score == bestScore && hkl == currentHkl))
                {
                    bestScore = score;
                    sourceHkl = hkl;
                }
            }

            // Find the source layout index
            int sourceIdx = Array.IndexOf(layouts, sourceHkl);
            if (sourceIdx < 0) sourceIdx = 0;

            string bestResult = text;
            for (int step = 1; step < layouts.Length; step++)
            {
                IntPtr candidateTarget = layouts[(sourceIdx + step) % layouts.Length];
                string candidateText = MapStringBetweenLayouts(text, sourceHkl, candidateTarget, layouts);
                if (step == 1)
                {
                    targetHkl = candidateTarget;
                    bestResult = candidateText;
                }
                if (!string.Equals(candidateText, text, StringComparison.Ordinal))
                {
                    targetHkl = candidateTarget;
                    return candidateText;
                }
            }

            return bestResult;
        }

        private static string MapStringBetweenLayouts(string text, IntPtr sourceHkl, IntPtr targetHkl, IntPtr[] layouts)
        {
            var sbResult = new StringBuilder(text.Length);
            var keyState = new byte[256];
            var charBuf = new StringBuilder(8);

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (ch == '\r' || ch == '\n' || ch == '\t' || ch == ' ')
                {
                    sbResult.Append(ch);
                    continue;
                }

                short vkScan = VkKeyScanEx(ch, sourceHkl);
                if (vkScan == -1)
                {
                    // Fallback: check if character belongs to another installed layout
                    foreach (var hkl in layouts)
                    {
                        if (hkl == sourceHkl) continue;
                        vkScan = VkKeyScanEx(ch, hkl);
                        if (vkScan != -1) break;
                    }
                }

                if (vkScan == -1)
                {
                    sbResult.Append(ch);
                    continue;
                }

                uint vk = (uint)(vkScan & 0xFF);
                int shiftState = (vkScan >> 8) & 0xFF;

                Array.Clear(keyState, 0, keyState.Length);
                if ((shiftState & 1) != 0) keyState[(int)Keys.ShiftKey] = 0x80;
                if ((shiftState & 2) != 0) keyState[(int)Keys.ControlKey] = 0x80;
                if ((shiftState & 4) != 0) keyState[(int)Keys.Menu] = 0x80;

                uint scanCode = MapVirtualKeyEx(vk, 0, targetHkl);
                charBuf.Clear();
                int rc = ToUnicodeEx(vk, scanCode, keyState, charBuf, charBuf.Capacity, 0, targetHkl);
                if (rc > 0)
                {
                    sbResult.Append(charBuf.ToString(0, rc));
                }
                else
                {
                    // Clear dead-key state if any
                    ToUnicodeEx((uint)Keys.Space, MapVirtualKeyEx((uint)Keys.Space, 0, targetHkl), new byte[256], charBuf, charBuf.Capacity, 0, targetHkl);
                    sbResult.Append(ch);
                }
            }

            return sbResult.ToString();
        }

        public static void SendCopy()
        {
            var ctrlDown = MakeKeyInput(Keys.LControlKey, true);
            var cDown = MakeKeyInput(Keys.C, true);
            var cUp = MakeKeyInput(Keys.C, false);
            var ctrlUp = MakeKeyInput(Keys.LControlKey, false);
            SendInput(4, new INPUT[4] { ctrlDown, cDown, cUp, ctrlUp }, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void SendPaste()
        {
            var ctrlDown = MakeKeyInput(Keys.LControlKey, true);
            var vDown = MakeKeyInput(Keys.V, true);
            var vUp = MakeKeyInput(Keys.V, false);
            var ctrlUp = MakeKeyInput(Keys.LControlKey, false);
            SendInput(4, new INPUT[4] { ctrlDown, vDown, vUp, ctrlUp }, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void SendKeyPress(Keys vkCode, bool shift = false)
        {
            var pureVk = vkCode & Keys.KeyCode;
            var down = MakeKeyInput(pureVk, true);
            var up = MakeKeyInput(pureVk, false);

            if (shift)
            {
                var shiftDown = MakeKeyInput(Keys.LShiftKey, true);
                var shiftUp = MakeKeyInput(Keys.LShiftKey, false);
                SendInput(4, new INPUT[4] { shiftDown, down, up, shiftUp }, Marshal.SizeOf(typeof(INPUT)));
            }
            else
            {
                SendInput(2, new INPUT[2] { down, up }, Marshal.SizeOf(typeof(INPUT)));
            }
        }

        public static void ReleasePressedFnKeys()
        {
            ReleasePressedKey(Keys.RControlKey, false);
            ReleasePressedKey(Keys.LControlKey, false);
            ReleasePressedKey(Keys.ControlKey, false);
            ReleasePressedKey(Keys.LShiftKey, false);
            ReleasePressedKey(Keys.RShiftKey, false);
            ReleasePressedKey(Keys.ShiftKey, false);
            ReleasePressedKey(Keys.LMenu, false);
            ReleasePressedKey(Keys.RMenu, false);
            ReleasePressedKey(Keys.Menu, false);
            ReleasePressedKey(Keys.LWin, false);
            ReleasePressedKey(Keys.RWin, false);
        }

        public static bool ReleasePressedKey(Keys keyCode, bool releaseTwice)
        {
            if (!KeyPressed(keyCode)) { return false; }
            var keyUp = MakeKeyInput(keyCode, false);
            if (releaseTwice)
            {
                var secondDown = MakeKeyInput(keyCode, true);
                var secondUp = MakeKeyInput(keyCode, false);
                SendInput(3, new INPUT[3] { keyUp, secondDown, secondUp }, Marshal.SizeOf(typeof(INPUT)));
            }
            else
            {
                SendInput(1, new INPUT[1] { keyUp }, Marshal.SizeOf(typeof(INPUT)));
            }
            return true;
        }

        public static void SendShowSettingsMessage()
        {
            PostMessage((IntPtr)HWND_BROADCAST, WM_SHOW_SETTINGS, IntPtr.Zero, IntPtr.Zero);
        }

        #region Autorun & Task Scheduler (Admin)

        public static bool IsRunningAsAdmin()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        private static string GetAutorunShortcutPath()
        {
            return System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                "dotSwitcher.lnk");
        }

        public static bool IsAutorunShortcutExists()
        {
            return System.IO.File.Exists(GetAutorunShortcutPath());
        }

        public static bool IsAutorunTaskExists()
        {
            try
            {
                var psi = new ProcessStartInfo("schtasks.exe", $"/Query /TN \"{TaskName}\"")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using (var proc = Process.Start(psi))
                {
                    proc.WaitForExit(3000);
                    return proc.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void CreateAutorunShortcut()
        {
            var currentPath = Assembly.GetExecutingAssembly().Location;
            var shortcutLocation = GetAutorunShortcutPath();
            var description = "Simple keyboard layout switcher";
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) return;
            dynamic shell = Activator.CreateInstance(shellType);
            dynamic shortcut = shell.CreateShortcut(shortcutLocation);
            shortcut.Description = description;
            shortcut.TargetPath = currentPath;
            shortcut.WorkingDirectory = System.IO.Path.GetDirectoryName(currentPath);
            shortcut.Save();
        }

        public static void DeleteAutorunShortcut()
        {
            var path = GetAutorunShortcutPath();
            if (System.IO.File.Exists(path))
            {
                try { System.IO.File.Delete(path); } catch { }
            }
        }

        public static bool CreateAutorunTask()
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            var workDir = System.IO.Path.GetDirectoryName(exePath);
            var userId = WindowsIdentity.GetCurrent().Name;

            string xml = $@"<?xml version=""1.0"" encoding=""UTF-16""?>
<Task version=""1.2"" xmlns=""http://schemas.microsoft.com/windows/2004/02/mit/task"">
  <RegistrationInfo>
    <Description>dotSwitcher keyboard layout switcher (Elevated)</Description>
  </RegistrationInfo>
  <Triggers>
    <LogonTrigger>
      <Enabled>true</Enabled>
      <UserId>{SecurityElement.Escape(userId)}</UserId>
    </LogonTrigger>
  </Triggers>
  <Principals>
    <Principal id=""Author"">
      <UserId>{SecurityElement.Escape(userId)}</UserId>
      <LogonType>InteractiveToken</LogonType>
      <RunLevel>HighestAvailable</RunLevel>
    </Principal>
  </Principals>
  <Settings>
    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
    <AllowHardTerminate>true</AllowHardTerminate>
    <StartWhenAvailable>true</StartWhenAvailable>
    <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>
    <IdleSettings>
      <StopOnIdleEnd>false</StopOnIdleEnd>
      <RestartOnIdle>false</RestartOnIdle>
    </IdleSettings>
    <AllowStartOnDemand>true</AllowStartOnDemand>
    <Enabled>true</Enabled>
    <Hidden>false</Hidden>
    <RunOnlyIfIdle>false</RunOnlyIfIdle>
    <WakeToRun>false</WakeToRun>
    <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>
    <Priority>7</Priority>
  </Settings>
  <Actions Context=""Author"">
    <Exec>
      <Command>{SecurityElement.Escape(exePath)}</Command>
      <WorkingDirectory>{SecurityElement.Escape(workDir)}</WorkingDirectory>
    </Exec>
  </Actions>
</Task>";

            string tempXml = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"dotSwitcher_task_{Guid.NewGuid():N}.xml");
            try
            {
                System.IO.File.WriteAllText(tempXml, xml, Encoding.Unicode);
                var psi = new ProcessStartInfo("schtasks.exe", $"/Create /TN \"{TaskName}\" /XML \"{tempXml}\" /F")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using (var proc = Process.Start(psi))
                {
                    proc.WaitForExit(5000);
                    return proc.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                if (System.IO.File.Exists(tempXml))
                {
                    try { System.IO.File.Delete(tempXml); } catch { }
                }
            }
        }

        public static void DeleteAutorunTask()
        {
            if (!IsAutorunTaskExists()) return;
            try
            {
                var psi = new ProcessStartInfo("schtasks.exe", $"/Delete /TN \"{TaskName}\" /F")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using (var proc = Process.Start(psi))
                {
                    proc.WaitForExit(3000);
                }
            }
            catch { }
        }

        public static bool ConfigureAutorun(bool autoStart, bool runAsAdmin)
        {
            if (!autoStart)
            {
                DeleteAutorunShortcut();
                if (IsAutorunTaskExists())
                {
                    if (IsRunningAsAdmin())
                    {
                        DeleteAutorunTask();
                    }
                    else
                    {
                        RunElevatedAutorunHelper("--remove-task");
                    }
                }
                return true;
            }

            if (runAsAdmin)
            {
                DeleteAutorunShortcut();
                if (IsRunningAsAdmin())
                {
                    return CreateAutorunTask();
                }
                else
                {
                    return RunElevatedAutorunHelper("--create-task");
                }
            }
            else
            {
                if (IsAutorunTaskExists())
                {
                    if (IsRunningAsAdmin())
                    {
                        DeleteAutorunTask();
                    }
                    else
                    {
                        RunElevatedAutorunHelper("--remove-task");
                    }
                }
                CreateAutorunShortcut();
                return true;
            }
        }

        public static bool RunElevatedAutorunHelper(string arg)
        {
            try
            {
                var exePath = Assembly.GetExecutingAssembly().Location;
                var psi = new ProcessStartInfo(exePath, arg)
                {
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                using (var proc = Process.Start(psi))
                {
                    if (proc == null) return false;
                    proc.WaitForExit(10000);
                    return proc.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Clipboard Backup / Restore

        private static string backupText = null;
        private static bool hadText = false;

        public static Keys ToKey(char ch)
        {
            var layout = GetCurrentLayout();
            short keyNumber = VkKeyScanEx(ch, layout);
            if (keyNumber == -1)
            {
                return Keys.None;
            }
            return (Keys)(((keyNumber & 0xFF00) << 8) | (keyNumber & 0xFF));
        }

        public static void BackupClipboard()
        {
            hadText = false;
            backupText = null;
            try
            {
                if (Clipboard.ContainsText())
                {
                    hadText = true;
                    backupText = Clipboard.GetText();
                }
            }
            catch { }
        }

        public static void RestoreClipboard()
        {
            try
            {
                if (hadText && backupText != null)
                {
                    Clipboard.SetText(backupText);
                }
            }
            catch { }
        }

        #endregion
    }
}
