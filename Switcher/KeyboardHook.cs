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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace dotSwitcher.Switcher
{
    public sealed class KeyboardHook : IDisposable
    {
        public event EventHandler<KeyboardEventArgs> KeyboardEvent;
        private IntPtr hookId = IntPtr.Zero;
        private HookProc keyboardEventHook;

        public KeyboardHook()
        {
            // prevents the hook method from being GCed: do not remove this member.
            keyboardEventHook = KeyboardEventHook;
        }
        public bool IsStarted()
        {
            return hookId != IntPtr.Zero;
        }
        public void Start()
        {
            if (IsStarted()) { return; }
            hookId = LowLevelAdapter.SetHook(LowLevelAdapter.WH_KEYBOARD_LL, keyboardEventHook);
        }
        public void Stop()
        {
            if (!IsStarted()) { return; }
            LowLevelAdapter.ReleaseHook(hookId);
            hookId = IntPtr.Zero;
        }
        private void OnKeyboardEvent(KeyboardEventArgs e)
        {
            if (KeyboardEvent != null)
            {
                KeyboardEvent(this, e);
            }
        }


        private IntPtr KeyboardEventHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            bool isHandled;
            ProcessKeyboardEvent(nCode, wParam, lParam, out isHandled);
            return isHandled ?
                new IntPtr(1) :
                LowLevelAdapter.NextHook(nCode, wParam, lParam);
        }

        // returns true if event is handled
        private void ProcessKeyboardEvent(int nCode, IntPtr wParam, IntPtr lParam, out bool isHandled)
        {
            isHandled = false;
            try
            {
                if (nCode < 0)
                    return;

                bool isKeyDownEvent = false;
                switch (wParam.ToInt32())
                {
                    case LowLevelAdapter.WM_KEYDOWN:
                    case LowLevelAdapter.WM_SYSKEYDOWN:
                        isKeyDownEvent = true;
                        goto case LowLevelAdapter.WM_KEYUP;

                    case LowLevelAdapter.WM_KEYUP:
                    case LowLevelAdapter.WM_SYSKEYUP:

                        var kbdStruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                        if (kbdStruct.dwExtraInfo == LowLevelAdapter.InjectedExtraInfo)
                        {
                            return;
                        }

                        var vk = (Keys)kbdStruct.vkCode;
                        var keyData = vk;

                        bool isCtrlKey = vk == Keys.ControlKey || vk == Keys.LControlKey || vk == Keys.RControlKey;
                        bool isAltKey = vk == Keys.Menu || vk == Keys.LMenu || vk == Keys.RMenu;
                        bool isShiftKey = vk == Keys.ShiftKey || vk == Keys.LShiftKey || vk == Keys.RShiftKey;
                        bool isWinKey = vk == Keys.LWin || vk == Keys.RWin;

                        if (!isCtrlKey && LowLevelAdapter.KeyPressed(Keys.ControlKey))
                            keyData |= Keys.Control;
                        if (!isAltKey && LowLevelAdapter.KeyPressed(Keys.Menu))
                            keyData |= Keys.Alt;
                        if (!isShiftKey && LowLevelAdapter.KeyPressed(Keys.ShiftKey))
                            keyData |= Keys.Shift;

                        var winPressed = !isWinKey && (LowLevelAdapter.KeyPressed(Keys.LWin) || LowLevelAdapter.KeyPressed(Keys.RWin));

                        var args = new KeyboardEventArgs(keyData, winPressed, isKeyDownEvent ? KeyboardEventType.KeyDown : KeyboardEventType.KeyUp);
                        OnKeyboardEvent(args);

                        isHandled = args.Handled;
                        break;
                }

            }
            catch { }
        }


        public void Dispose()
        {
            Stop();
        }
    }
}
