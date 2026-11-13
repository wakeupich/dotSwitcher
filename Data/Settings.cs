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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Xml.Linq;

namespace dotSwitcher.Data
{
    [Serializable]
    public sealed class Settings : ISettings
    {
        public static string SettingsDirectory
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
            get { return Path.Combine(SettingsDirectory, "settings.xml"); }
        }

        public KeyboardEventArgs SwitchHotkey { get; set; }
        public KeyboardEventArgs ConvertSelectionHotkey { get; set; }
        public KeyboardEventArgs SwitchLayoutHotkey { get; set; }
        public bool? SwitchLayoutInitialized { get; set; }
        public bool? AutoStart { get; set; }
        public bool? RunAsAdmin { get; set; }
        public bool? ShowTrayIcon { get; set; }
        public int SwitchDelay { get; set; }
        public bool? SmartSelection { get; set; }
        public bool? AutoSwitch { get; set; }
        public bool? AutoUpdate { get; set; }
        public bool? FilterLayouts { get; set; }
        public string EnabledLayouts { get; set; }
        public string Language { get; set; }

        public Settings()
        {
            SwitchHotkey = KeyboardEventArgs.Empty;
            ConvertSelectionHotkey = KeyboardEventArgs.Empty;
            SwitchLayoutHotkey = KeyboardEventArgs.Empty;
            EnabledLayouts = "";
        }

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
            if (settings.AutoUpdate == null)
            {
                settings.AutoUpdate = true;
            }
            if (settings.AutoStart == null)
            {
                settings.AutoStart = LowLevelAdapter.IsAutorunTaskExists() || LowLevelAdapter.IsAutorunShortcutExists();
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
            if (settings.EnabledLayouts == null)
            {
                settings.EnabledLayouts = "";
            }
            if (string.IsNullOrWhiteSpace(settings.Language))
            {
                string uiTwoLetter = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                settings.Language = string.Equals(uiTwoLetter, "ru", StringComparison.OrdinalIgnoreCase) ? "ru" : "en";
            }

            settings.Save();
            return settings;
        }

        public void Reload()
        {
            ResetToNulls();

            string path = SettingsFilePath;
            if (File.Exists(path))
            {
                try
                {
                    LoadFromXml(path);
                    return;
                }
                catch
                {
                    // Fall back to legacy migration or defaults if settings.xml is corrupted
                }
            }

            TryMigrateFromLegacyUserConfig();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsDirectory);

                var root = new XElement("Settings",
                    SerializeHotkey("SwitchHotkey", SwitchHotkey),
                    SerializeHotkey("ConvertSelectionHotkey", ConvertSelectionHotkey),
                    SerializeHotkey("SwitchLayoutHotkey", SwitchLayoutHotkey),
                    SerializeNullableBool("SwitchLayoutInitialized", SwitchLayoutInitialized),
                    SerializeNullableBool("AutoStart", AutoStart),
                    SerializeNullableBool("RunAsAdmin", RunAsAdmin),
                    SerializeNullableBool("ShowTrayIcon", ShowTrayIcon),
                    new XElement("SwitchDelay", SwitchDelay),
                    SerializeNullableBool("SmartSelection", SmartSelection),
                    SerializeNullableBool("AutoSwitch", AutoSwitch),
                    SerializeNullableBool("AutoUpdate", AutoUpdate),
                    SerializeNullableBool("FilterLayouts", FilterLayouts),
                    new XElement("EnabledLayouts", EnabledLayouts ?? ""),
                    new XElement("Language", Language ?? "")
                );

                var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
                string targetPath = SettingsFilePath;
                string tempPath = targetPath + ".tmp";
                doc.Save(tempPath);

