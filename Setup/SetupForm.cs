using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace dotSwitcher.Setup
{
    public class SetupForm : Form
    {
        private static readonly bool IsRu = string.Equals(
            CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
            "ru",
            StringComparison.OrdinalIgnoreCase);

        private TextBox textInstallDir;
        private Button buttonBrowse;
        private CheckBox checkAutorun;
        private CheckBox checkRunAsAdmin;
        private CheckBox checkStartMenu;
        private CheckBox checkDesktop;
        private CheckBox checkLaunch;
        private Button buttonInstall;
        private Button buttonCancel;
        private Label labelTitle;
        private Label labelSubtitle;
        private Label labelPath;

        public SetupForm()
        {
            InitializeUi();
        }

        private void InitializeUi()
        {
            string version = InstallEngine.GetSetupVersion();
            string existingDir = InstallEngine.GetInstalledLocation();
            bool isUpdate = InstallEngine.IsAlreadyInstalled(existingDir ?? InstallEngine.DefaultInstallDir);

            bool autoStart;
            bool runAsAdmin;
            InstallEngine.LoadExistingSettingsFlags(out autoStart, out runAsAdmin);

            this.Text = IsRu
                ? string.Format("Установка dotSwitcher v{0}", version)
                : string.Format("dotSwitcher v{0} Setup", version);
            this.ClientSize = new Size(460, 305);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F);

            try
            {
                this.Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            catch { }

            labelTitle = new Label
            {
                Text = string.Format("dotSwitcher v{0}", version),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(16, 14),
                AutoSize = true
            };

            labelSubtitle = new Label
            {
                Text = isUpdate
                    ? (IsRu ? "Найдена установленная версия. Настройки в %LOCALAPPDATA%\\dotSwitcher будут сохранены." : "Existing installation detected. Settings in %LOCALAPPDATA%\\dotSwitcher will be preserved.")
                    : (IsRu ? "Автоматический и горячий переключатель раскладки для Windows." : "Automatic & hotkey keyboard layout switcher for Windows."),
                Location = new Point(18, 42),
                Size = new Size(424, 20)
            };

            labelPath = new Label
            {
                Text = IsRu ? "Папка установки:" : "Install location:",
                Location = new Point(16, 72),
                AutoSize = true
            };

            textInstallDir = new TextBox
            {
                Text = !string.IsNullOrWhiteSpace(existingDir) ? existingDir : InstallEngine.DefaultInstallDir,
                Location = new Point(18, 92),
                Size = new Size(340, 23)
            };

            buttonBrowse = new Button
            {
                Text = IsRu ? "Обзор..." : "Browse...",
                Location = new Point(364, 91),
                Size = new Size(80, 25)
            };
            buttonBrowse.Click += ButtonBrowse_Click;

            checkAutorun = new CheckBox
            {
                Text = IsRu ? "Запускать вместе с Windows" : "Start on Windows startup",
                Checked = autoStart,
                Location = new Point(18, 128),
                AutoSize = true
            };

            checkRunAsAdmin = new CheckBox
            {
                Text = IsRu ? "Запуск от администратора (для работы во всех программах и играх)" : "Run as administrator (Task Scheduler — works in elevated apps & games)",
                Checked = runAsAdmin,
                Location = new Point(18, 152),
                AutoSize = true
            };
            checkAutorun.CheckedChanged += (s, e) => { checkRunAsAdmin.Enabled = checkAutorun.Checked; };
            checkRunAsAdmin.Enabled = checkAutorun.Checked;

            checkStartMenu = new CheckBox
            {
                Text = IsRu ? "Создать ярлыки в меню «Пуск»" : "Create Start Menu shortcuts",
                Checked = true,
                Location = new Point(18, 176),
                AutoSize = true
            };

            checkDesktop = new CheckBox
            {
                Text = IsRu ? "Создать ярлык на рабочем столе" : "Create Desktop shortcut",
                Checked = false,
                Location = new Point(18, 200),
                AutoSize = true
            };

            checkLaunch = new CheckBox
            {
                Text = IsRu ? "Запустить dotSwitcher после установки" : "Launch dotSwitcher after installation",
                Checked = true,
                Location = new Point(18, 224),
                AutoSize = true
            };

            buttonInstall = new Button
            {
                Text = isUpdate ? (IsRu ? "Обновить" : "Update") : (IsRu ? "Установить" : "Install"),
                Location = new Point(268, 264),
                Size = new Size(88, 27)
            };
            buttonInstall.Click += ButtonInstall_Click;

            buttonCancel = new Button
            {
                Text = IsRu ? "Отмена" : "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(362, 264),
                Size = new Size(82, 27)
            };

            this.AcceptButton = buttonInstall;
            this.CancelButton = buttonCancel;

            this.Controls.AddRange(new Control[]
            {
                labelTitle,
                labelSubtitle,
                labelPath,
                textInstallDir,
                buttonBrowse,
                checkAutorun,
                checkRunAsAdmin,
                checkStartMenu,
                checkDesktop,
                checkLaunch,
                buttonInstall,
                buttonCancel
            });
        }

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select folder to install dotSwitcher:";
                if (Directory.Exists(textInstallDir.Text))
                {
                    dlg.SelectedPath = textInstallDir.Text;
                }
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    string selected = dlg.SelectedPath;
                    if (!selected.EndsWith("dotSwitcher", StringComparison.OrdinalIgnoreCase))
                    {
                        selected = Path.Combine(selected, "dotSwitcher");
                    }
                    textInstallDir.Text = selected;
                }
            }
        }

        private void ButtonInstall_Click(object sender, EventArgs e)
        {
            string dir = textInstallDir.Text.Trim();
            if (string.IsNullOrEmpty(dir))
            {
                MessageBox.Show(
                    this,
                    IsRu ? "Укажите папку установки." : "Please specify an installation directory.",
                    "dotSwitcher Setup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string prevBtnText = buttonInstall.Text;
            buttonInstall.Enabled = false;
            buttonCancel.Enabled = false;
            buttonInstall.Text = IsRu ? "Установка..." : "Installing...";
            Application.DoEvents();

            try
            {
                InstallEngine.Install(new InstallOptions
                {
                    InstallDir = dir,
                    AutoStart = checkAutorun.Checked,
                    RunAsAdmin = checkRunAsAdmin.Checked,
                    CreateStartMenuShortcut = checkStartMenu.Checked,
                    CreateDesktopShortcut = checkDesktop.Checked,
                    LaunchAfterInstall = checkLaunch.Checked,
                    Silent = false,
                    CleanupSelfAfterExit = false,
                    MarkAsUpdated = false
                });

                MessageBox.Show(
                    this,
                    IsRu ? "dotSwitcher успешно установлен." : "dotSwitcher has been successfully installed.",
                    "dotSwitcher Setup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                buttonInstall.Enabled = true;
                buttonCancel.Enabled = true;
                buttonInstall.Text = prevBtnText;
                MessageBox.Show(
                    this,
                    (IsRu ? "Ошибка установки:\n" : "Installation failed:\n") + ex.Message,
                    "dotSwitcher Setup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
