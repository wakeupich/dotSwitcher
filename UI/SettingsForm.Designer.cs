namespace dotSwitcher.UI
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.buttonCancelSettings = new System.Windows.Forms.Button();
            this.buttonSaveSettings = new System.Windows.Forms.Button();
            this.buttonExit = new System.Windows.Forms.Button();
            this.buttonGithub = new System.Windows.Forms.Button();
            this.groupGeneral = new System.Windows.Forms.GroupBox();
            this.buttonCheckUpdate = new System.Windows.Forms.Button();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.checkBoxAutoUpdate = new System.Windows.Forms.CheckBox();
            this.checkBoxTrayIcon = new System.Windows.Forms.CheckBox();
            this.checkBoxRunAsAdmin = new System.Windows.Forms.CheckBox();
            this.checkBoxAutorun = new System.Windows.Forms.CheckBox();
            this.groupBehavior = new System.Windows.Forms.GroupBox();
            this.labelDelayHint = new System.Windows.Forms.Label();
            this.textBoxDelay = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.labelSmartSelectionHelp = new System.Windows.Forms.Label();
            this.checkBoxSmartSelection = new System.Windows.Forms.CheckBox();
            this.labelAutoSwitchHelp = new System.Windows.Forms.Label();
            this.checkBoxAutoSwitch = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxSwitchHotkey = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxConvertHotkey = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxSwitchLayoutHotkey = new System.Windows.Forms.TextBox();
            this.groupBoxLayouts = new System.Windows.Forms.GroupBox();
            this.radioAllLayouts = new System.Windows.Forms.RadioButton();
            this.radioSelectedLayouts = new System.Windows.Forms.RadioButton();
            this.checkedListBoxLayouts = new System.Windows.Forms.CheckedListBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupGeneral.SuspendLayout();
            this.groupBehavior.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBoxLayouts.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupGeneral
            // 
            this.groupGeneral.Controls.Add(this.buttonCheckUpdate);
            this.groupGeneral.Controls.Add(this.comboBoxLanguage);
            this.groupGeneral.Controls.Add(this.labelLanguage);
            this.groupGeneral.Controls.Add(this.checkBoxAutoUpdate);
            this.groupGeneral.Controls.Add(this.checkBoxTrayIcon);
            this.groupGeneral.Controls.Add(this.checkBoxRunAsAdmin);
            this.groupGeneral.Controls.Add(this.checkBoxAutorun);
            this.groupGeneral.Location = new System.Drawing.Point(12, 12);
            this.groupGeneral.Name = "groupGeneral";
            this.groupGeneral.Size = new System.Drawing.Size(250, 198);
            this.groupGeneral.TabIndex = 0;
            this.groupGeneral.TabStop = false;
            this.groupGeneral.Text = "General & Updates";
            // 
            // checkBoxAutorun
            // 
            this.checkBoxAutorun.AutoSize = true;
            this.checkBoxAutorun.Location = new System.Drawing.Point(12, 22);
            this.checkBoxAutorun.Name = "checkBoxAutorun";
            this.checkBoxAutorun.Size = new System.Drawing.Size(156, 19);
            this.checkBoxAutorun.TabIndex = 0;
            this.checkBoxAutorun.Text = "Start on Windows startup";
            this.checkBoxAutorun.UseVisualStyleBackColor = true;
            this.checkBoxAutorun.CheckedChanged += new System.EventHandler(this.checkBoxAutorun_CheckedChanged);
            // 
            // checkBoxRunAsAdmin
            // 
            this.checkBoxRunAsAdmin.AutoSize = true;
            this.checkBoxRunAsAdmin.Location = new System.Drawing.Point(12, 46);
            this.checkBoxRunAsAdmin.Name = "checkBoxRunAsAdmin";
            this.checkBoxRunAsAdmin.Size = new System.Drawing.Size(184, 19);
            this.checkBoxRunAsAdmin.TabIndex = 1;
            this.checkBoxRunAsAdmin.Text = "Run as admin (Task Scheduler)";
            this.checkBoxRunAsAdmin.UseVisualStyleBackColor = true;
            this.checkBoxRunAsAdmin.CheckedChanged += new System.EventHandler(this.checkBoxRunAsAdmin_CheckedChanged);
            // 
            // checkBoxTrayIcon
            // 
            this.checkBoxTrayIcon.AutoSize = true;
            this.checkBoxTrayIcon.Location = new System.Drawing.Point(12, 70);
            this.checkBoxTrayIcon.Name = "checkBoxTrayIcon";
            this.checkBoxTrayIcon.Size = new System.Drawing.Size(102, 19);
            this.checkBoxTrayIcon.TabIndex = 2;
            this.checkBoxTrayIcon.Text = "Show tray icon";
            this.checkBoxTrayIcon.UseVisualStyleBackColor = true;
            this.checkBoxTrayIcon.CheckedChanged += new System.EventHandler(this.checkBoxTrayIcon_CheckedChanged);
            // 
            // checkBoxAutoUpdate
            // 
            this.checkBoxAutoUpdate.AutoSize = true;
            this.checkBoxAutoUpdate.Checked = true;
            this.checkBoxAutoUpdate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoUpdate.Location = new System.Drawing.Point(12, 94);
            this.checkBoxAutoUpdate.Name = "checkBoxAutoUpdate";
            this.checkBoxAutoUpdate.Size = new System.Drawing.Size(161, 19);
            this.checkBoxAutoUpdate.TabIndex = 3;
            this.checkBoxAutoUpdate.Text = "Auto-update from GitHub";
            this.checkBoxAutoUpdate.UseVisualStyleBackColor = true;
            this.checkBoxAutoUpdate.CheckedChanged += new System.EventHandler(this.checkBoxAutoUpdate_CheckedChanged);
            // 
            // labelLanguage
            // 
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.Location = new System.Drawing.Point(12, 125);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(62, 15);
            this.labelLanguage.TabIndex = 4;
            this.labelLanguage.Text = "Language:";
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Location = new System.Drawing.Point(96, 122);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(142, 23);
            this.comboBoxLanguage.TabIndex = 5;
            this.comboBoxLanguage.SelectedIndexChanged += new System.EventHandler(this.comboBoxLanguage_SelectedIndexChanged);
            // 
            // buttonCheckUpdate
            // 
            this.buttonCheckUpdate.Location = new System.Drawing.Point(12, 157);
            this.buttonCheckUpdate.Name = "buttonCheckUpdate";
            this.buttonCheckUpdate.Size = new System.Drawing.Size(226, 27);
            this.buttonCheckUpdate.TabIndex = 6;
            this.buttonCheckUpdate.Text = "Check for updates";
            this.buttonCheckUpdate.UseVisualStyleBackColor = true;
            this.buttonCheckUpdate.Click += new System.EventHandler(this.buttonCheckUpdate_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBoxSwitchHotkey);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBoxConvertHotkey);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textBoxSwitchLayoutHotkey);
            this.groupBox1.Location = new System.Drawing.Point(272, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(250, 198);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Hotkeys";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Convert last word:";
            // 
            // textBoxSwitchHotkey
            // 
            this.textBoxSwitchHotkey.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxSwitchHotkey.Location = new System.Drawing.Point(12, 40);
            this.textBoxSwitchHotkey.Name = "textBoxSwitchHotkey";
            this.textBoxSwitchHotkey.ReadOnly = true;
            this.textBoxSwitchHotkey.Size = new System.Drawing.Size(226, 23);
            this.textBoxSwitchHotkey.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(102, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Convert selection:";
            // 
            // textBoxConvertHotkey
            // 
            this.textBoxConvertHotkey.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxConvertHotkey.Location = new System.Drawing.Point(12, 92);
            this.textBoxConvertHotkey.Name = "textBoxConvertHotkey";
            this.textBoxConvertHotkey.ReadOnly = true;
            this.textBoxConvertHotkey.Size = new System.Drawing.Size(226, 23);
            this.textBoxConvertHotkey.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 126);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(134, 15);
            this.label4.TabIndex = 4;
            this.label4.Text = "Switch keyboard layout:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Cursor = System.Windows.Forms.Cursors.Help;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label5.ForeColor = System.Drawing.Color.SteelBlue;
            this.label5.Location = new System.Drawing.Point(224, 126);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(14, 15);
            this.label5.TabIndex = 5;
            this.label5.Text = "?";
            this.label5.MouseLeave += new System.EventHandler(this.label5_MouseLeave);
            this.label5.MouseHover += new System.EventHandler(this.label5_MouseHover);
            // 
            // textBoxSwitchLayoutHotkey
            // 
            this.textBoxSwitchLayoutHotkey.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxSwitchLayoutHotkey.Location = new System.Drawing.Point(12, 144);
            this.textBoxSwitchLayoutHotkey.Name = "textBoxSwitchLayoutHotkey";
            this.textBoxSwitchLayoutHotkey.ReadOnly = true;
            this.textBoxSwitchLayoutHotkey.Size = new System.Drawing.Size(226, 23);
            this.textBoxSwitchLayoutHotkey.TabIndex = 6;
            // 
            // groupBehavior
            // 
            this.groupBehavior.Controls.Add(this.labelDelayHint);
            this.groupBehavior.Controls.Add(this.textBoxDelay);
            this.groupBehavior.Controls.Add(this.label3);
            this.groupBehavior.Controls.Add(this.labelSmartSelectionHelp);
            this.groupBehavior.Controls.Add(this.checkBoxSmartSelection);
            this.groupBehavior.Controls.Add(this.labelAutoSwitchHelp);
            this.groupBehavior.Controls.Add(this.checkBoxAutoSwitch);
            this.groupBehavior.Location = new System.Drawing.Point(12, 218);
            this.groupBehavior.Name = "groupBehavior";
            this.groupBehavior.Size = new System.Drawing.Size(250, 160);
            this.groupBehavior.TabIndex = 2;
            this.groupBehavior.TabStop = false;
            this.groupBehavior.Text = "Typing & Switching";
            // 
            // checkBoxAutoSwitch
            // 
            this.checkBoxAutoSwitch.AutoSize = true;
            this.checkBoxAutoSwitch.Checked = true;
            this.checkBoxAutoSwitch.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoSwitch.Location = new System.Drawing.Point(12, 24);
            this.checkBoxAutoSwitch.Name = "checkBoxAutoSwitch";
            this.checkBoxAutoSwitch.Size = new System.Drawing.Size(188, 19);
            this.checkBoxAutoSwitch.TabIndex = 0;
            this.checkBoxAutoSwitch.Text = "Auto-switch layout while typing";
            this.checkBoxAutoSwitch.UseVisualStyleBackColor = true;
            this.checkBoxAutoSwitch.CheckedChanged += new System.EventHandler(this.checkBoxAutoSwitch_CheckedChanged);
            // 
            // labelAutoSwitchHelp
            // 
            this.labelAutoSwitchHelp.AutoSize = true;
            this.labelAutoSwitchHelp.Cursor = System.Windows.Forms.Cursors.Help;
            this.labelAutoSwitchHelp.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelAutoSwitchHelp.ForeColor = System.Drawing.Color.SteelBlue;
            this.labelAutoSwitchHelp.Location = new System.Drawing.Point(224, 25);
            this.labelAutoSwitchHelp.Name = "labelAutoSwitchHelp";
            this.labelAutoSwitchHelp.Size = new System.Drawing.Size(14, 15);
            this.labelAutoSwitchHelp.TabIndex = 1;
            this.labelAutoSwitchHelp.Text = "?";
            this.labelAutoSwitchHelp.MouseLeave += new System.EventHandler(this.labelAutoSwitchHelp_MouseLeave);
            this.labelAutoSwitchHelp.MouseHover += new System.EventHandler(this.labelAutoSwitchHelp_MouseHover);
            // 
            // checkBoxSmartSelection
            // 
            this.checkBoxSmartSelection.AutoSize = true;
            this.checkBoxSmartSelection.Checked = true;
            this.checkBoxSmartSelection.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSmartSelection.Location = new System.Drawing.Point(12, 50);
            this.checkBoxSmartSelection.Name = "checkBoxSmartSelection";
            this.checkBoxSmartSelection.Size = new System.Drawing.Size(166, 19);
            this.checkBoxSmartSelection.TabIndex = 2;
            this.checkBoxSmartSelection.Text = "Use smart convert selection";
            this.checkBoxSmartSelection.UseVisualStyleBackColor = true;
            this.checkBoxSmartSelection.CheckedChanged += new System.EventHandler(this.smartSelection_CheckedChanged);
            // 
            // labelSmartSelectionHelp
            // 
            this.labelSmartSelectionHelp.AutoSize = true;
            this.labelSmartSelectionHelp.Cursor = System.Windows.Forms.Cursors.Help;
            this.labelSmartSelectionHelp.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelSmartSelectionHelp.ForeColor = System.Drawing.Color.SteelBlue;
            this.labelSmartSelectionHelp.Location = new System.Drawing.Point(224, 51);
            this.labelSmartSelectionHelp.Name = "labelSmartSelectionHelp";
            this.labelSmartSelectionHelp.Size = new System.Drawing.Size(14, 15);
            this.labelSmartSelectionHelp.TabIndex = 3;
            this.labelSmartSelectionHelp.Text = "?";
            this.labelSmartSelectionHelp.MouseLeave += new System.EventHandler(this.labelSmartSelectionHelp_MouseLeave);
            this.labelSmartSelectionHelp.MouseHover += new System.EventHandler(this.labelSmartSelectionHelp_MouseHover);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(130, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Delay before switching:";
            // 
            // textBoxDelay
            // 
            this.textBoxDelay.Location = new System.Drawing.Point(12, 104);
            this.textBoxDelay.Name = "textBoxDelay";
            this.textBoxDelay.Size = new System.Drawing.Size(70, 23);
            this.textBoxDelay.TabIndex = 5;
            this.textBoxDelay.TextChanged += new System.EventHandler(this.textBoxDelay_TextChanged);
            // 
            // labelDelayHint
            // 
            this.labelDelayHint.AutoSize = true;
            this.labelDelayHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelDelayHint.Location = new System.Drawing.Point(88, 107);
            this.labelDelayHint.Name = "labelDelayHint";
            this.labelDelayHint.Size = new System.Drawing.Size(64, 15);
            this.labelDelayHint.TabIndex = 6;
            this.labelDelayHint.Text = "(1–200 ms)";
            // 
            // groupBoxLayouts
            // 
            this.groupBoxLayouts.Controls.Add(this.radioAllLayouts);
            this.groupBoxLayouts.Controls.Add(this.radioSelectedLayouts);
            this.groupBoxLayouts.Controls.Add(this.checkedListBoxLayouts);
            this.groupBoxLayouts.Location = new System.Drawing.Point(272, 218);
            this.groupBoxLayouts.Name = "groupBoxLayouts";
            this.groupBoxLayouts.Size = new System.Drawing.Size(250, 160);
            this.groupBoxLayouts.TabIndex = 3;
            this.groupBoxLayouts.TabStop = false;
            this.groupBoxLayouts.Text = "Keyboard layouts in cycle";
            // 
            // radioAllLayouts
            // 
            this.radioAllLayouts.AutoSize = true;
            this.radioAllLayouts.Checked = true;
            this.radioAllLayouts.Location = new System.Drawing.Point(12, 22);
            this.radioAllLayouts.Name = "radioAllLayouts";
            this.radioAllLayouts.Size = new System.Drawing.Size(87, 19);
            this.radioAllLayouts.TabIndex = 0;
            this.radioAllLayouts.TabStop = true;
            this.radioAllLayouts.Text = "All installed";
            this.radioAllLayouts.UseVisualStyleBackColor = true;
            this.radioAllLayouts.CheckedChanged += new System.EventHandler(this.radioLayouts_CheckedChanged);
            // 
            // radioSelectedLayouts
            // 
            this.radioSelectedLayouts.AutoSize = true;
            this.radioSelectedLayouts.Location = new System.Drawing.Point(116, 22);
            this.radioSelectedLayouts.Name = "radioSelectedLayouts";
            this.radioSelectedLayouts.Size = new System.Drawing.Size(98, 19);
            this.radioSelectedLayouts.TabIndex = 1;
            this.radioSelectedLayouts.Text = "Only selected:";
            this.radioSelectedLayouts.UseVisualStyleBackColor = true;
            this.radioSelectedLayouts.CheckedChanged += new System.EventHandler(this.radioLayouts_CheckedChanged);
            // 
            // checkedListBoxLayouts
            // 
            this.checkedListBoxLayouts.CheckOnClick = true;
            this.checkedListBoxLayouts.FormattingEnabled = true;
            this.checkedListBoxLayouts.IntegralHeight = false;
            this.checkedListBoxLayouts.Location = new System.Drawing.Point(12, 46);
            this.checkedListBoxLayouts.Name = "checkedListBoxLayouts";
            this.checkedListBoxLayouts.Size = new System.Drawing.Size(226, 102);
            this.checkedListBoxLayouts.TabIndex = 2;
            this.checkedListBoxLayouts.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBoxLayouts_ItemCheck);
            // 
            // buttonExit
            // 
            this.buttonExit.Location = new System.Drawing.Point(12, 390);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(88, 27);
            this.buttonExit.TabIndex = 4;
            this.buttonExit.Text = "Exit";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // buttonGithub
            // 
            this.buttonGithub.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonGithub.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonGithub.Location = new System.Drawing.Point(106, 390);
            this.buttonGithub.Name = "buttonGithub";
            this.buttonGithub.Size = new System.Drawing.Size(156, 27);
            this.buttonGithub.TabIndex = 5;
            this.buttonGithub.Text = "Report an issue";
            this.buttonGithub.UseVisualStyleBackColor = true;
            this.buttonGithub.Click += new System.EventHandler(this.buttonGithub_Click);
            // 
            // buttonSaveSettings
            // 
            this.buttonSaveSettings.Location = new System.Drawing.Point(344, 390);
            this.buttonSaveSettings.Name = "buttonSaveSettings";
            this.buttonSaveSettings.Size = new System.Drawing.Size(86, 27);
            this.buttonSaveSettings.TabIndex = 6;
            this.buttonSaveSettings.Text = "Apply";
            this.buttonSaveSettings.UseVisualStyleBackColor = true;
            this.buttonSaveSettings.Click += new System.EventHandler(this.buttonSaveSettings_Click);
            // 
            // buttonCancelSettings
            // 
            this.buttonCancelSettings.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancelSettings.Location = new System.Drawing.Point(436, 390);
            this.buttonCancelSettings.Name = "buttonCancelSettings";
            this.buttonCancelSettings.Size = new System.Drawing.Size(86, 27);
            this.buttonCancelSettings.TabIndex = 7;
            this.buttonCancelSettings.Text = "Cancel";
            this.buttonCancelSettings.UseVisualStyleBackColor = true;
            this.buttonCancelSettings.Click += new System.EventHandler(this.buttonCancelSettings_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 7000;
            this.toolTip1.InitialDelay = 100;
            this.toolTip1.ReshowDelay = 100;
            this.toolTip1.Popup += new System.Windows.Forms.PopupEventHandler(this.toolTip1_Popup);
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.buttonSaveSettings;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancelSettings;
            this.ClientSize = new System.Drawing.Size(534, 429);
            this.Controls.Add(this.groupGeneral);
            this.Controls.Add(this.groupBehavior);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBoxLayouts);
            this.Controls.Add(this.buttonExit);
            this.Controls.Add(this.buttonGithub);
            this.Controls.Add(this.buttonSaveSettings);
            this.Controls.Add(this.buttonCancelSettings);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "dotSwitcher Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.Shown += new System.EventHandler(this.SettingsForm_Shown);
            this.groupGeneral.ResumeLayout(false);
            this.groupGeneral.PerformLayout();
            this.groupBehavior.ResumeLayout(false);
            this.groupBehavior.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxLayouts.ResumeLayout(false);
            this.groupBoxLayouts.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button buttonCancelSettings;
        private System.Windows.Forms.Button buttonSaveSettings;
        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.Button buttonGithub;
        private System.Windows.Forms.GroupBox groupGeneral;
        private System.Windows.Forms.Button buttonCheckUpdate;
        private System.Windows.Forms.ComboBox comboBoxLanguage;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.CheckBox checkBoxAutoUpdate;
        private System.Windows.Forms.CheckBox checkBoxTrayIcon;
        private System.Windows.Forms.CheckBox checkBoxRunAsAdmin;
        private System.Windows.Forms.CheckBox checkBoxAutorun;
        private System.Windows.Forms.GroupBox groupBehavior;
        private System.Windows.Forms.Label labelDelayHint;
        private System.Windows.Forms.TextBox textBoxDelay;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelSmartSelectionHelp;
        private System.Windows.Forms.CheckBox checkBoxSmartSelection;
        private System.Windows.Forms.Label labelAutoSwitchHelp;
        private System.Windows.Forms.CheckBox checkBoxAutoSwitch;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxSwitchHotkey;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxConvertHotkey;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxSwitchLayoutHotkey;
        private System.Windows.Forms.GroupBox groupBoxLayouts;
        private System.Windows.Forms.RadioButton radioAllLayouts;
        private System.Windows.Forms.RadioButton radioSelectedLayouts;
        private System.Windows.Forms.CheckedListBox checkedListBoxLayouts;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
