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
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace dotSwitcher.Switcher
{
    public class SwitcherCore : IDisposable
    {
        public event EventHandler<SwitcherErrorArgs> Error;
        private List<KeyboardEventArgs> currentSelection = new List<KeyboardEventArgs>();
        private KeyboardHook kbdHook;
        private MouseHook mouseHook;
        private ISettings settings;
        private bool readyToSwitch;
        private volatile bool isProcessing;
        private SynchronizationContext uiContext;

        public SwitcherCore(ISettings settings)
        {
            this.settings = settings;
            this.uiContext = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
            kbdHook = new KeyboardHook();
            kbdHook.KeyboardEvent += ProcessKeyPress;
            mouseHook = new MouseHook();
            mouseHook.MouseEvent += ProcessMousePress;
            readyToSwitch = false;
        }

        public static bool IsPrintable(KeyboardEventArgs evtData)
        {
            if (evtData.Alt || evtData.Control || evtData.Win) { return false; }
            var keyCode = evtData.KeyCode;
            if (keyCode >= Keys.D0 && keyCode <= Keys.Z) { return true; }
            if (keyCode >= Keys.Oem1 && keyCode <= Keys.OemBackslash) { return true; }
            if (keyCode >= Keys.NumPad0 && keyCode <= Keys.NumPad9) { return true; }
            if (keyCode == Keys.Decimal) { return true; }
            return false;
        }

        public bool IsStarted()
        {
            return kbdHook.IsStarted() || mouseHook.IsStarted();
        }
        public void Start()
        {
            kbdHook.Start();
            mouseHook.Start();
        }
        public void Stop()
        {
            kbdHook.Stop();
            mouseHook.Stop();
        }

        private void RunAsync(Action action)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try { action(); }
                catch (Exception ex) { OnError(ex); }
            });
        }

        // evtData.Handled must be set to true if the key is recognized as hotkey and doesn't need further processing
        private void ProcessKeyPress(object sender, KeyboardEventArgs evtData)
        {
            if (isProcessing)
            {
                return;
            }
            try
            {
                if (evtData.Type == KeyboardEventType.KeyDown)
                    OnKeyPress(evtData);
                else
                    onKeyRelease(evtData);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private void ProcessMousePress(object sender, EventArgs evtData)
        {
            try
            {
                readyToSwitch = false;
                BeginNewSelection();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        private bool HaveModifiers(KeyboardEventArgs evtData)
        {
            var ctrl = evtData.Control;
            var alt = evtData.Alt;
            var win = evtData.Win;

            return ctrl || alt || win;
        }

        private bool HaveTrackingKeys(KeyboardEventArgs evtData)
        {
            var vkCode = evtData.KeyCode;

            return vkCode == Keys.ControlKey ||
              vkCode == Keys.LControlKey ||
              vkCode == Keys.RControlKey ||
              // yes, don't interrupt the tracking on PrtSc!
              vkCode == Keys.PrintScreen ||
              vkCode == Keys.ShiftKey ||
              vkCode == Keys.RShiftKey ||
              vkCode == Keys.LShiftKey ||
              vkCode == Keys.NumLock ||
              vkCode == Keys.Scroll;
        }

        private static bool IsModifierKey(Keys vk)
        {
            return vk == Keys.ControlKey || vk == Keys.LControlKey || vk == Keys.RControlKey ||
                   vk == Keys.ShiftKey || vk == Keys.LShiftKey || vk == Keys.RShiftKey ||
                   vk == Keys.Menu || vk == Keys.LMenu || vk == Keys.RMenu ||
                   vk == Keys.LWin || vk == Keys.RWin;
        }

        private void onKeyRelease(KeyboardEventArgs evtData)
        {
            var switchLayoutHotkey = settings.SwitchLayoutHotkey;
            if (switchLayoutHotkey != null && switchLayoutHotkey.KeyCode != Keys.None)
            {
                if (evtData.Equals(switchLayoutHotkey) || evtData.KeyCode == switchLayoutHotkey.KeyCode)
                {
                    if (readyToSwitch)
                    {
                        readyToSwitch = false;
                        SwitchLayout();
                        evtData.Handled = false;
                        return;
                    }
                }
            }
        }

        private void OnKeyPress(KeyboardEventArgs evtData)
        {
            var vkCode = evtData.KeyCode;
            var switchLayoutHotkey = settings.SwitchLayoutHotkey;
            var switchHotkey = settings.SwitchHotkey;
            var convertHotkey = settings.ConvertSelectionHotkey;

            if (convertHotkey != null && convertHotkey.KeyCode != Keys.None && evtData.Equals(convertHotkey))
            {
                readyToSwitch = false;
                evtData.Handled = true;
                RunAsync(ConvertSelection);
                return;
            }

            if (switchHotkey != null && switchHotkey.KeyCode != Keys.None && evtData.Equals(switchHotkey))
            {
                // If SwitchHotkey and SwitchLayoutHotkey are different, handle ConvertLast immediately
                if (switchLayoutHotkey == null || !switchHotkey.Equals(switchLayoutHotkey))
                {
                    readyToSwitch = false;
                    evtData.Handled = true;
                    RunAsync(ConvertLast);
                    return;
                }
            }

            if (switchLayoutHotkey != null && switchLayoutHotkey.KeyCode != Keys.None &&
                evtData.Equals(switchLayoutHotkey))
            {
                readyToSwitch = true;
                if (!IsModifierKey(vkCode))
                {
                    evtData.Handled = true;
                }
                return;
            }

            readyToSwitch = false;

            if (this.HaveTrackingKeys(evtData))
                return;

            var notModified = !this.HaveModifiers(evtData);

            if (vkCode == Keys.Space && notModified) { AddToCurrentSelection(evtData); return; }
            if (vkCode == Keys.Back && notModified) { RemoveLast(); return; }
            if (IsPrintable(evtData))
            {
                if (GetPreviousVkCode() == Keys.Space && settings.SmartSelection == false) { BeginNewSelection(); }
                AddToCurrentSelection(evtData);
                return;
            }

            // default:
            BeginNewSelection();
        }

        private void OnError(Exception ex)
        {
            var handler = Error;
            if (handler == null) return;
            if (uiContext != null)
            {
                uiContext.Post(_ => handler(this, new SwitcherErrorArgs(ex)), null);
            }
            else
            {
                handler(this, new SwitcherErrorArgs(ex));
            }
        }

        #region selection manipulation
        private Keys GetPreviousVkCode()
        {
            if (currentSelection.Count == 0) { return Keys.None; }
            return currentSelection[currentSelection.Count - 1].KeyCode;
        }
        private void BeginNewSelection() { currentSelection.Clear(); }
        private void AddToCurrentSelection(KeyboardEventArgs data) { currentSelection.Add(data); }
        private void RemoveLast()
        {
            if (currentSelection.Count == 0) { return; }
            currentSelection.RemoveAt(currentSelection.Count - 1);
        }
        #endregion

        private void ConvertSelection()
        {
            if (isProcessing) return;
            isProcessing = true;
            try
            {
                BeginNewSelection();
                LowLevelAdapter.BackupClipboard();

                if (LowLevelAdapter.ReleasePressedFnKeys())
                {
                    Thread.Sleep(4);
                }

                uint seqBefore = LowLevelAdapter.GetClipboardSeqNumber();
                LowLevelAdapter.SendCopy();

                var sw = Stopwatch.StartNew();
                bool retriedCopy = false;
                while (sw.ElapsedMilliseconds < 110)
                {
                    Thread.Sleep(2);
                    if (LowLevelAdapter.GetClipboardSeqNumber() != seqBefore)
                    {
                        break;
                    }
                    if (!retriedCopy && sw.ElapsedMilliseconds >= 45)
                    {
                        retriedCopy = true;
                        LowLevelAdapter.ReleasePressedFnKeys();
                        LowLevelAdapter.SendCopy();
                    }
                }

                if (LowLevelAdapter.GetClipboardSeqNumber() == seqBefore)
                {
                    return;
                }

                string selection;
                if (!LowLevelAdapter.TryGetClipboardText(out selection) || string.IsNullOrEmpty(selection))
                {
                    LowLevelAdapter.RestoreClipboard();
                    return;
                }

                IntPtr targetHkl;
                string converted = LowLevelAdapter.ConvertTextBetweenLayouts(selection, out targetHkl, settings);
                if (string.IsNullOrEmpty(converted))
                {
                    LowLevelAdapter.RestoreClipboard();
                    return;
                }

                if (!LowLevelAdapter.TrySetClipboardText(converted, true))
                {
                    LowLevelAdapter.RestoreClipboard();
                    return;
                }

                LowLevelAdapter.ReleasePressedFnKeys();
                LowLevelAdapter.SendPaste();
                LowLevelAdapter.SetKeyboardLayout(targetHkl);

                Thread.Sleep(Math.Max(25, settings.SwitchDelay));
                LowLevelAdapter.RestoreClipboard();
            }
            finally
            {
                isProcessing = false;
            }
        }

        private void SwitchLayout()
        {
            BeginNewSelection();
            LowLevelAdapter.SetNextKeyboardLayout(settings);
        }

        private void ConvertLast()
        {
            if (isProcessing) return;
            var selection = currentSelection.ToList();
            if (selection.Count == 0)
            {
                SwitchLayout();
                return;
            }

            isProcessing = true;
            try
            {
                if (LowLevelAdapter.ReleasePressedFnKeys())
                {
                    Thread.Sleep(3);
                }

                LowLevelAdapter.SendBackspaces(selection.Count);

                Thread.Sleep(settings.SwitchDelay);
                var vkList = selection.Select(x => x.KeyCode).ToList();
                var targetHkl = LowLevelAdapter.GetNextLayoutForConversion(vkList, settings);
                LowLevelAdapter.SetKeyboardLayout(targetHkl);
                Thread.Sleep(settings.SwitchDelay);

                foreach (var data in selection)
                {
                    LowLevelAdapter.SendKeyPress(data.KeyCode, data.Shift);
                }
            }
            finally
            {
                isProcessing = false;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }

    public class SwitcherErrorArgs : EventArgs
    {
        public Exception Error { get; private set; }
        public SwitcherErrorArgs(Exception ex)
        {
            Error = ex;
        }
    }
}
