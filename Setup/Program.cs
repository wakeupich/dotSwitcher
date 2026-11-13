using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace dotSwitcher.Setup
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool uninstall = false;
            bool silent = false;
            bool cleanup = false;
            bool noLaunch = false;
            string customDir = null;

            if (args != null)
            {
                foreach (var rawArg in args)
                {
                    if (string.IsNullOrWhiteSpace(rawArg)) continue;
                    string arg = rawArg.Trim();

                    if (arg.Equals("/uninstall", StringComparison.OrdinalIgnoreCase) ||
                        arg.Equals("--uninstall", StringComparison.OrdinalIgnoreCase))
                    {
                        uninstall = true;
                    }
                    else if (arg.Equals("/silent", StringComparison.OrdinalIgnoreCase) ||
                             arg.Equals("/verysilent", StringComparison.OrdinalIgnoreCase) ||
                             arg.Equals("--silent", StringComparison.OrdinalIgnoreCase))
                    {
                        silent = true;
                    }
                    else if (arg.Equals("/cleanup", StringComparison.OrdinalIgnoreCase))
                    {
                        cleanup = true;
                    }
                    else if (arg.Equals("/nolaunch", StringComparison.OrdinalIgnoreCase) ||
                             arg.Equals("/norestart", StringComparison.OrdinalIgnoreCase))
                    {
                        noLaunch = true;
                    }
                    else if (arg.StartsWith("/dir=", StringComparison.OrdinalIgnoreCase))
                    {
                        customDir = arg.Substring(5).Trim('"');
                    }
                }
            }

            if (uninstall)
            {
                if (silent)
                {
                    InstallEngine.Uninstall(false);
                    return;
                }

                RunInteractiveUninstall();
                return;
            }

            if (silent)
            {
                string targetDir = customDir ?? InstallEngine.GetInstalledLocation() ?? InstallEngine.DefaultInstallDir;
                bool autoStart;
                bool runAsAdmin;
                InstallEngine.LoadExistingSettingsFlags(out autoStart, out runAsAdmin);

                InstallEngine.Install(new InstallOptions
                {
                    InstallDir = targetDir,
                    AutoStart = autoStart,
                    RunAsAdmin = runAsAdmin,
                    CreateStartMenuShortcut = true,
                    CreateDesktopShortcut = false,
                    LaunchAfterInstall = !noLaunch,
                    Silent = true,
                    CleanupSelfAfterExit = cleanup,
                    MarkAsUpdated = true
                });
                return;
            }

            Application.Run(new SetupForm());
        }

        private static void RunInteractiveUninstall()
        {
            bool isRu = string.Equals(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, "ru", StringComparison.OrdinalIgnoreCase);

            using (var form = new Form())
            {
                form.Text = isRu ? "Удаление dotSwitcher" : "Uninstall dotSwitcher";
                form.ClientSize = new Size(380, 145);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.Font = new Font("Segoe UI", 9F);

                try
                {
                    form.Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
                }
                catch { }

                var lbl = new Label
                {
                    Text = isRu ? "Вы уверены, что хотите удалить dotSwitcher?" : "Are you sure you want to uninstall dotSwitcher?",
                    Location = new Point(16, 18),
                    AutoSize = true
                };

                var chkSettings = new CheckBox
                {
                    Text = isRu ? "Удалить настройки пользователя (%LOCALAPPDATA%\\dotSwitcher)" : "Also delete user settings (%LOCALAPPDATA%\\dotSwitcher)",
                    Location = new Point(16, 50),
                    AutoSize = true,
                    Checked = false
                };

                var btnYes = new Button
                {
                    Text = isRu ? "Удалить" : "Uninstall",
                    DialogResult = DialogResult.Yes,
                    Location = new Point(192, 98),
                    Size = new Size(86, 27)
                };

                var btnCancel = new Button
                {
                    Text = isRu ? "Отмена" : "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(284, 98),
                    Size = new Size(80, 27)
                };

                form.AcceptButton = btnYes;
                form.CancelButton = btnCancel;
                form.Controls.AddRange(new Control[] { lbl, chkSettings, btnYes, btnCancel });

                if (form.ShowDialog() == DialogResult.Yes)
                {
                    InstallEngine.Uninstall(chkSettings.Checked);
                    MessageBox.Show(
                        isRu ? "dotSwitcher успешно удалён." : "dotSwitcher has been uninstalled.",
                        isRu ? "Удаление dotSwitcher" : "Uninstall dotSwitcher",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }
    }
}
