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
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace dotSwitcher.Data
{
    public enum KeyboardEventType { Unknown, KeyDown, KeyUp }
    [Serializable]
    public class KeyboardEventArgs : KeyEventArgs, ISerializable
    {
        public KeyboardEventArgs() : base(Keys.None) { }
        public bool Win { get; set; }

        public KeyboardEventType Type;

        public static Keys NormalizeKeyData(Keys keyData)
        {
            var vk = keyData & Keys.KeyCode;
            if (vk == Keys.ControlKey || vk == Keys.LControlKey || vk == Keys.RControlKey)
            {
                keyData &= ~Keys.Control;
            }
            if (vk == Keys.ShiftKey || vk == Keys.LShiftKey || vk == Keys.RShiftKey)
            {
                keyData &= ~Keys.Shift;
            }
            if (vk == Keys.Menu || vk == Keys.LMenu || vk == Keys.RMenu)
            {
                keyData &= ~Keys.Alt;
            }
            return keyData;
        }

        public KeyboardEventArgs(SerializationInfo info, StreamingContext context)
            : base(NormalizeKeyData((Keys)info.GetValue("keyData", typeof(Keys))))
        {
            Win = (bool)info.GetValue("winMod", typeof(bool));
            if (KeyCode == Keys.LWin || KeyCode == Keys.RWin)
            {
                Win = false;
            }
            Type = KeyboardEventType.KeyDown;
        }
        public KeyboardEventArgs(Keys keyData, bool isWinDown, KeyboardEventType type = KeyboardEventType.KeyDown)
            : base(NormalizeKeyData(keyData))
        {
            Win = (KeyCode == Keys.LWin || KeyCode == Keys.RWin) ? false : isWinDown;
            Type = type;
        }
        public new static readonly KeyboardEventArgs Empty = new KeyboardEventArgs(Keys.None, false, KeyboardEventType.Unknown);

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var p = obj as KeyboardEventArgs;
            if (p == null)
            {
                return false;
            }
            var normThis = NormalizeKeyData(KeyData);
            var normOther = NormalizeKeyData(p.KeyData);
            var equals = normOther == normThis &&
                p.KeyCode == KeyCode &&
                p.Win == Win;

            return equals;
        }

        public override int GetHashCode()
        {
            return (int)(base.GetHashCode() ^ Win.GetHashCode());
        }

        public bool HasModifiers()
        {
            return Modifiers != Keys.None || Win;
        }

        private static string FormatKeyCode(Keys keyCode)
        {
            switch (keyCode)
            {
                case Keys.LControlKey:
                    return "Left Ctrl";
                case Keys.RControlKey:
                    return "Right Ctrl";
                case Keys.ControlKey:
                    return "Ctrl";
                case Keys.LShiftKey:
                    return "Left Shift";
                case Keys.RShiftKey:
                    return "Right Shift";
                case Keys.ShiftKey:
                    return "Shift";
                case Keys.LMenu:
                    return "Left Alt";
                case Keys.RMenu:
                    return "Right Alt";
                case Keys.Menu:
                    return "Alt";
                case Keys.Cancel:
                case Keys.Pause:
                    return "Pause";
                case Keys.Capital:
                    return "Caps Lock";
                case Keys.Back:
                    return "Backspace";
                default:
                    return keyCode.ToString();
            }
        }

        public override string ToString()
        {
            if (KeyCode == Keys.None && !HasModifiers())
            {
                return "None";
            }

            var isCtrlSelf = KeyCode == Keys.ControlKey || KeyCode == Keys.LControlKey || KeyCode == Keys.RControlKey;
            var isShiftSelf = KeyCode == Keys.ShiftKey || KeyCode == Keys.LShiftKey || KeyCode == Keys.RShiftKey;
            var isAltSelf = KeyCode == Keys.Menu || KeyCode == Keys.LMenu || KeyCode == Keys.RMenu;
            var isWinSelf = KeyCode == Keys.LWin || KeyCode == Keys.RWin;

            var result =
                ((Control && !isCtrlSelf) ? "Control + " : "") +
                ((Shift && !isShiftSelf) ? "Shift + " : "") +
                ((Alt && !isAltSelf) ? "Alt + " : "") +
                ((Win && !isWinSelf) ? "Win + " : "") +
                FormatKeyCode(KeyCode);

            return result;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("keyData", NormalizeKeyData(KeyData), typeof(Keys));
            info.AddValue("winMod", Win, typeof(bool));
        }
    }
}
