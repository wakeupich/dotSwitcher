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
using System.Globalization;
using dotSwitcher.Data;

namespace dotSwitcher.WinApi
{
    public class LayoutItem
    {
        public string Id { get; set; }
        public IntPtr Handle { get; set; }
        public string DisplayName { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
    }

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
            if (fgWnd == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }
            var threadId = GetWindowThreadProcessId(fgWnd, IntPtr.Zero);
            var info = new GUITHREADINFO();
            info.cbSize = Marshal.SizeOf(info);
            if (GetGUIThreadInfo(threadId, ref info))
            {
                if (info.hwndCaret != IntPtr.Zero)
                    return info.hwndCaret;
                if (info.hwndFocus != IntPtr.Zero)
                    return info.hwndFocus;
                if (info.hwndActive != IntPtr.Zero)
                    return info.hwndActive;
            }

            var focusedHandle = GetFocusedHandle();
            if (focusedHandle != IntPtr.Zero)
            {
                return focusedHandle;
            }
            return fgWnd;
        }

        private static IntPtr lastSwitchedFgWindow = IntPtr.Zero;
        private static IntPtr lastObservedOsHkl = IntPtr.Zero;
        private static IntPtr lastTargetHkl = IntPtr.Zero;

        private static IntPtr GetRawOsLayout(out IntPtr fgWnd, out IntPtr fgHkl)
        {
            fgWnd = GetForegroundWindow();
            var hWnd = GetTargetWindowHandle();
            var wndThreadId = GetWindowThreadProcessId(hWnd != IntPtr.Zero ? hWnd : fgWnd, IntPtr.Zero);
            var osHkl = GetKeyboardLayout(wndThreadId);
            if (fgWnd != IntPtr.Zero && fgWnd != hWnd)
            {
                var fgThreadId = GetWindowThreadProcessId(fgWnd, IntPtr.Zero);
                fgHkl = GetKeyboardLayout(fgThreadId);
            }
            else
            {
                fgHkl = osHkl;
            }
            return osHkl;
        }