                if (File.Exists(targetPath))
                {
                    File.Copy(tempPath, targetPath, true);
                    File.Delete(tempPath);
                }
                else
                {
                    File.Move(tempPath, targetPath);
                }
            }
            catch { }
        }

        private void ResetToNulls()
        {
            SwitchHotkey = null;
            ConvertSelectionHotkey = null;
            SwitchLayoutHotkey = null;
            SwitchLayoutInitialized = null;
            AutoStart = null;
            RunAsAdmin = null;
            ShowTrayIcon = null;
            SwitchDelay = 0;
            SmartSelection = null;
            AutoSwitch = null;
            AutoUpdate = null;
            FilterLayouts = null;
            EnabledLayouts = "";
            Language = "";
        }

        private static XElement SerializeHotkey(string name, KeyboardEventArgs hk)
        {
            var effective = hk ?? KeyboardEventArgs.Empty;
            return new XElement(name,
                new XAttribute("KeyData", effective.KeyData.ToString()),
                new XAttribute("Win", effective.Win ? "true" : "false"));
        }

        private static KeyboardEventArgs DeserializeHotkey(XElement el)
        {
            if (el == null) return null;
            var keyAttr = el.Attribute("KeyData");
            var winAttr = el.Attribute("Win");
            if (keyAttr == null) return null;

            Keys keyData;
            if (!Enum.TryParse(keyAttr.Value, true, out keyData))
            {
                int rawInt;
                if (int.TryParse(keyAttr.Value, out rawInt))
                {
                    keyData = (Keys)rawInt;
                }
                else
                {
                    return null;
                }
            }

            bool win = false;
            if (winAttr != null)
            {
                bool.TryParse(winAttr.Value, out win);
            }
            return new KeyboardEventArgs(keyData, win);
        }

        private static XElement SerializeNullableBool(string name, bool? val)
        {
            return new XElement(name, val.HasValue ? (val.Value ? "true" : "false") : "");
        }

        private static bool? DeserializeNullableBool(XElement el)
        {
            if (el == null || string.IsNullOrWhiteSpace(el.Value)) return null;
            bool parsed;
            return bool.TryParse(el.Value.Trim(), out parsed) ? (bool?)parsed : null;
        }

        private void LoadFromXml(string path)
        {
            var doc = XDocument.Load(path);
            var root = doc.Root;
            if (root == null) return;

            SwitchHotkey = DeserializeHotkey(root.Element("SwitchHotkey"));
            ConvertSelectionHotkey = DeserializeHotkey(root.Element("ConvertSelectionHotkey"));
            SwitchLayoutHotkey = DeserializeHotkey(root.Element("SwitchLayoutHotkey"));
            SwitchLayoutInitialized = DeserializeNullableBool(root.Element("SwitchLayoutInitialized"));
            AutoStart = DeserializeNullableBool(root.Element("AutoStart"));
            RunAsAdmin = DeserializeNullableBool(root.Element("RunAsAdmin"));
            ShowTrayIcon = DeserializeNullableBool(root.Element("ShowTrayIcon"));

            var delayEl = root.Element("SwitchDelay");
            int delayVal;
            if (delayEl != null && int.TryParse(delayEl.Value, out delayVal))
            {
                SwitchDelay = delayVal;
            }

            SmartSelection = DeserializeNullableBool(root.Element("SmartSelection"));
            AutoSwitch = DeserializeNullableBool(root.Element("AutoSwitch"));
            AutoUpdate = DeserializeNullableBool(root.Element("AutoUpdate"));
            FilterLayouts = DeserializeNullableBool(root.Element("FilterLayouts"));

            var enabledEl = root.Element("EnabledLayouts");
            EnabledLayouts = enabledEl != null ? (enabledEl.Value ?? "") : "";

            var langEl = root.Element("Language");
            Language = langEl != null ? (langEl.Value ?? "") : "";
        }

        private void TryMigrateFromLegacyUserConfig()
        {
            try
            {
                if (!Directory.Exists(SettingsDirectory)) return;

                var legacyFile = Directory
                    .GetFiles(SettingsDirectory, "user.config", SearchOption.AllDirectories)
                    .Select(p => new FileInfo(p))
                    .OrderByDescending(f => f.LastWriteTimeUtc)
                    .FirstOrDefault();

                if (legacyFile == null || !legacyFile.Exists) return;

                var doc = XDocument.Load(legacyFile.FullName);
                var settingsNode = doc.Descendants("dotSwitcher.Data.Settings").FirstOrDefault();
                if (settingsNode == null) return;

                var formatter = new BinaryFormatter
                {
                    Binder = new LegacySettingsBinder()
                };

                foreach (var settingEl in settingsNode.Elements("setting"))
                {
                    var nameAttr = settingEl.Attribute("name");
                    var serAttr = settingEl.Attribute("serializeAs");
                    var valueEl = settingEl.Element("value");
                    if (nameAttr == null || valueEl == null) continue;

                    string name = nameAttr.Value;
                    string rawVal = valueEl.Value;
                    if (string.IsNullOrEmpty(rawVal)) continue;

                    object decoded = null;
                    if (serAttr != null && string.Equals(serAttr.Value, "Binary", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            byte[] bytes = Convert.FromBase64String(rawVal.Trim());
                            using (var ms = new MemoryStream(bytes))
                            {
                                decoded = formatter.Deserialize(ms);
                            }
                        }
                        catch
                        {
                            decoded = null;
                        }
                    }
                    else
                    {
                        decoded = rawVal;
                    }

                    if (decoded == null) continue;

                    switch (name)
                    {
                        case "SwitchHotkey":
                            SwitchHotkey = decoded as KeyboardEventArgs;
                            break;
                        case "ConvertSelectionHotkey":
                            ConvertSelectionHotkey = decoded as KeyboardEventArgs;
                            break;
                        case "SwitchLayoutHotkey":
                            SwitchLayoutHotkey = decoded as KeyboardEventArgs;
                            break;
                        case "SwitchLayoutInitialized":
                            if (decoded is bool) SwitchLayoutInitialized = (bool)decoded;
                            break;
                        case "AutoStart":
                            if (decoded is bool) AutoStart = (bool)decoded;
                            break;
                        case "RunAsAdmin":
                            if (decoded is bool) RunAsAdmin = (bool)decoded;
                            break;
                        case "ShowTrayIcon":
                            if (decoded is bool) ShowTrayIcon = (bool)decoded;
                            break;
                        case "SwitchDelay":
                            if (decoded is int) SwitchDelay = (int)decoded;
                            break;
                        case "SmartSelection":
                            if (decoded is bool) SmartSelection = (bool)decoded;
                            break;
                        case "AutoSwitch":
                            if (decoded is bool) AutoSwitch = (bool)decoded;
                            break;
                        case "FilterLayouts":
                            if (decoded is bool) FilterLayouts = (bool)decoded;
                            break;
                        case "EnabledLayouts":
                            EnabledLayouts = decoded.ToString();
                            break;
                    }
                }
            }
            catch { }
        }

        private sealed class LegacySettingsBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                if (string.Equals(typeName, "dotSwitcher.Data.KeyboardEventArgs", StringComparison.Ordinal))
                {
                    return typeof(KeyboardEventArgs);
                }
                return Type.GetType(string.Format("{0}, {1}", typeName, assemblyName), false);
            }
        }
    }
}
