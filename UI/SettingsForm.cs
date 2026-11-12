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
using dotSwitcher.Switcher;
using dotSwitcher.WinApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dotSwitcher.UI
{
    public partial class SettingsForm : Form
    {
        public event EventHandler<EventArgs> Exit;
        Settings settings;
        SwitcherCore engine;

        public SettingsForm(Settings settings, SwitcherCore engine)
        {
            this.settings = settings;
            this.engine = engine;
            engine.Error += OnEngineError;

            LowLevelAdapter.AllowShowSettingsFromNonElevated();

            InitializeComponent();
            InitializeTrayIcon();
            InitializeHotkeyBoxes();

            if (LowLevelAdapter.IsRunningAsAdmin())
            {
                this.Text = "dotSwitcher Settings (Administrator)";
            }

            UpdateUi();
        }

        /**
         * SETTINGS FORM
         */
        void ShowForm()
        {
            engine.Stop();
            kbdHook.Start();
            icon.Hide();
            TopMost = true;
            UpdateUi();
            Show();
            Activate();
        }

        void HideForm()
        {
            currentHotkeyType = HotKeyType.None;
            currentHotkey = null;
            kbdHook.Stop();
            engine.Start();
            if (settings.ShowTrayIcon == true)
            {
                icon.Show();
            }
            TopMost = false;
            UpdateUi();
            Hide();
        }

        // Update input values and icon state
        void UpdateUi()
        {
            textBoxSwitchHotkey.Text = (settings.SwitchHotkey ?? KeyboardEventArgs.Empty).ToString();
            textBoxConvertHotkey.Text = (settings.ConvertSelectionHotkey ?? KeyboardEventArgs.Empty).ToString();
            textBoxSwitchLayoutHotkey.Text = (settings.SwitchLayoutHotkey ?? KeyboardEventArgs.Empty).ToString();
            checkBoxAutorun.Checked = settings.AutoStart == true;
            checkBoxRunAsAdmin.Checked = settings.RunAsAdmin == true;
            checkBoxTrayIcon.Checked = settings.ShowTrayIcon == true;
            checkBoxSmartSelection.Checked = settings.SmartSelection == true;
            checkBoxAutoSwitch.Checked = settings.AutoSwitch == true;
            DisplaySwitchDelay(settings.SwitchDelay);
            PopulateLayoutsUi();
            icon.SetRunning(engine.IsStarted());
            icon.SetAutoSwitch(settings.AutoSwitch == true);
        }

        private bool isUpdatingLayoutsUi = false;

        void PopulateLayoutsUi()
        {
            isUpdatingLayoutsUi = true;
            try
            {
                bool filterActive = settings.FilterLayouts == true;
                radioSelectedLayouts.Checked = filterActive;
                radioAllLayouts.Checked = !filterActive;
                checkedListBoxLayouts.Enabled = true;

                var enabledSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (!string.IsNullOrWhiteSpace(settings.EnabledLayouts))
                {
                    var parts = settings.EnabledLayouts.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var p in parts)
                    {
                        enabledSet.Add(p.Trim());
                    }
                }

                var items = LowLevelAdapter.GetInstalledLayoutItems();
                checkedListBoxLayouts.BeginUpdate();
                checkedListBoxLayouts.Items.Clear();
                foreach (var item in items)
                {
                    bool isChecked = !filterActive || enabledSet.Count == 0 || enabledSet.Contains(item.Id);
                    checkedListBoxLayouts.Items.Add(item, isChecked);
                }
                checkedListBoxLayouts.EndUpdate();
            }
            finally
            {
                isUpdatingLayoutsUi = false;
            }
        }

        // also ESC
        void buttonCancelSettings_Click(object sender, EventArgs e)
        {
            ResetSettings();
            HideForm();
        }

        void buttonSaveSettings_Click(object sender, EventArgs e)
        {
            ApplyCurrentHotkey();
            if (settings.SwitchHotkey != null && (settings.SwitchHotkey.Alt || settings.SwitchHotkey.Win))
            {
                MessageBox.Show("Sorry, win+ and alt+ hotkeys are not supported yet for Convert last word");
                return;
            }

            var selectedIds = new List<string>();
            foreach (var obj in checkedListBoxLayouts.CheckedItems)
            {
                var item = obj as LayoutItem;
                if (item != null)
                {
                    selectedIds.Add(item.Id);
                }
            }

            int totalItems = checkedListBoxLayouts.Items.Count;
            bool useSelectedOnly = radioSelectedLayouts.Checked || (totalItems > 0 && selectedIds.Count < totalItems);

            if (useSelectedOnly)
            {
                if (selectedIds.Count == 0)
                {
                    MessageBox.Show(
                        "Please select at least one keyboard layout or choose 'All installed layouts'.",
                        "dotSwitcher",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
                settings.FilterLayouts = true;
                settings.EnabledLayouts = string.Join(",", selectedIds.ToArray());
            }
            else
            {
                settings.FilterLayouts = false;
                settings.EnabledLayouts = "";
            }

            SaveSettings();
            HideForm();
        }

        void buttonExit_Click(object sender, EventArgs e)
        {
            ResetSettings();
            OnExit();
        }

        // initial hidden state
        void SettingsForm_Shown(object sender, EventArgs e)
        {
            HideForm();
        }

        // prevent the form from user-initiated closing ([x] click, Alt+F4, closing from taskbar)
        // (acts as Cancel)
        void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing)
            {
                return;
            }

            e.Cancel = true;
            ResetSettings();
            HideForm();
        }

        // receive window message from another instance
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == LowLevelAdapter.WM_SHOW_SETTINGS)
            {
                ShowForm();
            }
            base.WndProc(ref m);
        }

        /**
         * TRAY ICON
         */
        TrayIcon icon;
        void InitializeTrayIcon()
        {
            icon = new TrayIcon(settings.ShowTrayIcon);
            icon.DoubleClick += icon_SettingsClick;
            icon.ExitClick += icon_ExitClick;
            icon.SettingsClick += icon_SettingsClick;
            icon.TogglePowerClick += icon_TogglePowerClick;
            icon.ToggleAutoSwitchClick += icon_ToggleAutoSwitchClick;
        }
        void icon_ToggleAutoSwitchClick(object sender, EventArgs e)
        {
            settings.AutoSwitch = settings.AutoSwitch != true;
            settings.Save();
            UpdateUi();
        }
        void icon_TogglePowerClick(object sender, EventArgs e)
        {
            if (engine.IsStarted())
            {
                engine.Stop();
                UpdateUi();
            }
            else
            {
                engine.Start();
                UpdateUi();
            }
        }
        void icon_SettingsClick(object sender, EventArgs e)
        {
            ShowForm();
        }
        void icon_ExitClick(object sender, EventArgs e)
        {
            OnExit();
        }
        void OnEngineError(object sender, SwitcherErrorArgs e)
        {
            icon.ShowTooltip(e.Error.Message, ToolTipIcon.Error);
        }

        /**
         * HOTKEY INPUTS
         */
        KeyboardHook kbdHook;
        KeyboardEventArgs currentHotkey;
        HotKeyType currentHotkeyType;
        enum HotKeyType { None, Switch, Convert, SwitchLayout }

        void InitializeHotkeyBoxes()
        {
            textBoxSwitchHotkey.GotFocus += (s, e) => { currentHotkeyType = HotKeyType.Switch; currentHotkey = null; };
            textBoxSwitchHotkey.Enter += (s, e) => { currentHotkeyType = HotKeyType.Switch; currentHotkey = null; };
            textBoxSwitchHotkey.LostFocus += (s, e) => ApplyCurrentHotkey();
            textBoxSwitchHotkey.Leave += (s, e) => ApplyCurrentHotkey();

            textBoxConvertHotkey.GotFocus += (s, e) => { currentHotkeyType = HotKeyType.Convert; currentHotkey = null; };
            textBoxConvertHotkey.Enter += (s, e) => { currentHotkeyType = HotKeyType.Convert; currentHotkey = null; };
            textBoxConvertHotkey.LostFocus += (s, e) => ApplyCurrentHotkey();
            textBoxConvertHotkey.Leave += (s, e) => ApplyCurrentHotkey();

            textBoxSwitchLayoutHotkey.GotFocus += (s, e) => { currentHotkeyType = HotKeyType.SwitchLayout; currentHotkey = null; };
            textBoxSwitchLayoutHotkey.Enter += (s, e) => { currentHotkeyType = HotKeyType.SwitchLayout; currentHotkey = null; };
            textBoxSwitchLayoutHotkey.LostFocus += (s, e) => ApplyCurrentHotkey();
            textBoxSwitchLayoutHotkey.Leave += (s, e) => ApplyCurrentHotkey();

            currentHotkeyType = HotKeyType.None;
            kbdHook = new KeyboardHook();
            kbdHook.KeyboardEvent += kbdHook_KeyboardEvent;
        }

        void kbdHook_KeyboardEvent(object sender, KeyboardEventArgs e)
        {
            if (currentHotkeyType == HotKeyType.None)
            {
                return;
            }

            if (e.Type != KeyboardEventType.KeyDown)
            {
                return;
            }

            var vk = e.KeyCode;
            if ((vk == Keys.Escape || vk == Keys.Back) && !e.HasModifiers())
            {
                e.Handled = true;
                ResetCurrentHotkey(vk == Keys.Back);
                return;
            }

            if (vk != Keys.LMenu && vk != Keys.RMenu
                && vk != Keys.LWin && vk != Keys.RWin
                && vk != Keys.LShiftKey && vk != Keys.RShiftKey
                && vk != Keys.LControlKey && vk != Keys.RControlKey)
            {
                e.Handled = true;
            }

            currentHotkey = e;
            StoreCurrentHotkey(currentHotkey);
            SetCurrentHotkeyInputText(e.ToString());
        }

        void SetCurrentHotkeyInputText(string text)
        {
            TextBox currentTextBox;
            switch (currentHotkeyType)
            {
                case HotKeyType.Switch:
                    currentTextBox = textBoxSwitchHotkey;
                    break;
                case HotKeyType.Convert:
                    currentTextBox = textBoxConvertHotkey;
                    break;
                case HotKeyType.SwitchLayout:
                    currentTextBox = textBoxSwitchLayoutHotkey;
                    break;
                default:
                    currentTextBox = null;
                    break;
            }
            if (currentTextBox == null) { return; }
            Invoke((MethodInvoker)delegate { currentTextBox.Text = text; });
        }

        void ResetCurrentHotkey(bool clear)
        {
            switch (currentHotkeyType)
            {
                case HotKeyType.Switch:
                    currentHotkey = clear ? KeyboardEventArgs.Empty : settings.SwitchHotkey;
                    break;
                case HotKeyType.Convert:
                    currentHotkey = clear ? KeyboardEventArgs.Empty : settings.ConvertSelectionHotkey;
                    break;
                case HotKeyType.SwitchLayout:
                    currentHotkey = clear ? KeyboardEventArgs.Empty : settings.SwitchLayoutHotkey;
                    break;
                default:
                    currentHotkey = null;
                    break;
            }
            if (currentHotkey != null)
            {
                StoreCurrentHotkey(currentHotkey);
            }
            SetCurrentHotkeyInputText(currentHotkey == null ? "None" : currentHotkey.ToString());
        }

        void StoreCurrentHotkey(KeyboardEventArgs hk)
        {
            if (hk == null) return;
            switch (currentHotkeyType)
            {
                case HotKeyType.Switch:
                    settings.SwitchHotkey = hk;
                    break;
                case HotKeyType.Convert:
                    settings.ConvertSelectionHotkey = hk;
                    break;
                case HotKeyType.SwitchLayout:
                    settings.SwitchLayoutHotkey = hk;
                    settings.SwitchLayoutInitialized = true;
                    break;
            }
        }

        void ApplyCurrentHotkey()
        {
            if (!Visible || currentHotkey == null)
            {
                return;
            }
            StoreCurrentHotkey(currentHotkey);
            currentHotkey = null;
            currentHotkeyType = HotKeyType.None;
        }

        /**
         * SETTINGS
         */
        void SaveSettings()
        {
            settings.Save();

            bool autoStart = settings.AutoStart == true;
            bool runAsAdmin = settings.RunAsAdmin == true;

            LowLevelAdapter.ConfigureAutorun(autoStart, runAsAdmin);

            if (runAsAdmin && !LowLevelAdapter.IsRunningAsAdmin())
            {
                var res = MessageBox.Show(
                    "Restart dotSwitcher as Administrator now so hotkeys also work in elevated windows?",
                    "dotSwitcher",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (res == DialogResult.Yes)
                {
                    RestartAsAdmin();
                }
            }
        }

        void RestartAsAdmin()
        {
            try
            {
                var exePath = Assembly.GetExecutingAssembly().Location;
                var psi = new ProcessStartInfo(exePath, "--restart-admin")
                {
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(psi);
                OnExit();
            }
            catch { }
        }

        void ResetSettings()
        {
            settings.Reload();
        }

        /**
         * OTHER INPUTS
         */
        void checkBoxAutorun_CheckedChanged(object sender, EventArgs e)
        {
            settings.AutoStart = checkBoxAutorun.Checked;
        }

        void checkBoxRunAsAdmin_CheckedChanged(object sender, EventArgs e)
        {
            settings.RunAsAdmin = checkBoxRunAsAdmin.Checked;
        }

        private void checkBoxTrayIcon_CheckedChanged(object sender, EventArgs e)
        {
            settings.ShowTrayIcon = checkBoxTrayIcon.Checked;
        }

        void DisplaySwitchDelay(int delay)
        {
            textBoxDelay.Text = delay.ToString() + " ms";
        }

        private void textBoxDelay_TextChanged(object sender, EventArgs e)
        {
            short delay = 0;
            if (!Int16.TryParse(Regex.Replace(textBoxDelay.Text, "[^0-9]", ""), out delay) || delay < 1)
            {
                delay = 1;
            }
            settings.SwitchDelay = delay;
            DisplaySwitchDelay(delay);
        }

        /**
         * MISC
         */
        void OnExit()
        {
            engine.Stop();
            if (Exit != null)
            {
                Exit(this, null);
            }
        }

        private void buttonGithub_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/wakeupich/dotSwitcher/issues");
        }

        private void label5_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show("Click the box and press any key (e.g. Right Ctrl, Left Shift, Caps Lock) or Backspace to clear", textBoxSwitchLayoutHotkey);
        }

        private void label5_MouseLeave(object sender, EventArgs e)
        {
            toolTip1.Hide(label5);
        }

        private void labelSmartSelectionHelp_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show(
                "When enabled, 'Convert last word' (Pause) converts the entire typed phrase (multiple words separated by spaces).\nWhen disabled, space resets tracking so only the single last word is converted.",
                checkBoxSmartSelection);
        }

        private void labelSmartSelectionHelp_MouseLeave(object sender, EventArgs e)
        {
            toolTip1.Hide(checkBoxSmartSelection);
        }

        private void labelAutoSwitchHelp_MouseHover(object sender, EventArgs e)
        {
            toolTip1.Show(
                "Automatically detects words typed in the wrong keyboard layout (using Windows Spell Checker + character N-gram model)\nand converts them on Space or a brief pause. Press Pause or Backspace right after to revert and ignore the word.",
                checkBoxAutoSwitch);
        }

        private void labelAutoSwitchHelp_MouseLeave(object sender, EventArgs e)
        {
            toolTip1.Hide(checkBoxAutoSwitch);
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {
        }

        private void smartSelection_CheckedChanged(object sender, EventArgs e)
        {
            settings.SmartSelection = checkBoxSmartSelection.Checked;
        }

        private void checkBoxAutoSwitch_CheckedChanged(object sender, EventArgs e)
        {
            settings.AutoSwitch = checkBoxAutoSwitch.Checked;
        }

        private void radioLayouts_CheckedChanged(object sender, EventArgs e)
        {
            if (isUpdatingLayoutsUi) return;
            if (radioAllLayouts.Checked)
            {
                isUpdatingLayoutsUi = true;
                try
                {
                    for (int i = 0; i < checkedListBoxLayouts.Items.Count; i++)
                    {
                        checkedListBoxLayouts.SetItemChecked(i, true);
                    }
                }
                finally
                {
                    isUpdatingLayoutsUi = false;
                }
                settings.FilterLayouts = false;
                settings.EnabledLayouts = "";
            }
        }

        private void checkedListBoxLayouts_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (isUpdatingLayoutsUi) return;
            var selectedIds = new List<string>();
            for (int i = 0; i < checkedListBoxLayouts.Items.Count; i++)
            {
                bool isChecked = (i == e.Index)
                    ? (e.NewValue == CheckState.Checked)
                    : checkedListBoxLayouts.GetItemChecked(i);
                if (isChecked)
                {
                    var item = checkedListBoxLayouts.Items[i] as LayoutItem;
                    if (item != null)
                    {
                        selectedIds.Add(item.Id);
                    }
                }
            }

            isUpdatingLayoutsUi = true;
            try
            {
                if (selectedIds.Count < checkedListBoxLayouts.Items.Count)
                {
                    radioSelectedLayouts.Checked = true;
                    radioAllLayouts.Checked = false;
                    settings.FilterLayouts = true;
                    settings.EnabledLayouts = string.Join(",", selectedIds.ToArray());
                }
                else
                {
                    radioAllLayouts.Checked = true;
                    radioSelectedLayouts.Checked = false;
                    settings.FilterLayouts = false;
                    settings.EnabledLayouts = "";
                }
            }
            finally
            {
                isUpdatingLayoutsUi = false;
            }
        }
    }
}