        public static IntPtr GetCurrentLayout()
        {
            IntPtr fgWnd;
            IntPtr fgHkl;
            var osHkl = GetRawOsLayout(out fgWnd, out fgHkl);

            if (fgWnd != IntPtr.Zero && fgWnd == lastSwitchedFgWindow && lastTargetHkl != IntPtr.Zero)
            {
                string targetId = GetLayoutId(lastTargetHkl);
                string osId = GetLayoutId(osHkl);
                string fgId = GetLayoutId(fgHkl);
                string prevOsId = GetLayoutId(lastObservedOsHkl);

                if (string.Equals(osId, targetId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(fgId, targetId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(osId, prevOsId, StringComparison.OrdinalIgnoreCase))
                {
                    return lastTargetHkl;
                }
            }

            return osHkl;
        }

        public static string GetLayoutId(IntPtr hkl)
        {
            uint id = (uint)(hkl.ToInt64() & 0xFFFFFFFFL);
            return id.ToString("X8");
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

        public static List<LayoutItem> GetInstalledLayoutItems()
        {
            var result = new List<LayoutItem>();
            var layouts = GetInstalledLayouts();
            foreach (var hkl in layouts)
            {
                string id = GetLayoutId(hkl);
                string display = id;
                try
                {
                    InputLanguage lang = null;
                    foreach (InputLanguage l in InputLanguage.InstalledInputLanguages)
                    {
                        if (string.Equals(GetLayoutId(l.Handle), id, StringComparison.OrdinalIgnoreCase))
                        {
                            lang = l;
                            break;
                        }
                    }
                    if (lang != null && lang.Culture != null)
                    {
                        string iso = lang.Culture.TwoLetterISOLanguageName.ToUpperInvariant();
                        string cultureName = lang.Culture.DisplayName;
                        string layoutName = lang.LayoutName;
                        if (!string.IsNullOrEmpty(layoutName) &&
                            !string.Equals(cultureName, layoutName, StringComparison.OrdinalIgnoreCase))
                        {
                            display = string.Format("[{0}] {1} - {2}", iso, cultureName, layoutName);
                        }
                        else
                        {
                            display = string.Format("[{0}] {1}", iso, cultureName);
                        }
                    }
                    else
                    {
                        int langId = (int)(hkl.ToInt64() & 0xFFFF);
                        var ci = new CultureInfo(langId);
                        display = string.Format("[{0}] {1} (0x{2})", ci.TwoLetterISOLanguageName.ToUpperInvariant(), ci.DisplayName, id);
                    }
                }
                catch
                {
                    display = "0x" + id;
                }

                result.Add(new LayoutItem
                {
                    Id = id,
                    Handle = hkl,
                    DisplayName = display
                });
            }
            return result;
        }

        private static int FindLayoutIndex(IList<IntPtr> layouts, IntPtr target)
        {
            if (layouts == null || layouts.Count == 0) return -1;
            string targetId = GetLayoutId(target);
            for (int i = 0; i < layouts.Count; i++)
            {
                if (string.Equals(GetLayoutId(layouts[i]), targetId, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            for (int i = 0; i < layouts.Count; i++)
            {
                if (((long)layouts[i] & 0xFFFF) == ((long)target & 0xFFFF))
                {
                    return i;
                }
            }
            return -1;
        }

        public static IntPtr[] GetAllowedLayouts(ISettings settings)
        {
            var allLayouts = GetInstalledLayouts();
            if (allLayouts.Length == 0 || settings == null || settings.FilterLayouts != true || string.IsNullOrWhiteSpace(settings.EnabledLayouts))
            {
                return allLayouts;
            }

            var enabledIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var parts = settings.EnabledLayouts.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                enabledIds.Add(p.Trim());
            }

            if (enabledIds.Count == 0)
            {
                return allLayouts;
            }

            var filtered = new List<IntPtr>(allLayouts.Length);
            foreach (var hkl in allLayouts)
            {
                if (enabledIds.Contains(GetLayoutId(hkl)))
                {
                    filtered.Add(hkl);
                }
            }

            return filtered.Count > 0 ? filtered.ToArray() : allLayouts;
        }

        public static IntPtr GetNextLayout(IntPtr currentLayout, ISettings settings = null)
        {
            var allowed = GetAllowedLayouts(settings);
            if (allowed.Length == 0)
            {
                return (IntPtr)HKL_NEXT;
            }
            int idx = FindLayoutIndex(allowed, currentLayout);
            if (idx >= 0)
            {
                return allowed[(idx + 1) % allowed.Length];
            }

            // Current layout is excluded from cycle: find the next allowed layout following currentLayout in OS order
            var all = GetInstalledLayouts();
            int allIdx = FindLayoutIndex(all, currentLayout);
            if (allIdx >= 0)
            {
                for (int step = 1; step <= all.Length; step++)
                {
                    var candidate = all[(allIdx + step) % all.Length];
                    int allowedIdx = FindLayoutIndex(allowed, candidate);
                    if (allowedIdx >= 0)
                    {
                        return allowed[allowedIdx];
                    }
                }
            }

            return allowed[0];
        }

        private static void TrySetTsfLanguage(ushort langId)
        {
            if (langId == 0) return;
            try
            {
                ITfInputProcessorProfiles profiles;
                if (TF_CreateInputProcessorProfiles(out profiles) == 0 && profiles != null)
                {
                    try
                    {
                        profiles.ChangeCurrentLanguage(langId);
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(profiles);
                    }
                }
            }
            catch { }
        }

        public static void SetKeyboardLayout(IntPtr targetHkl)
        {
            IntPtr fgWnd;
            IntPtr fgHkl;
            var rawOsHkl = GetRawOsLayout(out fgWnd, out fgHkl);
            IntPtr hWnd = GetTargetWindowHandle();

            if (targetHkl != IntPtr.Zero && targetHkl != (IntPtr)HKL_NEXT)
            {
                lastSwitchedFgWindow = fgWnd;
                lastObservedOsHkl = rawOsHkl;
                lastTargetHkl = targetHkl;

                if (hWnd != IntPtr.Zero && hWnd != fgWnd)
                {
                    PostMessage(hWnd, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, targetHkl);
                }
                if (fgWnd != IntPtr.Zero)
                {
                    PostMessage(fgWnd, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, targetHkl);
                }

                ushort langId = (ushort)(targetHkl.ToInt64() & 0xFFFF);
                TrySetTsfLanguage(langId);
            }
            else
            {
                if (hWnd != IntPtr.Zero)
                {
                    PostMessage(hWnd, WM_INPUTLANGCHANGEREQUEST, (IntPtr)INPUTLANGCHANGE_FORWARD, (IntPtr)HKL_NEXT);
                }
            }
        }

        public static void SetNextKeyboardLayout(ISettings settings = null)
        {
            var current = GetCurrentLayout();
            var next = GetNextLayout(current, settings);
            SetKeyboardLayout(next);
        }

        public static IntPtr GetNextLayoutForConversion(IList<Keys> vkCodes, ISettings settings = null)
        {
            var currentHkl = GetCurrentLayout();
            var allowed = GetAllowedLayouts(settings);
            if (allowed.Length == 0 || vkCodes == null || vkCodes.Count == 0)
            {
                return GetNextLayout(currentHkl, settings);
            }

            int startIdx = FindLayoutIndex(allowed, currentHkl);
            var keyState = new byte[256];
            var buf1 = new StringBuilder(8);
            var buf2 = new StringBuilder(8);

            int count = allowed.Length;
            for (int step = 1; step <= count; step++)
            {
                int candidateIdx = startIdx >= 0 ? (startIdx + step) % count : (step - 1) % count;
                IntPtr candidate = allowed[candidateIdx];
                if (string.Equals(GetLayoutId(candidate), GetLayoutId(currentHkl), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

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

            return GetNextLayout(currentHkl, settings);
        }

        public static string ConvertTextBetweenLayouts(string text, out IntPtr targetHkl, ISettings settings = null)
        {
            var currentHkl = GetCurrentLayout();
            var allLayouts = GetInstalledLayouts();
            var allowedLayouts = GetAllowedLayouts(settings);
            targetHkl = GetNextLayout(currentHkl, settings);

            if (string.IsNullOrEmpty(text) || allLayouts.Length < 2)
            {
                return text;
            }

            // Detect source layout by checking which layout maps the characters best (especially non-ASCII / letters)
            IntPtr sourceHkl = currentHkl;
            int bestScore = -1;

            foreach (var hkl in allLayouts)
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
                bool isCurrent = string.Equals(GetLayoutId(hkl), GetLayoutId(currentHkl), StringComparison.OrdinalIgnoreCase);
                bool isAllowed = FindLayoutIndex(allowedLayouts, hkl) >= 0;
                if (score > bestScore ||
                    (score == bestScore && (isCurrent || (isAllowed && FindLayoutIndex(allowedLayouts, sourceHkl) < 0))))
                {
                    bestScore = score;
                    sourceHkl = hkl;
                }
            }

            int startIdx = FindLayoutIndex(allowedLayouts, sourceHkl);
            string bestResult = text;
            bool firstSet = false;

            for (int step = 1; step <= allowedLayouts.Length; step++)
            {
                int idx = startIdx >= 0 ? (startIdx + step) % allowedLayouts.Length : (step - 1) % allowedLayouts.Length;
                IntPtr candidateTarget = allowedLayouts[idx];
                if (string.Equals(GetLayoutId(candidateTarget), GetLayoutId(sourceHkl), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string candidateText = MapStringBetweenLayouts(text, sourceHkl, candidateTarget, allLayouts);
                if (!firstSet)
                {
                    firstSet = true;
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

        public static string KeysToString(IList<KeyboardEventArgs> keys, IntPtr hkl)
        {
            if (keys == null || keys.Count == 0)
            {
                return "";
            }

            var sb = new StringBuilder(keys.Count);
            var keyState = new byte[256];
            var charBuf = new StringBuilder(8);

            for (int i = 0; i < keys.Count; i++)
            {
                var k = keys[i];
                uint vk = (uint)(k.KeyCode & Keys.KeyCode);
                if (vk == (uint)Keys.Space)
                {
                    sb.Append(' ');
                    continue;
                }
                if (vk == (uint)Keys.Back)
                {
                    continue;
                }

                Array.Clear(keyState, 0, keyState.Length);
                if (k.Shift)
                {
                    keyState[(int)Keys.ShiftKey] = 0x80;
                }

                uint scanCode = MapVirtualKeyEx(vk, 0, hkl);
                charBuf.Clear();
                int rc = ToUnicodeEx(vk, scanCode, keyState, charBuf, charBuf.Capacity, 0, hkl);
                if (rc > 0)
                {
                    sb.Append(charBuf.ToString(0, rc));
                }
                else if (rc < 0)
                {
                    // Clear dead-key state
                    ToUnicodeEx((uint)Keys.Space, MapVirtualKeyEx((uint)Keys.Space, 0, hkl), new byte[256], charBuf, charBuf.Capacity, 0, hkl);
                }
            }

            return sb.ToString();
        }

        private static readonly HashSet<string> BlockedWindowClasses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "UnityWndClass",
            "UnrealWindow",
            "CryENGINE",
            "Valve001",
            "Source2",
            "SDL_app",
            "GLFW30",
            "LWJGL"
        };

        public static bool IsAutoSwitchBlockedWindow()
        {
            try
            {
                IntPtr fgWnd = GetForegroundWindow();
                if (fgWnd == IntPtr.Zero)
                {
                    return false;
                }

                uint threadId = GetWindowThreadProcessId(fgWnd, IntPtr.Zero);
                var info = new GUITHREADINFO();
                info.cbSize = Marshal.SizeOf(info);
                bool hasGuiInfo = GetGUIThreadInfo(threadId, ref info);

                var classBuf = new StringBuilder(128);
                if (GetClassName(fgWnd, classBuf, classBuf.Capacity) > 0)
                {
                    string cls = classBuf.ToString();
                    if (BlockedWindowClasses.Contains(cls))
                    {
                        if (!hasGuiInfo || info.hwndCaret == IntPtr.Zero)
                        {
                            return true;
                        }
                    }
                }
            }
            catch { }
            return false;
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

        public static void SendBackspaces(int count)
        {
            if (count <= 0) return;
            var inputs = new INPUT[count * 2];
            var down = MakeKeyInput(Keys.Back, true);
            var up = MakeKeyInput(Keys.Back, false);
            for (int i = 0; i < count; i++)
            {
                inputs[i * 2] = down;
                inputs[i * 2 + 1] = up;
            }
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private static readonly Keys[] ModifierKeysToRelease = new Keys[]
        {
            Keys.RControlKey,
            Keys.LControlKey,
            Keys.ControlKey,
            Keys.LShiftKey,
            Keys.RShiftKey,
            Keys.ShiftKey,
            Keys.LMenu,
            Keys.RMenu,
            Keys.Menu,
            Keys.LWin,
            Keys.RWin
        };

        public static bool ReleasePressedFnKeys()
        {
            var inputs = new List<INPUT>(6);
            for (int i = 0; i < ModifierKeysToRelease.Length; i++)
            {
                var k = ModifierKeysToRelease[i];
                if (KeyPressed(k))
                {
                    inputs.Add(MakeKeyInput(k, false));
                }
            }
            if (inputs.Count == 0)
            {
                return false;
            }
            SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf(typeof(INPUT)));
            return true;
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
                var psi = new ProcessStartInfo("schtasks.exe", string.Format("/Query /TN \"{0}\"", TaskName))
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

            string xml = string.Format(@"<?xml version=""1.0"" encoding=""UTF-16""?>
<Task version=""1.2"" xmlns=""http://schemas.microsoft.com/windows/2004/02/mit/task"">
  <RegistrationInfo>
    <Description>dotSwitcher keyboard layout switcher (Elevated)</Description>
  </RegistrationInfo>
  <Triggers>
    <LogonTrigger>
      <Enabled>true</Enabled>
      <UserId>{0}</UserId>
    </LogonTrigger>
  </Triggers>
  <Principals>
    <Principal id=""Author"">
      <UserId>{0}</UserId>
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
      <Command>{1}</Command>
      <WorkingDirectory>{2}</WorkingDirectory>
    </Exec>
  </Actions>
</Task>",
                SecurityElement.Escape(userId),
                SecurityElement.Escape(exePath),
                SecurityElement.Escape(workDir));

            string tempXml = System.IO.Path.Combine(System.IO.Path.GetTempPath(), string.Format("dotSwitcher_task_{0:N}.xml", Guid.NewGuid()));
            try
            {
                System.IO.File.WriteAllText(tempXml, xml, Encoding.Unicode);
                var psi = new ProcessStartInfo("schtasks.exe", string.Format("/Create /TN \"{0}\" /XML \"{1}\" /F", TaskName, tempXml))
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
                var psi = new ProcessStartInfo("schtasks.exe", string.Format("/Delete /TN \"{0}\" /F", TaskName))
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

        #region Fast Native Win32 Clipboard & Backup / Restore

        private static string backupText = null;
        private static bool hadText = false;
        private static readonly uint FormatExcludeMonitor = RegisterClipboardFormat("ExcludeClipboardContentFromMonitorProcessing");
        private static readonly uint FormatCanIncludeHistory = RegisterClipboardFormat("CanIncludeInClipboardHistory");
        private static readonly uint FormatCanUploadCloud = RegisterClipboardFormat("CanUploadToCloudClipboard");

        public static uint GetClipboardSeqNumber()
        {
            return GetClipboardSequenceNumber();
        }

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

        public static bool TryGetClipboardText(out string text)
        {
            text = null;
            if (!IsClipboardFormatAvailable(CF_UNICODETEXT))
            {
                return false;
            }

            for (int attempt = 0; attempt < 15; attempt++)
            {
                if (OpenClipboard(IntPtr.Zero))
                {
                    try
                    {
                        IntPtr handle = GetClipboardData(CF_UNICODETEXT);
                        if (handle == IntPtr.Zero)
                        {
                            return false;
                        }
                        IntPtr ptr = GlobalLock(handle);
                        if (ptr == IntPtr.Zero)
                        {
                            return false;
                        }
                        try
                        {
                            text = Marshal.PtrToStringUni(ptr);
                            return text != null;
                        }
                        finally
                        {
                            GlobalUnlock(handle);
                        }
                    }
                    finally
                    {
                        CloseClipboard();
                    }
                }
                Thread.Sleep(2);
            }
            return false;
        }

        private static void SetClipboardDword(uint format, int value)
        {
            if (format == 0) return;
            IntPtr hMem = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)4);
            if (hMem == IntPtr.Zero) return;
            IntPtr ptr = GlobalLock(hMem);
            if (ptr == IntPtr.Zero)
            {
                GlobalFree(hMem);
                return;
            }
            try
            {
                Marshal.WriteInt32(ptr, value);
            }
            finally
            {
                GlobalUnlock(hMem);
            }
            if (SetClipboardData(format, hMem) == IntPtr.Zero)
            {
                GlobalFree(hMem);
            }
        }

        public static bool TrySetClipboardText(string text, bool excludeFromHistory)
        {
            if (text == null)
            {
                return false;
            }

            for (int attempt = 0; attempt < 15; attempt++)
            {
                if (OpenClipboard(IntPtr.Zero))
                {
                    try
                    {
                        EmptyClipboard();
                        int bytes = (text.Length + 1) * 2;
                        IntPtr hGlobal = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)bytes);
                        if (hGlobal == IntPtr.Zero)
                        {
                            return false;
                        }
                        IntPtr ptr = GlobalLock(hGlobal);
                        if (ptr == IntPtr.Zero)
                        {
                            GlobalFree(hGlobal);
                            return false;
                        }
                        try
                        {
                            Marshal.Copy(text.ToCharArray(), 0, ptr, text.Length);
                            Marshal.WriteInt16(ptr, text.Length * 2, 0);
                        }
                        finally
                        {
                            GlobalUnlock(hGlobal);
                        }
                        if (SetClipboardData(CF_UNICODETEXT, hGlobal) == IntPtr.Zero)
                        {
                            GlobalFree(hGlobal);
                            return false;
                        }
                        if (excludeFromHistory)
                        {
                            SetClipboardDword(FormatExcludeMonitor, 1);
                            SetClipboardDword(FormatCanIncludeHistory, 0);
                            SetClipboardDword(FormatCanUploadCloud, 0);
                        }
                        return true;
                    }
                    finally
                    {
                        CloseClipboard();
                    }
                }
                Thread.Sleep(2);
            }
            return false;
        }

        public static void BackupClipboard()
        {
            hadText = TryGetClipboardText(out backupText);
        }

        public static void RestoreClipboard()
        {
            if (hadText && backupText != null)
            {
                TrySetClipboardText(backupText, true);
            }
        }

        #endregion
    }
}
