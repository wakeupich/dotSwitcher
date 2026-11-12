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

using dotSwitcher.WinApi;
using System;
using System.Configuration;
using System.Windows.Forms;

namespace dotSwitcher.Data
{
    [Serializable]
    public sealed class Settings : ApplicationSettingsBase, ISettings
    {
        public static Settings Init()
        {
            var settings = new Settings();
            settings.Reload();
            if (settings.SwitchHotkey == null || settings.SwitchHotkey.KeyData == Keys.None)
            {
                settings.SwitchHotkey = new KeyboardEventArgs(Keys.Pause, false);
            }
            if (settings.ConvertSelectionHotkey == null || settings.ConvertSelectionHotkey.KeyData == Keys.None)
            {
                settings.ConvertSelectionHotkey = new KeyboardEventArgs(Keys.Pause | Keys.Shift, false);
            }
            if (settings.SwitchLayoutHotkey == null ||
                (settings.SwitchLayoutHotkey.KeyData == Keys.None && settings.SwitchLayoutInitialized != true))
            {
                settings.SwitchLayoutHotkey = new KeyboardEventArgs(Keys.RControlKey, false);
                settings.SwitchLayoutInitialized = true;
            }
            if (settings.ShowTrayIcon == null)
            {
                settings.ShowTrayIcon = true;
            }
            if (settings.SmartSelection == null)
            {
                settings.SmartSelection = true;
            }
            if (settings.AutoSwitch == null)
            {
                settings.AutoSwitch = true;
            }
            if (settings.RunAsAdmin == null)
            {
                settings.RunAsAdmin = LowLevelAdapter.IsAutorunTaskExists() || LowLevelAdapter.IsRunningAsAdmin();
            }
            if (settings.FilterLayouts == null)
            {
                settings.FilterLayouts = false;
            }
            if (settings.SwitchDelay < 1)
            {
                settings.SwitchDelay = 20;
            }
            settings.Save();
            return settings;
        }

        [UserScopedSetting]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        [DefaultSettingValue("")]
        public KeyboardEventArgs SwitchHotkey
        {
            get
            {
                return (KeyboardEventArgs)this["SwitchHotkey"] ?? KeyboardEventArgs.Empty;
            }
            set
            {
                this["SwitchHotkey"] = (KeyboardEventArgs)value; 
            }
        }

        [UserScopedSetting]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        [DefaultSettingValue("")]
        public KeyboardEventArgs ConvertSelectionHotkey
        {
            get
            {
                return (KeyboardEventArgs)this["ConvertSelectionHotkey"] ?? KeyboardEventArgs.Empty;
            }
            set
            {
                this["ConvertSelectionHotkey"] = (KeyboardEventArgs)value;
            }
        }

        [UserScopedSetting]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        [DefaultSettingValue("")]
        public KeyboardEventArgs SwitchLayoutHotkey
        {
            get
            {
                return (KeyboardEventArgs)this["SwitchLayoutHotkey"] ?? KeyboardEventArgs.Empty;
            }
            set
            {
                this["SwitchLayoutHotkey"] = (KeyboardEventArgs)value;
            }
        }

        [UserScopedSetting]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        [DefaultSettingValue("")]
        public bool? SwitchLayoutInitialized
        {
            get
            {
                return (bool?)this["SwitchLayoutInitialized"];
            }
            set
            {
                this["SwitchLayoutInitialized"] = (bool?)value;
            }
        }

        [UserScopedSetting]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        [DefaultSettingValue("")]
        public bool? AutoStart
        {
            get
            {
                return (bool?)this["AutoStart"];
            }
            set
            {
                this["AutoStart"] = (bool?)value;
            }
        }

        [UserScopedSetting]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        [DefaultSettingValue("")]
        public bool? RunAsAdmin
        {
            get
            {
                return (bool?)this["RunAsAdmin"];
            }
            set
            {
                this["RunAsAdmin"] = (bool?)value;
            }
        }

        [UserScopedSetting]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        [DefaultSettingValue("")]
        public bool? ShowTrayIcon
        {
            get
            {
                return (bool?)this["ShowTrayIcon"];
            }
            set
            {
                this["ShowTrayIcon"] = (bool?)value;
            }
        }

        [UserScopedSetting]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        [DefaultSettingValue("")]
        public int SwitchDelay
        {
            get
            {
                return (int)this["SwitchDelay"];
            }
            set
            {
                this["SwitchDelay"] = (int)value;
            }
        }

        [UserScopedSetting]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        [DefaultSettingValue("")]
        public bool? SmartSelection
        {
            get
            {
                return (bool?)this["SmartSelection"];
            }
            set
            {
                this["SmartSelection"] = (bool?)value;
            }
        }

        [UserScopedSetting]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        [DefaultSettingValue("")]
        public bool? AutoSwitch
        {
            get
            {
                return (bool?)this["AutoSwitch"];
            }
            set
            {
                this["AutoSwitch"] = (bool?)value;
            }
        }

        [UserScopedSetting]
        [SettingsSerializeAs(SettingsSerializeAs.Binary)]
        [DefaultSettingValue("")]
        public bool? FilterLayouts
        {
            get
            {
                return (bool?)this["FilterLayouts"];
            }
            set
            {
                this["FilterLayouts"] = (bool?)value;
            }
        }

        [UserScopedSetting]
        [SettingsSerializeAs(SettingsSerializeAs.String)]
        [DefaultSettingValue("")]
        public string EnabledLayouts
        {
            get
            {
                return (string)this["EnabledLayouts"] ?? "";
            }
            set
            {
                this["EnabledLayouts"] = value ?? "";
            }
        }
    }
}
