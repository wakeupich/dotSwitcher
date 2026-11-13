using System;
using System.Collections.Generic;

namespace dotSwitcher.UI
{
    public static class Loc
    {
        public static string CurrentLang { get; set; }

        static Loc()
        {
            CurrentLang = "en";
        }

        private static readonly Dictionary<string, string[]> Strings = new Dictionary<string, string[]>
        {
            // Form titles
            { "formTitle", new[] { "dotSwitcher v{0} Settings", "dotSwitcher v{0} — Настройки" } },
            { "formTitleAdmin", new[] { "dotSwitcher v{0} Settings (Administrator)", "dotSwitcher v{0} — Настройки (Администратор)" } },

            // Group boxes
            { "groupGeneral", new[] { "General && Updates", "Основные и обновления" } },
            { "groupBehavior", new[] { "Typing && Switching", "Набор и переключение" } },
            { "groupHotkeys", new[] { "Hotkeys", "Горячие клавиши" } },
            { "groupLayouts", new[] { "Keyboard layouts in cycle", "Раскладки в цикле" } },

            // General
            { "autorun", new[] { "Start on Windows startup", "Запускать вместе с Windows" } },
            { "runAsAdmin", new[] { "Run as admin (Task Scheduler)", "Запуск от администратора" } },
            { "trayIcon", new[] { "Show tray icon", "Показывать значок в трее" } },
            { "autoUpdate", new[] { "Auto-update from GitHub", "Автообновление с GitHub" } },
            { "language", new[] { "Language:", "Язык:" } },
            { "checkUpdate", new[] { "Check for updates", "Проверить обновления" } },
            { "checking", new[] { "Checking...", "Проверка..." } },
            { "updating", new[] { "Updating...", "Обновление..." } },

            // Behavior
            { "autoSwitch", new[] { "Auto-switch layout while typing", "Автопереключение при наборе" } },
            { "smartSelection", new[] { "Use smart convert selection", "Умный захват фразы" } },
            { "switchDelay", new[] { "Delay before switching:", "Задержка переключения:" } },
            { "delayHint", new[] { "(1–200 ms)", "(1–200 мс)" } },

            // Hotkeys
            { "convertWord", new[] { "Convert last word:", "Исправить последнее слово:" } },
            { "convertSel", new[] { "Convert selection:", "Исправить выделенный текст:" } },
            { "switchLayout", new[] { "Switch keyboard layout:", "Переключить раскладку:" } },

            // Layouts
            { "allLayouts", new[] { "All installed", "Все" } },
            { "selectedLayouts", new[] { "Only selected:", "Выбранные:" } },

            // Bottom buttons
            { "exitProgram", new[] { "Exit", "Выход" } },
            { "reportIssue", new[] { "Report an issue", "Сообщить об ошибке" } },
            { "apply", new[] { "Apply", "Применить" } },
            { "cancel", new[] { "Cancel", "Отмена" } },

            // Tooltips
            {
                "tipSwitchLayout",
                new[]
                {
                    "Click the box and press any key (e.g. Right Ctrl, Left Shift, Caps Lock) or Backspace to clear.",
                    "Кликните по полю и нажмите нужную клавишу (например, Right Ctrl, Left Shift, Caps Lock) или Backspace для очистки."
                }
            },
            {
                "tipSmartSelection",
                new[]
                {
                    "When enabled, 'Convert last word' (Pause) converts the entire typed phrase (multiple words separated by spaces).\nWhen disabled, Space resets tracking so only the single last word is converted.",
                    "Когда включено, клавиша исправления (Pause) переводит всю набранную фразу целиком (несколько слов через пробел).\nКогда выключено, пробел сбрасывает буфер — исправляется только последнее слово."
                }
            },
            {
                "tipAutoSwitch",
                new[]
                {
                    "Automatically detects words typed in the wrong keyboard layout (Windows Spell Checker + N-gram model)\nand converts them on Space or a brief pause. Press Pause or Backspace right after to revert and ignore the word.",
                    "Автоматически распознаёт слова, набранные в неверной раскладке (словарь Windows + N-граммная модель),\nи исправляет по пробелу или короткой паузе. Нажмите Pause или Backspace сразу после срабатывания, чтобы отменить и запомнить слово как исключение."
                }
            },

            // Tray menu
            { "trayTurnOff", new[] { "Turn off", "Выключить" } },
            { "trayTurnOn", new[] { "Turn on", "Включить" } },
            { "trayAutoSwitch", new[] { "Auto-switch layout", "Автопереключение раскладки" } },
            { "traySettings", new[] { "Settings", "Настройки" } },
            { "trayExit", new[] { "Exit", "Выход" } },
            { "trayError", new[] { "dotSwitcher error", "Ошибка dotSwitcher" } },
            { "trayUpdated", new[] { "Updated to v{0}", "Обновлено до версии v{0}" } },

            // Messages
            {
                "msgWinAltUnsupported",
                new[]
                {
                    "Sorry, Win+ and Alt+ hotkeys are not supported yet for 'Convert last word'.",
                    "Комбинации с Win+ и Alt+ пока не поддерживаются для исправления последнего слова."
                }
            },
            {
                "msgSelectAtLeastOneLayout",
                new[]
                {
                    "Please select at least one keyboard layout or choose 'All installed'.",
                    "Выберите хотя бы одну раскладку или отметьте «Все»."
                }
            },
            {
                "msgRestartAdmin",
                new[]
                {
                    "Restart dotSwitcher as Administrator now so hotkeys also work in elevated windows?",
                    "Перезапустить dotSwitcher с правами администратора прямо сейчас, чтобы горячие клавиши и автопереключение работали во всех программах?"
                }
            },
            {
                "msgUpdateCheckFailed",
                new[]
                {
                    "Failed to check for updates:\n{0}",
                    "Не удалось проверить обновления:\n{0}"
                }
            },
            {
                "msgLatestVersion",
                new[]
                {
                    "You are using the latest version (v{0}).",
                    "У вас уже установлена последняя версия (v{0})."
                }
            },
            {
                "msgUpdateAvailable",
                new[]
                {
                    "New version v{0} is available (current: v{1}).\nDownload and install update now?",
                    "Доступна новая версия v{0} (текущая: v{1}).\nСкачать и установить обновление сейчас?"
                }
            },
            {
                "msgUpdateFailed",
                new[]
                {
                    "Update failed:\n{0}",
                    "Ошибка при обновлении:\n{0}"
                }
            },
            { "msgUpdateTitle", new[] { "dotSwitcher Update", "Обновление dotSwitcher" } }
        };

        public static string Get(string key)
        {
            string[] pair;
            if (!Strings.TryGetValue(key, out pair) || pair.Length == 0)
            {
                return key;
            }
            bool isRu = string.Equals(CurrentLang, "ru", StringComparison.OrdinalIgnoreCase);
            return isRu && pair.Length > 1 ? pair[1] : pair[0];
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }
    }
}
