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
using dotSwitcher.UI;
using dotSwitcher.WinApi;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace dotSwitcher
{
    static class Program
    {
        static Mutex mutex = new Mutex(true, "{D09B5AC3-1219-4987-88A3-7C80075A571B}");
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                if (args[0] == "--create-task")
                {
                    Environment.ExitCode = LowLevelAdapter.CreateAutorunTask() ? 0 : 1;
                    return;
                }
                if (args[0] == "--remove-task")
                {
                    LowLevelAdapter.DeleteAutorunTask();
                    Environment.ExitCode = 0;
                    return;
                }
            }

            bool waitRestart = args != null && args.Length > 0 && args[0] == "--restart-admin";
            bool acquired = false;
            try
            {
                acquired = mutex.WaitOne(waitRestart ? TimeSpan.FromSeconds(5) : TimeSpan.Zero, true);
            }
            catch (AbandonedMutexException)
            {
                acquired = true;
            }

            if (acquired)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var settings = Settings.Init();
                if (LowLevelAdapter.IsRunningAsAdmin() && settings.AutoStart == true && settings.RunAsAdmin == true)
                {
                    // Ensure scheduled task points to current executable path
                    LowLevelAdapter.ConfigureAutorun(true, true);
                }

                var engine = new SwitcherCore(settings);
                Application.ApplicationExit += (s, a) => { engine.Dispose(); };
                var app = new SettingsForm(settings, engine);
                app.Exit += (s, e) => Application.Exit();
                var context = new ApplicationContext(app);
                Application.Run(context);
                mutex.ReleaseMutex();
            }
            else
            {
                LowLevelAdapter.SendShowSettingsMessage();
            }
        }
    }
}
