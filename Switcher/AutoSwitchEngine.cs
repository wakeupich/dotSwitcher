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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace dotSwitcher.Switcher
{
    public struct AutoSwitchDecision
    {
        public bool ShouldSwitch;
        public IntPtr TargetHkl;
        public int ReplaceKeyCount;
        public List<KeyboardEventArgs> KeysToRetype;
        public string OriginalWordKey;
    }

    public static class AutoSwitchEngine
    {
        #region Windows Native Spell Checker COM Interop

        [ComImport, Guid("8E018A9D-2415-4677-BF08-794EA61F94BB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ISpellCheckerFactory
        {
            IntPtr SupportedLanguages { get; }
            int IsSupported([MarshalAs(UnmanagedType.LPWStr)] string languageTag);
            [return: MarshalAs(UnmanagedType.Interface)]
            ISpellChecker CreateSpellChecker([MarshalAs(UnmanagedType.LPWStr)] string languageTag);
        }

        [ComImport, Guid("B6FD0B71-E2BC-4653-8D05-F197E412770B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ISpellChecker
        {
            string LanguageTag { [return: MarshalAs(UnmanagedType.LPWStr)] get; }
            [return: MarshalAs(UnmanagedType.Interface)]
            IEnumSpellingError Check([MarshalAs(UnmanagedType.LPWStr)] string text);
        }

        [ComImport, Guid("803E3BD4-2828-4410-8290-418D1D73C762"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IEnumSpellingError
        {
            [PreserveSig]
            int Next([MarshalAs(UnmanagedType.Interface)] out ISpellingError value);
        }

        [ComImport, Guid("B7C82D61-FBE8-4B47-9B27-6C0D2E0DE0A3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ISpellingError
        {
        }

        private static readonly object spellLock = new object();
        private static bool factoryInitialized = false;
        private static ISpellCheckerFactory spellFactory = null;
        private static readonly Dictionary<int, ISpellChecker> spellCheckers = new Dictionary<int, ISpellChecker>();
        private static readonly Dictionary<string, bool> spellCache = new Dictionary<string, bool>(StringComparer.Ordinal);

        #endregion

        #region Built-in Fast Lexicons & Phonotactic Data

        private static readonly HashSet<string> CommonEnTechWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "git", "ssh", "ping", "dir", "cls", "cmd", "npm", "pip", "scp", "wsl", "rdp", "vpn", "dns", "dhcp",
            "vlan", "tcp", "udp", "http", "https", "www", "ftp", "json", "yaml", "yml", "xml", "sql", "api",
            "url", "uri", "cli", "gui", "cpu", "gpu", "ram", "ssd", "hdd", "nas", "vps", "vds", "exe", "dll",
            "bat", "ps1", "cfg", "ini", "env", "log", "tmp", "usr", "var", "etc", "bin", "src", "dev", "prod",
            "test", "main", "root", "admin", "sudo", "grep", "curl", "wget", "nano", "vim", "cat", "chmod",
            "chown", "docker", "kubectl", "winget", "choco", "msbuild", "dotnet", "nuget", "github", "gitlab",
            "powershell", "windows", "linux", "ubuntu", "debian", "mikrotik", "zyxel", ***REMOVED***, ***REMOVED***,
            ***REMOVED***, "caliber", "ipconfig", "netstat", "tracert", "nslookup", "schtasks", "taskkill", "regedit",
            "localhost", "boolean", "string", "int", "void", "null", "true", "false", "async", "await", "return",
            "public", "private", "static", "class", "struct", "interface"
        };

        private static readonly HashSet<string> CommonRuWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "привет", "пока", "давай", "сейчас", "спасибо", "пожалуйста", "хорошо", "ладно", "понял", "поняла",
            "конечно", "вообще", "кстати", "короче", "просто", "очень", "только", "еще", "ещё", "уже", "тут",
            "там", "вот", "где", "как", "что", "кто", "это", "все", "всё", "нет", "да", "не", "ни", "ну", "но",
            "же", "ли", "бы", "то", "на", "по", "за", "из", "от", "до", "без", "для", "при", "про", "под", "над",
            "или", "если", "чтобы", "когда", "потому", "почему", "зачем", "какой", "такой", "мой", "твой", "свой",
            "наш", "ваш", "его", "ее", "её", "их", "мне", "тебе", "себе", "нам", "вам", "им", "меня", "тебя",
            "себя", "нас", "вас", "него", "нее", "них", "мной", "тобой", "собой", "нами", "вами", "ими",
            "я", "в", "и", "а", "с", "у", "о", "к", "он", "она", "оно", "они", "мы", "ты", "вы",
            "че", "чё", "ща", "щас", "спс", "пж", "плиз", "норм", "инфа", "инфу", "ава", "лвл", "го", "хз",
            "имхо", "изи", "топчик", "сервак", "админка", "деплой", "конфиг", "свитчер", "раскладка", "хоткей",
            "бэкап", "роутер", "флешка", "винда", "скрин", "скриншот", "скрипт", "коммит", "релиз", "приложуха",
            "аватарка", "видосик", "багулина", ***REMOVED***, ***REMOVED***, ***REMOVED***, ***REMOVED***, ***REMOVED***
        };

        // Short 2-3 letter words that exist in EN dictionary as rare/archaic/slang words
        // but correspond to extremely common RU words on the same physical keys
        private static readonly HashSet<string> AmbiguousShortEnWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ult", "tot", "ye", "yt", "jy", "yf", "ys", "ns", "ds", "vs", "ab"
        };

        private static readonly HashSet<string> AmbiguousShortRuWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "шт", "еу", "туе", "ащк", "фнд"
        };

        private static readonly char[] EdgePunctuation = new char[]
        {
            '.', ',', '!', '?', ':', ';', '"', '\'', '(', ')', '[', ']', '{', '}', '<', '>'
        };

        #endregion

        private static ISpellChecker GetSpellChecker(int langId)
        {
            lock (spellLock)
            {
                ISpellChecker existing;
                if (spellCheckers.TryGetValue(langId, out existing))
                {
                    return existing;
                }

                if (!factoryInitialized)
                {
                    factoryInitialized = true;
                    try
                    {
                        var t = Type.GetTypeFromCLSID(new Guid("7AB36653-1796-484B-BDFA-E74F1DB7C1DC"));
                        if (t != null)
                        {
                            spellFactory = (ISpellCheckerFactory)Activator.CreateInstance(t);
                        }
                    }
                    catch
                    {
                        spellFactory = null;
                    }
                }

                if (spellFactory == null)
                {
                    spellCheckers[langId] = null;
                    return null;
                }

                ISpellChecker created = null;
                try
                {
                    var ci = new CultureInfo(langId);
                    string tag = ci.Name; // e.g. "en-US", "ru-RU", "lv-LV"
                    if (spellFactory.IsSupported(tag) != 0)
                    {
                        created = spellFactory.CreateSpellChecker(tag);
                    }
                    else
                    {
                        string twoLetter = ci.TwoLetterISOLanguageName;
                        if (spellFactory.IsSupported(twoLetter) != 0)
                        {
                            created = spellFactory.CreateSpellChecker(twoLetter);
                        }
                    }
                }
                catch
                {
                    created = null;
                }

                spellCheckers[langId] = created;
                return created;
            }
        }

        private static string GetIsoLang(IntPtr hkl)
        {
            int langId = (int)(hkl.ToInt64() & 0xFFFF);
            if (langId == 0x0419) return "ru";
            if (langId == 0x0409 || (langId & 0xFF) == 0x09) return "en";
            if (langId == 0x0426) return "lv";
            try
            {
                return new CultureInfo(langId).TwoLetterISOLanguageName.ToLowerInvariant();
            }
            catch
            {
                return "en";
            }
        }

        private static bool HasInternalNonLetters(string word, string isoLang)
        {
            if (string.IsNullOrEmpty(word)) return true;
            for (int i = 0; i < word.Length; i++)
            {
                char c = word[i];
                if (!char.IsLetter(c))
                {
                    if (c == '-' && i > 0 && i < word.Length - 1)
                        continue;
                    if (isoLang == "en" && c == '\'' && i > 0 && i < word.Length - 1)
                        continue;
                    return true;
                }
            }
            return false;
        }

        public static bool IsDictionaryWord(IntPtr hkl, string word)
        {
            if (string.IsNullOrEmpty(word)) return false;
            string iso = GetIsoLang(hkl);

            // A valid word cannot contain internal punctuation like ',', '.', ';', '[', '`', '&'
            if (HasInternalNonLetters(word, iso))
            {
                return false;
            }

            if (iso == "en" && CommonEnTechWords.Contains(word))
            {
                return true;
            }
            if (iso == "ru" && CommonRuWords.Contains(word))
            {
                return true;
            }

            int langId = (int)(hkl.ToInt64() & 0xFFFF);
            string cacheKey = langId.ToString("X4") + ":" + word.ToLowerInvariant();

            lock (spellLock)
            {
                bool cached;
                if (spellCache.TryGetValue(cacheKey, out cached))
                {
                    return cached;
                }
            }

            var checker = GetSpellChecker(langId);
            if (checker == null)
            {
                return false;
            }

            bool isValid = false;
            lock (spellLock)
            {
                try
                {
                    var errEnum = checker.Check(word);
                    ISpellingError err;
                    int hr = errEnum.Next(out err);
                    if (err != null) Marshal.ReleaseComObject(err);
                    Marshal.ReleaseComObject(errEnum);
                    isValid = (hr == 1);

                    if (!isValid && word.Length >= 4 && char.IsLower(word[0]))
                    {
                        string cap = char.ToUpperInvariant(word[0]) + word.Substring(1);
                        var errEnum2 = checker.Check(cap);
                        ISpellingError err2;
                        int hr2 = errEnum2.Next(out err2);
                        if (err2 != null) Marshal.ReleaseComObject(err2);
                        Marshal.ReleaseComObject(errEnum2);
                        isValid = (hr2 == 1);
                    }
                }
                catch
                {
                    isValid = false;
                }

                if (spellCache.Count > 4096)
                {
                    spellCache.Clear();
                }
                spellCache[cacheKey] = isValid;
            }

            return isValid;
        }

        public static int ScoreWordModel(IntPtr hkl, string word, out bool isDictValid)
        {
            isDictValid = false;
            if (string.IsNullOrEmpty(word)) return -150;

            string iso = GetIsoLang(hkl);
            isDictValid = IsDictionaryWord(hkl, word);

            int score = isDictValid ? 65 : 0;
            int vowels = 0;
            int consonantsInRow = 0;
            int maxConsonantsInRow = 0;
            int internalPunct = 0;
            int scriptLetters = 0;
            int wrongScriptLetters = 0;
            string lower = word.ToLowerInvariant();

            string vowelSet = iso == "ru"
                ? "аеёиоуыэюя"
                : (iso == "lv" ? "aāeēiīouūy" : "aeiouy");

            for (int i = 0; i < lower.Length; i++)
            {
                char c = lower[i];
                if (!char.IsLetter(c))
                {
                    if (c != '-' && !(iso == "en" && c == '\'' && i > 0 && i < lower.Length - 1))
                    {
                        internalPunct++;
                    }
                    consonantsInRow = 0;
                    continue;
                }

                bool isCyrillic = (c >= 'а' && c <= 'я') || c == 'ё';
                if (iso == "ru")
                {
                    if (isCyrillic) scriptLetters++;
                    else wrongScriptLetters++;
                }
                else
                {
                    if (!isCyrillic) scriptLetters++;
                    else wrongScriptLetters++;
                }

                if (vowelSet.IndexOf(c) >= 0)
                {
                    vowels++;
                    consonantsInRow = 0;
                }
                else
                {
                    consonantsInRow++;
                    if (consonantsInRow > maxConsonantsInRow)
                    {
                        maxConsonantsInRow = consonantsInRow;
                    }
                }
            }

            if (wrongScriptLetters > 0) score -= wrongScriptLetters * 40;
            if (internalPunct > 0) score -= internalPunct * 45;

            if (lower.Length >= 2 && vowels == 0)
            {
                score -= 45;
            }
            else if (vowels > 0)
            {
                score += 15;
            }

            if (maxConsonantsInRow >= 4)
            {
                score -= (maxConsonantsInRow - 2) * 20;
            }
            else if (maxConsonantsInRow == 3 && lower.Length <= 4)
            {
                score -= 15;
            }

            if (iso == "ru")
            {
                if (lower[0] == 'ы' || lower[0] == 'ъ' || lower[0] == 'ь')
                {
                    score -= 60;
                }
                if (lower.Contains("щщ") || lower.Contains("ыы") || lower.Contains("уу") ||
                    lower.Contains("шш") || lower.Contains("жж") || lower.Contains("чч") ||
                    lower.Contains("йй") || (lower.Contains("ъ") && lower.EndsWith("ъ")))
                {
                    score -= 45;
                }
                string[] badRu = {
                    "ыа", "ыо", "ыу", "ыэ", "ыя", "ыю", "шы", "жы", "чя", "щя",
                    "щф", "щх", "щц", "щч", "щш", "цр", "штп", "ешщт", "щге", "фег", "уку"
                };
                for (int i = 0; i < badRu.Length; i++)
                {
                    if (lower.Contains(badRu[i])) score -= 25;
                }

                string[] goodRu = {
                    "пр", "ст", "ет", "ов", "ат", "ть", "ен", "ос", "ра", "ко",
                    "то", "на", "по", "не", "ка", "ся", "ого", "что", "как", "ер", "ит", "чк", "шк"
                };
                for (int i = 0; i < goodRu.Length; i++)
                {
                    if (lower.Contains(goodRu[i])) score += 10;
                }
            }
            else if (iso == "en")
            {
                for (int i = 0; i < lower.Length; i++)
                {
                    if (lower[i] == 'q' && (i == lower.Length - 1 || lower[i + 1] != 'u'))
                    {
                        score -= 35;
                    }
                    if (lower[i] == 'j' && i > 0)
                    {
                        score -= 20;
                    }
                }
                string[] badEn = {
                    "ghb", "xnj", "rfr", "ytn", "lfdf", "ctqx", "cgfc", "gjx", "k.,", "nt,", "'nj", "[jx"
                };
                for (int i = 0; i < badEn.Length; i++)
                {
                    if (lower.Contains(badEn[i])) score -= 40;
                }

                string[] goodEn = {
                    "th", "he", "in", "er", "an", "re", "on", "at", "en", "nd",
                    "ti", "es", "or", "te", "of", "ed", "is", "it", "al", "ar",
                    "st", "to", "nt", "ng", "ou", "ea", "ll", "oo", "ee", "ss"
                };
                for (int i = 0; i < goodEn.Length; i++)
                {
                    if (lower.Contains(goodEn[i])) score += 10;
                }
            }

            return score;
        }

        private struct WordSlice
        {
            public int StartIndex;
            public int Length;
            public List<KeyboardEventArgs> Keys;
        }

        private static List<WordSlice> SplitIntoWordSlices(IList<KeyboardEventArgs> selection)
        {
            var words = new List<WordSlice>();
            int i = 0;
            int n = selection.Count;
            while (i < n)
            {
                while (i < n && (selection[i].KeyCode & Keys.KeyCode) == Keys.Space)
                {
                    i++;
                }
                if (i >= n) break;

                int start = i;
                var list = new List<KeyboardEventArgs>();
                while (i < n && (selection[i].KeyCode & Keys.KeyCode) != Keys.Space)
                {
                    list.Add(selection[i]);
                    i++;
                }
                if (list.Count > 0)
                {
                    words.Add(new WordSlice
                    {
                        StartIndex = start,
                        Length = list.Count,
                        Keys = list
                    });
                }
            }
            return words;
        }

        private static bool ShouldSkipWordKeys(IList<KeyboardEventArgs> wordKeys)
        {
            if (wordKeys == null || wordKeys.Count == 0) return true;

            int upperCount = 0;
            int lowerCount = 0;
            bool upperAfterFirst = false;

            for (int i = 0; i < wordKeys.Count; i++)
            {
                var k = wordKeys[i];
                var vk = k.KeyCode & Keys.KeyCode;

                // Unshifted digits or numpad digits -> skip (passwords, versions, numbers)
                if ((vk >= Keys.D0 && vk <= Keys.D9 && !k.Shift) ||
                    (vk >= Keys.NumPad0 && vk <= Keys.NumPad9))
                {
                    return true;
                }

                if (vk >= Keys.A && vk <= Keys.Z)
                {
                    if (k.Shift)
                    {
                        upperCount++;
                        if (i > 0) upperAfterFirst = true;
                    }
                    else
                    {
                        lowerCount++;
                    }
                }
            }

            // Skip camelCase / PascalCase identifiers (e.g. dotSwitcher, GitHub)
            if (upperAfterFirst && lowerCount > 0)
            {
                return true;
            }

            // Skip ALLCAPS acronyms up to 5 letters (e.g. CPU, API, SQL, SSH)
            if (upperCount >= 2 && lowerCount == 0 && wordKeys.Count <= 5)
            {
                return true;
            }

            // Skip repetitive single-key or gaming WASD sequences (e.g. "www", "wasd", "asdasd")
            if (IsRepetitiveOrGamingSequence(wordKeys))
            {
                return true;
            }

            return false;
        }

        private static bool IsRepetitiveOrGamingSequence(IList<KeyboardEventArgs> wordKeys)
        {
            if (wordKeys.Count >= 3)
            {
                bool allSame = true;
                var firstVk = wordKeys[0].KeyCode & Keys.KeyCode;
                for (int i = 1; i < wordKeys.Count; i++)
                {
                    if ((wordKeys[i].KeyCode & Keys.KeyCode) != firstVk)
                    {
                        allSame = false;
                        break;
                    }
                }
                if (allSame) return true;
            }

            bool allWasd = true;
            for (int i = 0; i < wordKeys.Count; i++)
            {
                var vk = wordKeys[i].KeyCode & Keys.KeyCode;
                if (vk != Keys.W && vk != Keys.A && vk != Keys.S && vk != Keys.D)
                {
                    allWasd = false;
                    break;
                }
            }
            if (allWasd && wordKeys.Count >= 2)
            {
                return true;
            }

            return false;
        }

        private static string ExtractCoreWord(IList<KeyboardEventArgs> wordKeys, IntPtr hkl)
        {
            string raw = LowLevelAdapter.KeysToString(wordKeys, hkl);
            if (string.IsNullOrEmpty(raw)) return "";
            // Trim only trailing common sentence punctuation (?, !, ., ,) if word has at least 2 chars before it
            string trimmed = raw;
            while (trimmed.Length > 2)
            {
                char last = trimmed[trimmed.Length - 1];
                if (last == '?' || last == '!' || last == ',' || last == '.' || last == ':' || last == ';')
                {
                    trimmed = trimmed.Substring(0, trimmed.Length - 1);
                }
                else
                {
                    break;
                }
            }
            return trimmed;
        }

        private static bool IsAmbiguousShortWord(IntPtr hkl, string word)
        {
            string iso = GetIsoLang(hkl);
            if (iso == "en") return AmbiguousShortEnWords.Contains(word);
            if (iso == "ru") return AmbiguousShortRuWords.Contains(word);
            return false;
        }

        public static AutoSwitchDecision EvaluateAutoSwitch(
            IList<KeyboardEventArgs> currentSelection,
            bool triggeredBySpace,
            IntPtr currentHkl,
            IntPtr[] allowedLayouts,
            HashSet<string> ignoredWords)
        {
            var noSwitch = new AutoSwitchDecision { ShouldSwitch = false };

            if (currentSelection == null || currentSelection.Count == 0 ||
                allowedLayouts == null || allowedLayouts.Length < 2)
            {
                return noSwitch;
            }

            if (LowLevelAdapter.IsAutoSwitchBlockedWindow())
            {
                return noSwitch;
            }

            var words = SplitIntoWordSlices(currentSelection);
            if (words.Count == 0)
            {
                return noSwitch;
            }

            var lastSlice = words[words.Count - 1];
            int minLen = triggeredBySpace ? 2 : 4;
            if (lastSlice.Length < minLen)
            {
                return noSwitch;
            }

            if (ShouldSkipWordKeys(lastSlice.Keys))
            {
                return noSwitch;
            }

            string currentRaw = LowLevelAdapter.KeysToString(lastSlice.Keys, currentHkl);
            string currentWord = ExtractCoreWord(lastSlice.Keys, currentHkl);
            if (string.IsNullOrEmpty(currentWord))
            {
                return noSwitch;
            }

            if (ignoredWords != null && ignoredWords.Contains(currentWord.ToLowerInvariant()))
            {
                return noSwitch;
            }

            bool curDict;
            int curScore = ScoreWordModel(currentHkl, currentWord, out curDict);
            bool curIsAmbiguous = IsAmbiguousShortWord(currentHkl, currentWord);

            // Check if previous word exists for 2-word context disambiguation
            bool hasPrevWord = words.Count >= 2;
            WordSlice prevSlice = hasPrevWord ? words[words.Count - 2] : default(WordSlice);

            IntPtr bestTargetHkl = IntPtr.Zero;
            int bestTargetScore = int.MinValue;
            bool bestTargetDict = false;

            for (int i = 0; i < allowedLayouts.Length; i++)
            {
                IntPtr candidateHkl = allowedLayouts[i];
                if (string.Equals(LowLevelAdapter.GetLayoutId(candidateHkl), LowLevelAdapter.GetLayoutId(currentHkl), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string candidateRaw = LowLevelAdapter.KeysToString(lastSlice.Keys, candidateHkl);
                if (string.Equals(candidateRaw, currentRaw, StringComparison.Ordinal))
                {
                    // Same characters produced on both layouts (e.g. EN US vs LV QWERTY basic keys)
                    continue;
                }

                string candidateWord = ExtractCoreWord(lastSlice.Keys, candidateHkl);
                if (string.IsNullOrEmpty(candidateWord))
                {
                    continue;
                }

                bool tgtDict;
                int tgtScore = ScoreWordModel(candidateHkl, candidateWord, out tgtDict);

                bool canSwitchSingleWord = false;
                if (!curDict)
                {
                    if (tgtDict && tgtScore > curScore)
                    {
                        // For 2-letter words without previous word context, require clear phonotactic rejection in current layout
                        if (lastSlice.Length >= 3 || curScore <= -20)
                        {
                            canSwitchSingleWord = true;
                        }
                    }
                    else if (!tgtDict && lastSlice.Length >= 4 && curScore <= -15 && tgtScore >= 15 && (tgtScore - curScore) >= 45)
                    {
                        canSwitchSingleWord = true;
                    }
                }

                bool canSwitchTwoWordPhrase = false;
                if (!canSwitchSingleWord && hasPrevWord && !ShouldSkipWordKeys(prevSlice.Keys))
                {
                    string prevCurWord = ExtractCoreWord(prevSlice.Keys, currentHkl);
                    string prevTgtWord = ExtractCoreWord(prevSlice.Keys, candidateHkl);
                    bool prevTgtDict = IsDictionaryWord(candidateHkl, prevTgtWord);
                    bool prevCurDict = IsDictionaryWord(currentHkl, prevCurWord);
                    bool prevCurAmbiguous = IsAmbiguousShortWord(currentHkl, prevCurWord) || prevSlice.Length <= 2;

                    if (prevTgtDict && tgtDict &&
                        (!curDict || curIsAmbiguous) &&
                        (!prevCurDict || prevCurAmbiguous))
                    {
                        canSwitchTwoWordPhrase = true;
                    }
                }

                if ((canSwitchSingleWord || canSwitchTwoWordPhrase) && tgtScore > bestTargetScore)
                {
                    bestTargetScore = tgtScore;
                    bestTargetHkl = candidateHkl;
                    bestTargetDict = tgtDict;
                }
            }

            if (bestTargetHkl == IntPtr.Zero)
            {
                return noSwitch;
            }

            // Retroactive prefix expansion: include preceding short/unconverted words in currentSelection
            // if they also form valid dictionary words in bestTargetHkl
            int replaceStartIndex = lastSlice.StartIndex;
            for (int wIdx = words.Count - 2; wIdx >= 0; wIdx--)
            {
                var wSlice = words[wIdx];
                if (ShouldSkipWordKeys(wSlice.Keys))
                {
                    break;
                }
                string wCur = ExtractCoreWord(wSlice.Keys, currentHkl);
                string wTgt = ExtractCoreWord(wSlice.Keys, bestTargetHkl);
                bool wTgtDict = IsDictionaryWord(bestTargetHkl, wTgt);
                bool wCurDict = IsDictionaryWord(currentHkl, wCur);
                bool wCurAmbiguous = IsAmbiguousShortWord(currentHkl, wCur) || wSlice.Length <= 2;

                if (wTgtDict && (!wCurDict || wCurAmbiguous))
                {
                    replaceStartIndex = wSlice.StartIndex;
                }
                else
                {
                    break;
                }
            }

            int replaceCount = currentSelection.Count - replaceStartIndex;
            var retypeList = new List<KeyboardEventArgs>(replaceCount + 1);
            for (int i = replaceStartIndex; i < currentSelection.Count; i++)
            {
                retypeList.Add(currentSelection[i]);
            }

            return new AutoSwitchDecision
            {
                ShouldSwitch = true,
                TargetHkl = bestTargetHkl,
                ReplaceKeyCount = replaceCount,
                KeysToRetype = retypeList,
                OriginalWordKey = currentWord.ToLowerInvariant()
            };
        }
    }
}
